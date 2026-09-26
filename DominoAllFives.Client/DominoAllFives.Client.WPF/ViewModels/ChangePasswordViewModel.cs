using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class ChangePasswordViewModel : ViewModelBase
    {
        private readonly Action _onPasswordChangedSuccess;
        private readonly Action _onCancel;

        public RelayCommand ChangePasswordCommand { get; }
        public RelayCommand CancelCommand { get; }

        public ChangePasswordViewModel(
            Action onPasswordChangedSuccess,
            Action onCancel)
        {
            _onPasswordChangedSuccess = onPasswordChangedSuccess ?? throw new ArgumentNullException(nameof(onPasswordChangedSuccess));
            _onCancel = onCancel ?? throw new ArgumentNullException(nameof(onCancel));

            ChangePasswordCommand = new RelayCommand(ExecuteChangePassword);
            CancelCommand = new RelayCommand(_onCancel);
        }

        private void ExecuteChangePassword(object parameter)
        {
            // TODO: Validar formato seguro de contraseña (RV-02) y actualizar en WCF
            _onPasswordChangedSuccess?.Invoke();
        }
    }
}
