using System;
using System.Windows;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Presentation retained for a future remote use case implementation.</summary>
    public class ChangePasswordViewModel : ViewModelBase
    {
        public Visibility IsCurrentPasswordVisible { get; set; }
        public bool IsCurrentPasswordRequiredVisible { get; set; }
        public bool IsNewPasswordRequiredVisible { get; set; }
        public bool IsConfirmPasswordRequiredVisible { get; set; }
        public RelayCommand ChangePasswordCommand { get; }
        public RelayCommand CancelCommand { get; }

        public ChangePasswordViewModel(Action onCancel = null)
        {
            ChangePasswordCommand = new RelayCommand(() =>
            {
            }, () => false);
            CancelCommand = new RelayCommand(() => onCancel?.Invoke());
        }
    }
}
