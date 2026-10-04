using System.Windows;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Local;
using DominoAllFives.Client.WPF.ViewModels;
using DominoAllFives.Contracts.Services;

namespace DominoAllFives.Client.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
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


            // Initialize controllers and services temporarily for local testing,
            // these will be replaced with remote implementations later.
            AuthenticationController authenticationController =
                new AuthenticationController();

            RegistrationController registrationController =
                new RegistrationController();

            ProfilePictureController profilePictureController =
                new ProfilePictureController();

            IAccountService accountService =
                new LocalAccountService(
                    authenticationController,
                    registrationController,
                    profilePictureController);

            ViewModelFactory viewModelFactory =
                new ViewModelFactory(
                    navigationService,
                    mainWindowViewModel.DialogService,
                    accountService,
                    playerSession);

            navigationService.SetViewModelFactory(viewModelFactory);

            DataContext = mainWindowViewModel;

            navigationService.NavigateTo<HomePageViewModel>();
        }
    }
}