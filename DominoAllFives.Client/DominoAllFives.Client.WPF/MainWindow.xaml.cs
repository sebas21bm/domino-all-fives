using System.Windows;

using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Local;
using DominoAllFives.Client.WPF.Services.ProfilePicture;
using DominoAllFives.Client.WPF.ViewModels;
using DominoAllFives.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            FrameNavigationService navigationService =
                new FrameNavigationService(mainFrame);

            PlayerSession playerSession =
                new PlayerSession();

            MainWindowViewModel mainWindowViewModel =
                new MainWindowViewModel(navigationService);

            ILoggerFactory loggerFactory =
                LoggerFactoryProvider.Instance;

            ProfilePictureImageService profilePictureImageService =
                new ProfilePictureImageService(
                    loggerFactory.CreateLogger<ProfilePictureImageService>());

            ProfilePictureFileService profilePictureFileService =
                new ProfilePictureFileService(
                    loggerFactory.CreateLogger<ProfilePictureFileService>());

            /*
             * Initialize controllers and services temporarily for local testing,
             * these will be replaced with remote implementations later.
             */

            ProfilePictureStorage profilePictureStorage =
                new ProfilePictureStorage(
                    loggerFactory.CreateLogger<ProfilePictureStorage>());

            AuthenticationController authenticationController =
                new AuthenticationController();

            RegistrationController registrationController =
                new RegistrationController(
                    loggerFactory.CreateLogger<RegistrationController>());

            ProfilePictureController profilePictureController =
                new ProfilePictureController(
                    profilePictureStorage,
                    loggerFactory.CreateLogger<ProfilePictureController>());

            RankingController rankingController =
                new RankingController(
                    profilePictureStorage);

            ProfileController profileController =
                new ProfileController(
                    profilePictureStorage,
                    loggerFactory.CreateLogger<ProfileController>());

            IAccountService accountService =
                new LocalAccountService(
                    authenticationController,
                    registrationController,
                    profilePictureController,
                    profileController);

            IRankingService rankingService =
                new LocalRankingService(
                    rankingController);

            ViewModelFactory viewModelFactory =
                new ViewModelFactory(
                    navigationService,
                    mainWindowViewModel.DialogService,
                    accountService,
                    rankingService,
                    playerSession,
                    loggerFactory,
                    profilePictureImageService,
                    profilePictureFileService);

            navigationService.SetViewModelFactory(viewModelFactory);

            DataContext = mainWindowViewModel;

            navigationService.NavigateTo<HomePageViewModel>();
        }
    }
}