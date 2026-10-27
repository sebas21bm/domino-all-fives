using System.Windows;

using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Local;
using DominoAllFives.Client.WPF.Services.Wcf;
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

            // Login and ranking use WCF. Other account operations remain local until migrated.
            // Lazy construction keeps persistence controllers out of both remote call paths.
            IAccountService accountService = new LegacyAccountServiceAdapter(() =>
            {
                ProfilePictureStorage storage = new ProfilePictureStorage(
                    loggerFactory.CreateLogger<ProfilePictureStorage>());
                return new LocalAccountService(
                    new RegistrationController(
                        loggerFactory.CreateLogger<RegistrationController>()),
                    new ProfilePictureController(storage,
                        loggerFactory.CreateLogger<ProfilePictureController>()),
                    new ProfileController(storage,
                        loggerFactory.CreateLogger<ProfileController>()),
                    new AccountController(
                        loggerFactory.CreateLogger<AccountController>()),
                    new AccountLanguageController(
                        loggerFactory.CreateLogger<AccountLanguageController>()),
                    playerSession);
            });

            IRankingClient rankingService = new WcfRankingService();
            ViewModelFactory viewModelFactory =
                new ViewModelFactory(
                    navigationService,
                    mainWindowViewModel.DialogService,
                    accountService,
                    new WcfLoginService(),
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
