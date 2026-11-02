using System;

using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class VerifyEmailViewModel : ViewModelBase
    {
        private readonly Action _onVerificationSuccess;
        private readonly Action _onCancel;

        private string _email;
        private string _verificationCode;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string VerificationCode
        {
            get => _verificationCode;
            set => SetProperty(ref _verificationCode, value);
        }

        public RelayCommand VerifyCodeCommand { get; }
        public RelayCommand ResendCodeCommand { get; }
        public RelayCommand CancelCommand { get; }

        public VerifyEmailViewModel(
            string targetEmail,
            Action onVerificationSuccess,
            Action onCancel)
        {
            _email = targetEmail;
            _onVerificationSuccess = onVerificationSuccess ?? throw new ArgumentNullException(nameof(onVerificationSuccess));
            _onCancel = onCancel ?? throw new ArgumentNullException(nameof(onCancel));

            VerifyCodeCommand = new RelayCommand(ExecuteVerifyCode);
            ResendCodeCommand = new RelayCommand(ExecuteResendCode);
            CancelCommand = new RelayCommand(_onCancel);
        }

        private void ExecuteVerifyCode()
        {
            _onVerificationSuccess?.Invoke();
        }

        private void ExecuteResendCode()
        {

        }
    }
}
