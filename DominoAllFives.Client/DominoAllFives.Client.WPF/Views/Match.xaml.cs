using System.Windows;
using System.Windows.Controls;
using DominoAllFives.Client.WPF.ViewModels;

namespace DominoAllFives.Client.WPF.Views
{
    public partial class Match : Page
    {
        public Match()
        {
            InitializeComponent();
        }

        private async void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            MatchViewModel model = DataContext as MatchViewModel;
            if (model != null)
            {
                await model.InitializeAsync();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
        {
            (DataContext as MatchViewModel)?.Dispose();
        }
    }
}
