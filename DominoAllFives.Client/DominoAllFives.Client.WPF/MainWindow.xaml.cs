using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels;
using System.Windows;

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

            ViewModelFactory viewModelFactory =
                new ViewModelFactory(
                    navigationService,
                    playerSession);

            navigationService.SetViewModelFactory(viewModelFactory);

            DataContext = new MainWindowViewModel(navigationService);

            navigationService.NavigateTo<HomePageViewModel>();
        }
    }
}