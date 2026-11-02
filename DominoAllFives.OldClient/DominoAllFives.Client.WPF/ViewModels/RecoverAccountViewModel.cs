using System;

using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class RecoverAccountViewModel : ViewModelBase
    {
        private readonly Action<string> _onGoToVerifyEmail;
        private readonly Action _onCancel;

        private string _email;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public RelayCommand SendCodeCommand { get; }
        public RelayCommand CancelCommand { get; }

        public RecoverAccountViewModel(
            Action<string> onGoToVerifyEmail,
            Action onCancel)
        {
            _onGoToVerifyEmail = onGoToVerifyEmail ?? 
                throw new ArgumentNullException(nameof(onGoToVerifyEmail));
            _onCancel = onCancel ?? throw new ArgumentNullException(nameof(onCancel));

            SendCodeCommand = new RelayCommand(ExecuteSendCode);
            CancelCommand = new RelayCommand(_onCancel);
        }

        private void ExecuteSendCode()
        {
            _onGoToVerifyEmail?.Invoke(Email);
        }
    }
}
