using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Views;
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

namespace DominoAllFives.Client.WPF.Controls
{


    /// <summary>
    /// Lógica de interacción para RegisterAccount.xaml
    /// </summary>
    public partial class RegisterAccount : UserControl
    {

        private readonly Action _onCancel;
        private readonly Action _onRegisterSuccess;

        public RegisterAccount(Action onCancel, Action onRegisterSuccess)
        {
            _onCancel = onCancel;
            _onRegisterSuccess = onRegisterSuccess;
            InitializeComponent();
        }
        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            _onCancel.Invoke();
        }
        private void RegisterButtonClick(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is IModalNavigator navigator)
            {
                navigator.OpenModal(new VerifyEmail(
                    onCancel: () => navigator.CloseModal(),
                    onVerifySuccess: () =>
                    {
                        Action goToMainMenu = () =>
                        {
                            MainMenu mainMenu = new MainMenu();
                            mainMenu.Show();
                            Window.GetWindow(this)?.Close();
                        };

                        navigator.OpenModal(new UploadProfilePicture(
                            onBack: () => navigator.CloseModal(),
                            onAddPhoto: goToMainMenu,
                            onSkipPhoto: goToMainMenu
                        ));
                    }
                ));
            }
        }
    }
}
