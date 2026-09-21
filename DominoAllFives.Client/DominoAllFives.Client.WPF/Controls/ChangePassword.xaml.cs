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
    /// Lógica de interacción para ChangePassword.xaml
    /// </summary>
    public partial class ChangePassword : UserControl
    {
        private readonly Action _onCancel;
        private readonly Action _onChangeSuccess;

        public ChangePassword(Action onCancel, Action onChangeSuccess)
        {
            _onCancel = onCancel;
            _onChangeSuccess = onChangeSuccess;
            InitializeComponent();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            _onCancel.Invoke();
        }

        private void ChangePasswordButtonClick(object sender, RoutedEventArgs e)
        {
            _onChangeSuccess.Invoke();
        }
    }
}
