using System;
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
    /// <summary>Handles the account deletion dialog for an authenticated account.</summary>
    public class DeleteAccountViewModel : ViewModelBase
    {
        private readonly PlayerSession _playerSession;
        private readonly Action _onSuccess;
        private readonly Action _onCancel;
        private IDialogService _dialogService;
        private IAccountClient _accountClient;
        private ILogger<DeleteAccountViewModel> _logger;
        private bool _isBusy;
        private bool _isPasswordRequiredVisible;

        public bool IsPasswordRequiredVisible
        {
            get => _isPasswordRequiredVisible;
            private set => SetProperty(ref _isPasswordRequiredVisible, value);
        }

        public RelayCommand ConfirmDeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        public DeleteAccountViewModel(PlayerSession playerSession, Action onSuccess, Action onCancel)
        {
            _playerSession = playerSession;
            _onSuccess = onSuccess;
            _onCancel = onCancel;
            ConfirmDeleteCommand = new RelayCommand(ExecuteDeleteAccount, unused => CanSubmit());
            CancelCommand = new RelayCommand(Cancel, () => !_isBusy);
        }

        public void Initialize(
            IDialogService dialogService,
            IAccountClient accountClient,
            ILogger<DeleteAccountViewModel> logger)
        {
            _dialogService = dialogService;
            _logger = logger;
            _accountClient = accountClient;
            ConfirmDeleteCommand.RaiseCanExecuteChanged();
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

        private void ExecuteDeleteAccount(object parameter)
        {
            if (!CanSubmit())
            {
                return;
            }

            if (!(parameter is PasswordBox passwordBox))
            {
                _logger.LogError("The password box was missing for account deletion.");
                return;
            }

            string password = passwordBox.Password;
            IsPasswordRequiredVisible = string.IsNullOrWhiteSpace(password);
            if (IsPasswordRequiredVisible)
            {
                return;
            }

            string sessionKey = _playerSession.SessionKey;
            BackgroundJob<(string SessionKey, string Password), bool> job =
                new BackgroundJob<(string SessionKey, string Password), bool>(
                    request =>
                    {
                        _accountClient.DeleteAccount(request.SessionKey, request.Password);
                        return true;
                    },
                    (sessionKey, password),
                    (completed, request) => Complete(passwordBox, request.SessionKey),
                    (error, request) => HandleFailure(error, request.SessionKey));
            SetBusy(true);
            BackgroundWork.Start(job);
        }

        private void Complete(PasswordBox passwordBox, string sessionKey)
        {
            if (_playerSession.SessionKey != sessionKey)
            {
                SetBusy(false);
                return;
            }

            passwordBox.Clear();
            _dialogService.ShowDialog(new DialogRequest
            {
                Type = DialogType.Success,
                TitleKey = "MessageAccount_msgDeleteSuccessTitle",
                MessageKey = "MessageAccount_msgDeleteSuccess",
                OnAccept = () =>
                {
                    SetBusy(false);
                    if (_playerSession.SessionKey != sessionKey)
                    {
                        return;
                    }

                    _playerSession.Clear();
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

            DialogRequest dialog = AccountFailureDialogs.ForAccountDeletion(error, _logger);
            _dialogService.ShowDialog(dialog);
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;
            ConfirmDeleteCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }
    }
}
