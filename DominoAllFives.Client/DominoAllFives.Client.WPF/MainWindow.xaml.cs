using System;
using System.ComponentModel;
using System.Windows;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.ProfilePicture;
using DominoAllFives.Client.WPF.Services.Threading;
using DominoAllFives.Client.WPF.Services.Wcf.Services;
using DominoAllFives.Client.WPF.ViewModels;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF
{
    public partial class MainWindow : Window
    {
        private readonly ILogger<MainWindow> _logger =
            LoggerFactoryProvider.CreateLogger<MainWindow>();
        private ViewModelFactory _viewModelFactory;

        private bool _closingStarted;
        private bool _closingCompleted;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                InitializeApplication();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while initializing the application.");

                MessageBox.Show(
                    "The application could not be initialized and will close.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Application.Current.Shutdown();
                return;
            }

            Closing += OnWindowClosing;
        }

        private void InitializeApplication()
        {
            FrameNavigationService navigationService = new FrameNavigationService(mainFrame);
            PlayerSession playerSession = new PlayerSession();
            ProfilePictureImageService profilePictureImageService =
                new ProfilePictureImageService(
                    LoggerFactoryProvider.CreateLogger<ProfilePictureImageService>());

            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
            mainWindowViewModel.Initialize(navigationService);

            _viewModelFactory =
                new ViewModelFactory(
                    navigationService,
                    mainWindowViewModel.DialogService,
                    new SessionClient(),
                    new RegistrationClient(),
                    new RankingClient(),
                    new ProfileClient(),
                    new AccountClient(),
                    playerSession,
                    profilePictureImageService);

            navigationService.SetViewModelFactory(_viewModelFactory);

            DataContext = mainWindowViewModel;

            navigationService.NavigateTo<HomePageViewModel>();
        }

        private void OnWindowClosing(object sender, CancelEventArgs eventArgs)
        {
            if (_closingCompleted)
            {
                return;
            }

            eventArgs.Cancel = true;

            if (_closingStarted)
            {
                return;
            }

            _closingStarted = true;

            BackgroundWork.Start(
                new BackgroundJob<object, bool>(
                    DisposeServices,
                    null,
                    CompleteClose,
                    OnDisposeFailed));
        }

        private bool DisposeServices(object state)
        {
            _viewModelFactory.Dispose();
            return true;
        }

        private void CompleteClose(bool result, object state)
        {
            _closingCompleted = true;
            Close();
        }

        private void OnDisposeFailed(
            Exception error,
            object state)
        {
            _logger.LogError(
                error,
                "An error occurred while disposing application services.");

            CompleteClose(false, state);
        }
    }
}