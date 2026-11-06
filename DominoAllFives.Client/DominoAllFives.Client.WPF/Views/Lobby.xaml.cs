using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DominoAllFives.Client.WPF.ViewModels;

namespace DominoAllFives.Client.WPF.Views
{
    public partial class Lobby : Page
    {
        private NavigationService _navigation;
        public Lobby()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_navigation != null) return;
            _navigation = NavigationService;
            if (_navigation != null) _navigation.Navigated += OnNavigated;
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (ReferenceEquals(e.Content, this)) return;
            _navigation.Navigated -= OnNavigated;
            _navigation = null;
            (DataContext as LobbyViewModel)?.Dispose();
        }
    }
}
