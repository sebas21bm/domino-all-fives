using System;
using System.ServiceModel;
using System.Windows;
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
                ShowMessageFailure(passwordConfirmationMismatch: true);
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

            if (error is FaultException<AccountIssue> accountFault)
            {
                _logger.LogDebug(error, "Password change rejected. Account issue: {AccountIssue}.",
                    accountFault.Detail);
                ShowMessageFailure(accountFault.Detail);
            }
            else if (error is FaultException<AccessIssue> accessFault)
            {
                _logger.LogDebug(error, "Password change rejected. Access issue: {AccessIssue}.",
                    accessFault.Detail);
                ShowMessageFailure(accessIssue: accessFault.Detail);
            }
            else
            {
                _logger.LogError(error, "Password change could not complete.");
                bool connectionFailure = error is TimeoutException
                    || (error is CommunicationException && !(error is FaultException));
                ShowMessageFailure(connectionFailure: connectionFailure);
            }
        }

        private void ShowMessageFailure(
            AccountIssue? issue = null,
            AccessIssue? accessIssue = null,
            bool connectionFailure = false,
            bool passwordConfirmationMismatch = false)
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
                case AccountIssue.InvalidPassword:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageAccount_msgUnsafePasswordTitle",
                        MessageKey = "MessageAccount_msgUnsafePassword"
                    });
                    break;
                case AccountIssue.PasswordUnchanged:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageAccount_msgSamePasswordTitle",
                        MessageKey = "MessageAccount_msgSamePassword"
                    });
                    break;
                case null when passwordConfirmationMismatch:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageAccount_msgIncorrectPasswordTitle",
                        MessageKey = "MessageAccount_msgIncorrectPassword"
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
            ChangePasswordCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }
    }
}
