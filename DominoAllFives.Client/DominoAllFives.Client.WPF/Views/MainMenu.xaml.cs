using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DominoAllFives.Client.WPF.Views
{
    /// <summary>
    /// Lógica de interacción para MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Window
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void ExitGameButtonClick(object sender, RoutedEventArgs eventArgs)
        {
            Application.Current.Shutdown();
        }

        private void UserProfileButtonClick(object sender, RoutedEventArgs eventArgs)
        {
            Profile profileWindow = new Profile();
            profileWindow.Show();
            this.Close();
        }

        private void FriendsButtonClick(object sender, RoutedEventArgs eventArgs)
        {
            Friends friendsWindow = new Friends();
            friendsWindow.Show();
            this.Close();
        }

        private void SettingsButtonClick(object sender, RoutedEventArgs eventArgs)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.Show();
            this.Close();
        }
    }
}
