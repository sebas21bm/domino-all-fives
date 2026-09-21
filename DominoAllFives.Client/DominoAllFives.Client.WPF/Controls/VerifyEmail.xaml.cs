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
    /// Lógica de interacción para VerifyEmail.xaml
    /// </summary>
    public partial class VerifyEmail : UserControl
    {
        private readonly Action _onCancel;
        private readonly Action _onVerifySuccess;

        public VerifyEmail(Action onCancel, Action onVerifySuccess)
        {
            _onCancel = onCancel;
            _onVerifySuccess = onVerifySuccess;
            InitializeComponent();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            _onCancel.Invoke();
        }

        private void VerifyCodeButtonClick(object sender, RoutedEventArgs e)
        {
            _onVerifySuccess.Invoke();
        }
    }
}
