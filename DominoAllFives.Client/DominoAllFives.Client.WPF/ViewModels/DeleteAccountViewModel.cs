using System;
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
    /// Handles player account deletion confirmation.
    /// </summary>
    public class DeleteAccountViewModel : ViewModelBase
    {
        private readonly IAccountService _accountService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<DeleteAccountViewModel> _logger;
        private readonly PlayerSession _playerSession;

        private readonly Action _onAccountDeletedSuccess;
        private readonly Action _onCancel;

        private bool _isPasswordRequiredVisible;

        public bool IsPasswordRequiredVisible
        {
            get => _isPasswordRequiredVisible;
            set => SetProperty(ref _isPasswordRequiredVisible, value);
        }

        public RelayCommand ConfirmDeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        public DeleteAccountViewModel(
            IDialogService dialogService,
            IAccountService accountService,
            ILogger<DeleteAccountViewModel> logger,
            PlayerSession playerSession,
            Action onAccountDeletedSuccess,
            Action onCancel)
        {
            _dialogService = dialogService
                ?? throw new ArgumentNullException(
                    nameof(dialogService));

            _accountService = accountService
                ?? throw new ArgumentNullException(
                    nameof(accountService));

            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(
                    nameof(playerSession));

            _onAccountDeletedSuccess = onAccountDeletedSuccess
                ?? throw new ArgumentNullException(
                    nameof(onAccountDeletedSuccess));

            _onCancel = onCancel
                ?? throw new ArgumentNullException(nameof(onCancel));

            ConfirmDeleteCommand =
                new RelayCommand(ExecuteConfirmDelete);

            CancelCommand = new RelayCommand(_onCancel);
        }

        private void ExecuteConfirmDelete(object parameter)
        {
            if (!(parameter is PasswordBox passwordBox))
            {
                _logger.LogError(
                    "PasswordBox parameter was not provided for account deletion.");
                return;
            }

            string password = passwordBox.Password;

            IsPasswordRequiredVisible =
                string.IsNullOrWhiteSpace(password);

            if (IsPasswordRequiredVisible)
            {
                return;
            }

            if (!_playerSession.IsAuthenticated ||
                !_playerSession.PlayerId.HasValue)
            {
                _logger.LogWarning(
                    "Account deletion attempted without an authenticated session.");

                ShowDeleteFailure(
                    DeleteFailureReason.ServiceUnavailable);

                return;
            }

            int playerId = _playerSession.PlayerId.Value;

            _logger.LogInformation(
                "Account deletion requested for player {PlayerId}.",
                playerId);

            DeleteAccountRequestDto request =
                new DeleteAccountRequestDto
                {
                    PlayerId = playerId,
                    Password = password
                };

            DeleteAccountResultDto result =
                _accountService.DeleteAccount(request);

            if (!result.IsSuccessful)
            {
                _logger.LogWarning(
                    "Account deletion failed for player {PlayerId}. Reason: {FailureReason}.",
                    playerId,
                    result.FailureReason);

                ShowDeleteFailure(result.FailureReason);
                return;
            }

            _logger.LogInformation(
                "Account successfully deleted for player {PlayerId}.",
                playerId);

            passwordBox.Clear();

            _dialogService.ShowDialog(new DialogRequest
            {
                Type = DialogType.Success,
                TitleKey = "MessageAccount_msgDeleteSuccessTitle",
                MessageKey = "MessageAccount_msgDeleteSuccess",
                OnAccept = _onAccountDeletedSuccess
            });
        }

        private void ShowDeleteFailure(
            DeleteFailureReason failureReason)
        {
            switch (failureReason)
            {
                case DeleteFailureReason.PasswordNotMatch:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey =
                            "MessageAccount_msgWrongCurrentPasswordTitle",
                        MessageKey =
                            "MessageAccount_msgWrongCurrentPassword"
                    });
                    break;

                case DeleteFailureReason.AccountNotFound:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey =
                            "MessageAccount_msgAccountNotFoundTitle",
                        MessageKey =
                            "MessageAccount_msgAccountNotFound"
                    });
                    break;

                case DeleteFailureReason.ServiceUnavailable:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey = "Global_msgConnectionErrorTitle",
                        MessageKey = "Global_msgConnectionError"
                    });
                    break;

                default:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey = "Global_msgDefaultErrorTitle",
                        MessageKey = "Global_msgDefaultError"
                    });
                    break;
            }
        }
    }
}