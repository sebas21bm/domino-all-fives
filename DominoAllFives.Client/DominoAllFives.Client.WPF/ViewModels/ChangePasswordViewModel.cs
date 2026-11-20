using System;
using System.Windows;
using System.Windows.Controls;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Threading;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.ViewModels.Base;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Handles the password change dialog for an authenticated account.</summary>
    public class ChangePasswordViewModel : ViewModelBase
    {
        private readonly PlayerSession _playerSession;
        private readonly Action _onSuccess;
        private readonly Action _onCancel;
        private IDialogService _dialogService;
        private IAccountClient _accountClient;
        private ILogger<ChangePasswordViewModel> _logger;
        private bool _isBusy;
        private bool _isCurrentPasswordRequiredVisible;
        private bool _isNewPasswordRequiredVisible;
        private bool _isConfirmPasswordRequiredVisible;

        public Visibility IsCurrentPasswordVisible => Visibility.Visible;

        public bool IsCurrentPasswordRequiredVisible
        {
            get => _isCurrentPasswordRequiredVisible;
            private set => SetProperty(ref _isCurrentPasswordRequiredVisible, value);
        }

        public bool IsNewPasswordRequiredVisible
        {
            get => _isNewPasswordRequiredVisible;
            private set => SetProperty(ref _isNewPasswordRequiredVisible, value);
        }

        public bool IsConfirmPasswordRequiredVisible
        {
            get => _isConfirmPasswordRequiredVisible;
            private set => SetProperty(ref _isConfirmPasswordRequiredVisible, value);
        }

        public RelayCommand ChangePasswordCommand { get; }
        public RelayCommand CancelCommand { get; }

        public ChangePasswordViewModel(PlayerSession playerSession, Action onSuccess, Action onCancel)
        {
            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));
            _onSuccess = onSuccess
                ?? throw new ArgumentNullException(nameof(onSuccess));
            _onCancel = onCancel
                ?? throw new ArgumentNullException(nameof(onCancel));
            ChangePasswordCommand = new RelayCommand(ExecuteChangePassword, unused => CanSubmit());
            CancelCommand = new RelayCommand(Cancel, () => !_isBusy);
        }

        public void Initialize(
            IDialogService dialogService,
            IAccountClient accountClient,
            ILogger<ChangePasswordViewModel> logger)
        {
            if (_accountClient != null)
            {
                throw new InvalidOperationException("ChangePasswordViewModel has already been initialized.");
            }

            _dialogService = dialogService
                ?? throw new ArgumentNullException(nameof(dialogService));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _accountClient = accountClient
                ?? throw new ArgumentNullException(nameof(accountClient));
            ChangePasswordCommand.RaiseCanExecuteChanged();
        }

        private bool CanSubmit()
        {
            return _accountClient != null && !_isBusy && _playerSession.HasServerSession;
        }

        private void Cancel()
        {
            if (!_isBusy)
            {
                _onCancel();
            }
        }

        private void ExecuteChangePassword(object parameter)
        {
            if (!CanSubmit())
            {
                return;
            }

            if (!(parameter is PasswordBox[] passwordBoxes) || passwordBoxes.Length != 3
                || passwordBoxes[0] == null || passwordBoxes[1] == null || passwordBoxes[2] == null)
            {
                _logger.LogError("Password boxes were missing for the password change.");
                return;
            }

            string currentPassword = passwordBoxes[0].Password;
            string newPassword = passwordBoxes[1].Password;
            string confirmPassword = passwordBoxes[2].Password;
            IsCurrentPasswordRequiredVisible = string.IsNullOrWhiteSpace(currentPassword);
            IsNewPasswordRequiredVisible = string.IsNullOrWhiteSpace(newPassword);
            IsConfirmPasswordRequiredVisible = string.IsNullOrWhiteSpace(confirmPassword);

            if (IsCurrentPasswordRequiredVisible || IsNewPasswordRequiredVisible
                || IsConfirmPasswordRequiredVisible)
            {
                return;
            }

            if (newPassword != confirmPassword)
            {
                _dialogService.ShowDialog(FailureDialogs.Create(
                    DialogType.Warning,
                    "MessageAccount_msgIncorrectPasswordTitle",
                    "MessageAccount_msgIncorrectPassword"));
                return;
            }

            string sessionKey = _playerSession.SessionKey;
            BackgroundJob<(string SessionKey, string CurrentPassword, string NewPassword), bool> job =
                new BackgroundJob<(string SessionKey, string CurrentPassword, string NewPassword), bool>(
                    request =>
                    {
                        _accountClient.ChangePassword(request.SessionKey, request.CurrentPassword, request.NewPassword);
                        return true;
                    },
                    (sessionKey, currentPassword, newPassword),
                    (completed, request) => Complete(passwordBoxes, request.SessionKey),
                    (error, request) => HandleFailure(error, request.SessionKey));
            SetBusy(true);
            BackgroundWork.Start(job);
        }

        private void Complete(PasswordBox[] passwordBoxes, string sessionKey)
        {
            if (_playerSession.SessionKey != sessionKey)
            {
                SetBusy(false);
                return;
            }

            foreach (PasswordBox passwordBox in passwordBoxes)
            {
                passwordBox.Clear();
            }
            _dialogService.ShowDialog(new DialogRequest
            {
                Type = DialogType.Success,
                TitleKey = "MessageAccount_msgPasswordChangedTitle",
                MessageKey = "MessageAccount_msgPasswordChanged",
                OnAccept = () =>
                {
                    SetBusy(false);
                    if (_playerSession.SessionKey != sessionKey)
                    {
                        return;
                    }

                    _onSuccess();
                }
            });
        }

        private void HandleFailure(Exception error, string sessionKey)
        {
            SetBusy(false);
            if (_playerSession.SessionKey != sessionKey)
            {
                return;
            }

            DialogRequest dialog = AccountFailureDialogs.ForPasswordChange(error, _logger);
            _dialogService.ShowDialog(dialog);
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;
            ChangePasswordCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }
    }
}
