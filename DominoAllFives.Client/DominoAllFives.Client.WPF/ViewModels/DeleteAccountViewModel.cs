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
    public class DeleteAccountViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IAccountService _accountService;
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
                ?? throw new ArgumentNullException(nameof(dialogService));

            _accountService = accountService
                ?? throw new ArgumentNullException(nameof(accountService));

            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));

            _onAccountDeletedSuccess = onAccountDeletedSuccess 
                ?? throw new ArgumentNullException(nameof(onAccountDeletedSuccess));

            _onCancel = onCancel 
                ?? throw new ArgumentNullException(nameof(onCancel));

            ConfirmDeleteCommand = new RelayCommand(ExecuteConfirmDelete);
            CancelCommand = new RelayCommand(_onCancel);
        }

        private void ExecuteConfirmDelete(object parameter)
        {
            // TODO: Implement delete account (logic elimination)

        }

        private void ShowDeleteFailure(DeleteFailureReason failureReason)
        {
            switch (failureReason)
            {
                case DeleteFailureReason.PasswordNotMatch:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "",
                        MessageKey = ""
                    });
                    break;
                case DeleteFailureReason.ServiceUnavailable:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "",
                        MessageKey = ""
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