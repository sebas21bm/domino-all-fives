using System;
using System.ServiceModel;
using System.Windows.Controls;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Threading;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFivesService;
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
            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));
            _onSuccess = onSuccess
                ?? throw new ArgumentNullException(nameof(onSuccess));
            _onCancel = onCancel
                ?? throw new ArgumentNullException(nameof(onCancel));
            ConfirmDeleteCommand = new RelayCommand(ExecuteDeleteAccount, unused => CanSubmit());
            CancelCommand = new RelayCommand(Cancel, () => !_isBusy);
        }

        public void Initialize(
            IDialogService dialogService,
            IAccountClient accountClient,
            ILogger<DeleteAccountViewModel> logger)
        {
            if (_accountClient != null)
            {
                throw new InvalidOperationException("DeleteAccountViewModel has already been initialized.");
            }

            _dialogService = dialogService
                ?? throw new ArgumentNullException(nameof(dialogService));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _accountClient = accountClient
                ?? throw new ArgumentNullException(nameof(accountClient));
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

            if (error is FaultException<AccountIssue> accountFault)
            {
                _logger.LogDebug(error, "Account deletion rejected. Account issue: {AccountIssue}.",
                    accountFault.Detail);
                ShowMessageFailure(accountFault.Detail);
            }
            else if (error is FaultException<AccessIssue> accessFault)
            {
                _logger.LogDebug(error, "Account deletion rejected. Access issue: {AccessIssue}.",
                    accessFault.Detail);
                ShowMessageFailure(accessIssue: accessFault.Detail);
            }
            else
            {
                _logger.LogError(error, "Account deletion could not complete.");
                bool connectionFailure = error is TimeoutException
                    || (error is CommunicationException && !(error is FaultException));
                ShowMessageFailure(connectionFailure: connectionFailure);
            }
        }

        private void ShowMessageFailure(
            AccountIssue? issue = null,
            AccessIssue? accessIssue = null,
            bool connectionFailure = false)
        {
            switch (issue)
            {
                case AccountIssue.IncorrectCurrentPassword:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageAccount_msgWrongCurrentPasswordTitle",
                        MessageKey = "MessageAccount_msgWrongCurrentPassword"
                    });
                    break;
                case AccountIssue.PlayerNotFound:
                case null when accessIssue == AccessIssue.SessionRequired:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageAccount_msgAccountNotFoundTitle",
                        MessageKey = "MessageAccount_msgAccountNotFound"
                    });
                    break;
                case null when connectionFailure:
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

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;
            ConfirmDeleteCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }
    }
}
