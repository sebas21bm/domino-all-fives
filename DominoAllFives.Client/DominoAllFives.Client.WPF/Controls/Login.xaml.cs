using DominoAllFives.Client.WPF.Controls;
using DominoAllFives.Client.WPF.Services;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DominoAllFives.Client.WPF.Views
{
    /// <summary>
    /// Lógica de interacción para Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        private readonly Action _onLoginSuccess;
        public Login(Action onLoginSuccess)
        {
            _onLoginSuccess = onLoginSuccess;
            InitializeComponent();
            InitializeComponent();
        }

        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is IModalNavigator navigator)
            {
                navigator.CloseModal();
            }
        }

        private void CreateAccountButtonClick(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is IModalNavigator navigator)
            {
                navigator.OpenModal(new RegisterAccount(
                    onCancel: () => navigator.OpenModal(new Login(_onLoginSuccess)),
                    onRegisterSuccess: () =>
                    {
                        navigator.CloseModal();
                    }
                ));
            }
        }

        private void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            _onLoginSuccess.Invoke();
        }
    }
}
