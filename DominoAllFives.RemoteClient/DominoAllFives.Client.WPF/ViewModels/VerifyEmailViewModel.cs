using System;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Presentation retained for a future remote use case implementation.</summary>
    public class VerifyEmailViewModel : ViewModelBase
    {
        public string Email { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
        public RelayCommand VerifyCodeCommand { get; }
        public RelayCommand ResendCodeCommand { get; }
        public RelayCommand CancelCommand { get; }

        public VerifyEmailViewModel(Action onCancel = null)
        {
            VerifyCodeCommand = new RelayCommand(() => { }, () => false);
            ResendCodeCommand = new RelayCommand(() => { }, () => false);
            CancelCommand = new RelayCommand(() => onCancel?.Invoke());
        }
    }
}
