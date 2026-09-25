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
using DominoAllFives.Client.WPF.Controls;
using DominoAllFives.Client.WPF.Services;

namespace DominoAllFives.Client.WPF.Views
{
    /// <summary>
    /// Lógica de interacción para HomePage.xaml
    /// </summary>
    public partial class HomePage : Page, IModalNavigator
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        public void OpenModal(UserControl view)
        {
            ModalContent.Content = view;
            ModalOverlay.Visibility = Visibility.Visible;
        }
        public void CloseModal()
        {
            ModalContent.Content = null;
            ModalOverlay.Visibility = Visibility.Collapsed;
        }
        private void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            OpenModal(new Login(
                onLoginSuccess: () =>
                {
                    
                }
            ));
        }
    }
}
