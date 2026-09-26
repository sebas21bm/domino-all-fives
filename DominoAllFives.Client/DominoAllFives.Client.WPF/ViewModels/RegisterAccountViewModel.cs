using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class RegisterAccountViewModel : ViewModelBase
    {
        private readonly Action<string> _onGoToVerifyEmail;
        private readonly Action _onCancel;

        private string _username;
        private string _email;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public RelayCommand RegisterCommand { get; }
        public RelayCommand CancelCommand { get; }

        public RegisterAccountViewModel(
            Action<string> onGoToVerifyEmail,
            Action onCancel)
        {
            _onGoToVerifyEmail = onGoToVerifyEmail ?? throw new ArgumentNullException(nameof(onGoToVerifyEmail));
            _onCancel = onCancel ?? throw new ArgumentNullException(nameof(onCancel));

            RegisterCommand = new RelayCommand(ExecuteRegister);
            CancelCommand = new RelayCommand(_onCancel);
        }

        private void ExecuteRegister(object parameter)
        {
            _onGoToVerifyEmail?.Invoke(Email);
        }
    }
}
