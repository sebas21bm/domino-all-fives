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

        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            MatchViewModel model = DataContext as MatchViewModel;
            if (model != null)
            {
                model.Initialize();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
        {
            (DataContext as MatchViewModel)?.Dispose();
        }
    }
}
