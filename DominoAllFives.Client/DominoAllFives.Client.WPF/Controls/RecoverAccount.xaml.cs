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
    /// Lógica de interacción para RecoverAccount.xaml
    /// </summary>
    public partial class RecoverAccount : UserControl
    {
        private readonly Action _onCancel;
        private readonly Action _onSendCodeSuccess;

        public RecoverAccount(Action onCancel, Action onSendCodeSuccess)
        {
            _onCancel = onCancel;
            _onSendCodeSuccess = onSendCodeSuccess;
            InitializeComponent();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            _onCancel.Invoke();
        }

        private void SendCodeButtonClick(object sender, RoutedEventArgs e)
        {
            _onSendCodeSuccess.Invoke();
        }
    }
}
