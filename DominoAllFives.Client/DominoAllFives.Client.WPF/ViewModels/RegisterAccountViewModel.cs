using System;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Presentation retained for a future remote use case implementation.</summary>
    public class RegisterAccountViewModel : ViewModelBase
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsUsernameRequiredVisible { get; set; }
        public bool IsEmailRequiredVisible { get; set; }
        public bool IsPasswordRequiredVisible { get; set; }
        public bool IsConfirmPasswordRequiredVisible { get; set; }
        public RelayCommand RegisterCommand { get; }
        public RelayCommand CancelCommand { get; }

        public RegisterAccountViewModel(Action onCancel = null)
        {
            RegisterCommand = new RelayCommand(() =>
            {
            }, () => false);
            CancelCommand = new RelayCommand(() => onCancel?.Invoke());
        }
    }
}
