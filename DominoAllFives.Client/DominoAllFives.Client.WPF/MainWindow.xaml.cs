using System.ComponentModel;
using System.Windows;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.ProfilePicture;
using DominoAllFives.Client.WPF.Services.Threading;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.Services.Wcf.Services;
using DominoAllFives.Client.WPF.ViewModels;
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
            FrameNavigationService navigationService = new FrameNavigationService(mainFrame);
            PlayerSession playerSession = new PlayerSession();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
            mainWindowViewModel.Initialize(navigationService);
            ILoggerFactory loggerFactory = LoggerFactoryProvider.Instance;
            ProfilePictureImageService profilePictureImageService =
                new ProfilePictureImageService(
                loggerFactory.CreateLogger<ProfilePictureImageService>());
            IRankingClient rankingService = new RankingClient();
            ViewModelFactory viewModelFactory =
                new ViewModelFactory(
                navigationService,
                mainWindowViewModel.DialogService,
                new LoginClient(),
                rankingService,
                playerSession,
                loggerFactory,
                profilePictureImageService);
            bool closingStarted = false;
            bool closingCompleted = false;
            Closing += OnWindowClosing;
            void OnWindowClosing(object sender, CancelEventArgs eventArgs)
            {
                if (closingCompleted)
                {
                    return;
                }

                eventArgs.Cancel = true;
                if (closingStarted)
                {
                    return;
                }

                closingStarted = true;
                BackgroundWork.Start(
                    new BackgroundJob<object, bool>(
                    DisposeServices,
                    null,
                    CompleteClose,
                    (
                    error,
                    state) => CompleteClose(
                    false,
                    state)));
            }

            bool DisposeServices(object state)
            {
                viewModelFactory.Dispose();
                return true;
            }

            void CompleteClose(bool result, object state)
            {
                closingCompleted = true;
                Close();
            }

            navigationService.SetViewModelFactory(viewModelFactory);
            DataContext = mainWindowViewModel;
            navigationService.NavigateTo<HomePageViewModel>();
        }
    }
}
