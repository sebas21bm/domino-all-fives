using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly Action _onLoginSuccess;
        private readonly Action _onCancel;
        private readonly Action _onGoToRegister;
        private readonly Action _onGoToRecover;

        private string _email;
        private string _password;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public RelayCommand LoginCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand GoToRegisterCommand { get; }
        public RelayCommand GoToRecoverCommand { get; }

        public LoginViewModel(
            Action onLoginSuccess,
            Action onCancel,
            Action onGoToRegister,
            Action onGoToRecover)
        {
            _onLoginSuccess = onLoginSuccess ?? throw new ArgumentNullException(nameof(onLoginSuccess));
            _onCancel = onCancel ?? throw new ArgumentNullException(nameof(onCancel));
            _onGoToRegister = onGoToRegister ?? throw new ArgumentNullException(nameof(onGoToRegister));
            _onGoToRecover = onGoToRecover ?? throw new ArgumentNullException(nameof(onGoToRecover));

            LoginCommand = new RelayCommand(ExecuteLogin);
            CancelCommand = new RelayCommand(_onCancel);
            GoToRegisterCommand = new RelayCommand(_onGoToRegister);
            GoToRecoverCommand = new RelayCommand(_onGoToRecover);
        }

        private void ExecuteLogin(object parameter)
        {
            if (parameter is PasswordBox passwordBox)
            {
                Password = passwordBox.Password;
            }

            // TODO: Validar credenciales con WCF cuando esté implementado
            _onLoginSuccess?.Invoke();
        }
    }
}
