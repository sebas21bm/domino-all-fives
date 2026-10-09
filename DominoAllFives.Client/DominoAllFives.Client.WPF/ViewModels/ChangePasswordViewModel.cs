
using System;
using System.Windows;
using System.Windows.Controls;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>
    /// Handles password changes for authenticated player accounts.
    /// </summary>
    public class ChangePasswordViewModel : ViewModelBase
    {
        private readonly IAccountService _accountService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ChangePasswordViewModel> _logger;
        private readonly PlayerSession _playerSession;
        private readonly Action _onPasswordChangedSuccess;
        private readonly Action _onCancel;
        private readonly bool _isFromSettings;

        private bool _isCurrentPasswordRequiredVisible;
        private bool _isNewPasswordRequiredVisible;
        private bool _isConfirmPasswordRequiredVisible;

        public Visibility IsCurrentPasswordVisible =>
            _isFromSettings ? Visibility.Visible : Visibility.Collapsed;

        public bool IsCurrentPasswordRequiredVisible
        {
            get => _isCurrentPasswordRequiredVisible;
            set => SetProperty(
                ref _isCurrentPasswordRequiredVisible, value);
        }

        public bool IsNewPasswordRequiredVisible
        {
            get => _isNewPasswordRequiredVisible;
            set => SetProperty(
                ref _isNewPasswordRequiredVisible, value);
        }

        public bool IsConfirmPasswordRequiredVisible
        {
            get => _isConfirmPasswordRequiredVisible;
            set => SetProperty(
                ref _isConfirmPasswordRequiredVisible, value);
        }

        public RelayCommand ChangePasswordCommand { get; }
        public RelayCommand CancelCommand { get; }

        public ChangePasswordViewModel(
            IAccountService accountService,
            IDialogService dialogService,
            ILogger<ChangePasswordViewModel> logger,
            PlayerSession playerSession,
            Action onPasswordChangedSuccess,
            Action onCancel,
            bool isFromSettings = false)
        {
            _accountService = accountService ??
                throw new ArgumentNullException(nameof(accountService));

            _dialogService = dialogService ??
                throw new ArgumentNullException(nameof(dialogService));

            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));

            _playerSession = playerSession ??
                throw new ArgumentNullException(nameof(playerSession));

            _onPasswordChangedSuccess = onPasswordChangedSuccess ??
                throw new ArgumentNullException(
                    nameof(onPasswordChangedSuccess));

            _onCancel = onCancel ??
                throw new ArgumentNullException(nameof(onCancel));

            _isFromSettings = isFromSettings;

            ChangePasswordCommand =
                new RelayCommand(ExecuteChangePassword);

            CancelCommand = new RelayCommand(_onCancel);
        }

        private void ExecuteChangePassword(object parameter)
        {
            if (!(parameter is PasswordBox[] passwordBoxes) ||
                passwordBoxes.Length != 3 ||
                passwordBoxes[0] == null ||
                passwordBoxes[1] == null ||
                passwordBoxes[2] == null)
            {
                _logger.LogError(
                    "PasswordBox parameters were not provided.");
                return;
            }

            PasswordBox currentPasswordBox = passwordBoxes[0];
            PasswordBox newPasswordBox = passwordBoxes[1];
            PasswordBox confirmPasswordBox = passwordBoxes[2];

            string currentPassword = currentPasswordBox.Password;
            string newPassword = newPasswordBox.Password;
            string confirmPassword = confirmPasswordBox.Password;

            IsCurrentPasswordRequiredVisible =
                _isFromSettings &&
                string.IsNullOrWhiteSpace(currentPassword);

            IsNewPasswordRequiredVisible =
                string.IsNullOrWhiteSpace(newPassword);

            IsConfirmPasswordRequiredVisible =
                string.IsNullOrWhiteSpace(confirmPassword);

            if (IsCurrentPasswordRequiredVisible ||
                IsNewPasswordRequiredVisible ||
                IsConfirmPasswordRequiredVisible)
            {
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowDialog(
                    DialogType.Warning,
                    "MessageAccount_msgIncorrectPasswordTitle",
                    "MessageAccount_msgIncorrectPassword");
                return;
            }

            // CU-03 requires a separate, authorized recovery operation.
            if (!_isFromSettings)
            {
                _logger.LogWarning(
                    "Password recovery is not implemented.");
                ShowDefaultError();
                return;
            }

            if (!_playerSession.IsAuthenticated ||
                !_playerSession.PlayerId.HasValue)
            {
                ShowDefaultError();
                return;
            }

            int playerId = _playerSession.PlayerId.Value;

            ChangePasswordRequestDto request =
                new ChangePasswordRequestDto
                {
                    PlayerId = playerId,
                    CurrentPassword = currentPassword,
                    NewPassword = newPassword
                };

            ChangePasswordResultDto result =
                _accountService.ChangePassword(request);

            if (!result.IsSuccessful)
            {
                _logger.LogWarning(
                    "Password change failed for player {PlayerId}. Reason: {FailureReason}.",
                    playerId,
                    result.FailureReason);

                ShowChangePasswordFailure(result.FailureReason);
                return;
            }

            _logger.LogInformation(
                "Password changed successfully for player {PlayerId}.",
                playerId);

            currentPasswordBox.Clear();
            newPasswordBox.Clear();
            confirmPasswordBox.Clear();

            _dialogService.ShowDialog(new DialogRequest
            {
                Type = DialogType.Success,
                TitleKey = "MessageAccount_msgPasswordChangedTitle",
                MessageKey = "MessageAccount_msgPasswordChanged",
                OnAccept = _onPasswordChangedSuccess
            });
        }

        private void ShowChangePasswordFailure(
            ChangePasswordFailureReason failureReason)
        {
            switch (failureReason)
            {
                case ChangePasswordFailureReason.IncorrectCurrentPassword:
                    ShowDialog(
                        DialogType.Warning,
                        "MessageAccount_msgWrongCurrentPasswordTitle",
                        "MessageAccount_msgWrongCurrentPassword");
                    break;

                case ChangePasswordFailureReason.InvalidNewPassword:
                    ShowDialog(
                        DialogType.Warning,
                        "MessageAccount_msgUnsafePasswordTitle",
                        "MessageAccount_msgUnsafePassword");
                    break;

                case ChangePasswordFailureReason.AccountNotFound:
                    ShowDialog(
                        DialogType.Warning,
                        "MessageAccount_msgAccountNotFoundTitle",
                        "MessageAccount_msgAccountNotFound");
                    break;

                case ChangePasswordFailureReason.SameAsCurrentPassword:
                    ShowDialog(
                        DialogType.Warning,
                        "MessageAccount_msgSamePasswordTitle",
                        "MessageAccount_msgSamePassword");
                    break;

                case ChangePasswordFailureReason.ServiceUnavailable:
                    ShowDialog(
                        DialogType.Error,
                        "Global_msgConnectionErrorTitle",
                        "Global_msgConnectionError");
                    break;

                default:
                    ShowDefaultError();
                    break;
            }
        }

        private void ShowDefaultError()
        {
            ShowDialog(
                DialogType.Error,
                "Global_msgDefaultErrorTitle",
                "Global_msgDefaultError");
        }

        private void ShowDialog(
            DialogType type,
            string titleKey,
            string messageKey)
        {
            _dialogService.ShowDialog(new DialogRequest
            {
                Type = type,
                TitleKey = titleKey,
                MessageKey = messageKey
            });
        }
    }
}

