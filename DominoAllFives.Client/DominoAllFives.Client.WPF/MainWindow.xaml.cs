using System.Windows;

using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.Services.Wcf.Services;
using DominoAllFives.Client.WPF.Services.ProfilePicture;
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

            IRankingClient rankingService = new WcfRankingService();
            ViewModelFactory viewModelFactory =
                new ViewModelFactory(
                    navigationService,
                    mainWindowViewModel.DialogService,
                    new WcfLoginService(),
                    rankingService,
                    playerSession,
                    loggerFactory,
                    profilePictureImageService);

            bool closingStarted = false;
            bool closingCompleted = false;
            Closing += (sender, args) =>
            {
                if (closingCompleted) return;
                args.Cancel = true;
                if (closingStarted) return;
                closingStarted = true;
                UiOperationRunner.Run(
                    () => { viewModelFactory.Dispose(); return true; },
                    result => { closingCompleted = true; Close(); },
                    error => { closingCompleted = true; Close(); });
            };

            navigationService.SetViewModelFactory(viewModelFactory);

            DataContext = mainWindowViewModel;

            navigationService.NavigateTo<HomePageViewModel>();
        }
    }
}
