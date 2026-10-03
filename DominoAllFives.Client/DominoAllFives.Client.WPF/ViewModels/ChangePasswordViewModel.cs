using System;
using System.Windows;

using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class ChangePasswordViewModel : ViewModelBase
    {
        private readonly Action _onPasswordChangedSuccess;
        private readonly Action _onCancel;
        private readonly bool _isFromSettings;

        public Visibility IsCurrentPasswordVisible => _isFromSettings ? 
            Visibility.Visible : Visibility.Collapsed;


        public RelayCommand ChangePasswordCommand { get; }
        public RelayCommand CancelCommand { get; }

        public ChangePasswordViewModel(
            Action onPasswordChangedSuccess,
            Action onCancel,
            bool isFromSettings = false)
        {
            _onPasswordChangedSuccess = onPasswordChangedSuccess ?? 
                throw new ArgumentNullException(nameof(onPasswordChangedSuccess));
            _onCancel = onCancel ?? throw new ArgumentNullException(nameof(onCancel));
            _isFromSettings = isFromSettings;

            ChangePasswordCommand = new RelayCommand(ExecuteChangePassword);
            CancelCommand = new RelayCommand(_onCancel);
        }

        private void ExecuteChangePassword(object parameter)
        {
            _onPasswordChangedSuccess?.Invoke();
        }
    }
}
