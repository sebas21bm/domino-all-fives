using System;
using System.ServiceModel;
using System.Windows.Controls;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Threading;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFivesService;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private IDialogService _dialogService;
        private ISessionClient _loginClient;
        private readonly PlayerSession _playerSession;
        private ILogger<LoginViewModel> _logger;
        private LanguageManager _languageManager;
        private readonly Action _onLoginSuccess;
        private readonly Action _onCancel;
        private readonly Action _onGoToRegister;
        private readonly Action _onGoToRecover;
        private string _email;
        private string _password;
        private bool _isEmailRequiredVisible;
        private bool _isPasswordRequiredVisible;
        private bool _isBusy;
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                SetProperty(ref _email, value);
            }
        }
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                SetProperty(ref _password, value);
            }
        }
        public bool IsEmailRequiredVisible
        {
            get
            {
                return _isEmailRequiredVisible;
            }
            set
            {
                SetProperty(ref _isEmailRequiredVisible, value);
            }
        }
        public bool IsPasswordRequiredVisible
        {
            get
            {
                return _isPasswordRequiredVisible;
            }
            set
            {
                SetProperty(ref _isPasswordRequiredVisible, value);
            }
        }
        public RelayCommand LoginCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand GoToRegisterCommand { get; }
        public RelayCommand GoToRecoverCommand { get; }

        public LoginViewModel(
            PlayerSession playerSession,
            Action onLoginSuccess,
            Action onCancel,
            Action onGoToRegister,
            Action onGoToRecover)
        {
            _playerSession = playerSession;
            _onLoginSuccess = onLoginSuccess;
            _onCancel = onCancel;
            _onGoToRegister = onGoToRegister;
            _onGoToRecover = onGoToRecover;
            _email = string.Empty;
            _password = string.Empty;
            _isEmailRequiredVisible = false;
            _isPasswordRequiredVisible = false;
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            CancelCommand = new RelayCommand(CancelLogin, CanCancelLogin);
            GoToRegisterCommand = new RelayCommand(_onGoToRegister, CanCancelLogin);
            GoToRecoverCommand = new RelayCommand(_onGoToRecover, () => false);
        }

        public void Initialize(
            IDialogService dialogService,
            ISessionClient loginClient,
            ILogger<LoginViewModel> logger,
            LanguageManager languageManager = null)
        {
            _dialogService = dialogService;
            _logger = logger;
            _languageManager = languageManager ?? LanguageManager.Instance;
            _loginClient = loginClient;
        }

        private void ExecuteLogin(object parameter)
        {
            if (_isBusy)
            {
                return;
            }

            if (parameter is PasswordBox passwordBox)
            {
                Password = passwordBox.Password;
            }

            if (!ValidateInput())
            {
                return;
            }

            Credentials credentials = new Credentials
            {
                EmailOrUsername = Email.Trim(),
                Password = Password
            };
            BackgroundJob<Credentials, PlayerAccount> job =
                new BackgroundJob<Credentials, PlayerAccount>(
                    _loginClient.LoginWithAccount,
                    credentials,
                    CompleteLogin,
                    HandleLoginFailure);
            SetBusy(true);
            BackgroundWork.Start(job);
        }

        private void CompleteLogin(PlayerAccount authenticatedAccount, Credentials credentials)
        {
            SetBusy(false);
            if (authenticatedAccount == null
                || authenticatedAccount.PlayerId <= 0
                || string.IsNullOrWhiteSpace(authenticatedAccount.Username)
                || string.IsNullOrWhiteSpace(authenticatedAccount.SessionKey))
            {
                _logger.LogError("Authentication service returned an invalid account.");
                ShowMessageFailure();
                return;
            }

            _languageManager.ApplyAccountLanguage(authenticatedAccount.PreferredLanguageId);
            if (_languageManager.CurrentLanguageCode != authenticatedAccount.PreferredLanguageId)
            {
                _logger.LogWarning(
                    "Unsupported stored account language "
                    + "{PreferredLanguageId}; using fallback.",
                    authenticatedAccount.PreferredLanguageId);
            }

            _playerSession.Start(
                authenticatedAccount.PlayerId,
                authenticatedAccount.Username,
                authenticatedAccount.PreferredLanguageId,
                authenticatedAccount.SessionKey);
            _logger.LogInformation(
                "Player {PlayerId} logged in successfully.",
                authenticatedAccount.PlayerId);
            _onLoginSuccess.Invoke();
        }

        private void HandleLoginFailure(Exception ex, Credentials credentials)
        {
            SetBusy(false);
            if (ex is FaultException<AccountIssue> accountFault)
            {
                _logger.LogDebug(ex, "Login rejected. Account issue: {AccountIssue}.",
                    accountFault.Detail);
                ShowMessageFailure(accountFault.Detail);
            }
            else
            {
                _logger.LogError(ex, "Authentication service could not complete login.");
                bool connectionFailure = ex is TimeoutException
                    || (ex is CommunicationException && !(ex is FaultException));
                ShowMessageFailure(connectionFailure: connectionFailure);
            }
        }

        private bool CanExecuteLogin(object parameter)
        {
            return !_isBusy;
        }

        private bool CanCancelLogin()
        {
            return !_isBusy;
        }

        private void CancelLogin()
        {
            if (!_isBusy)
            {
                _onCancel();
            }
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;
            LoginCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
            GoToRegisterCommand.RaiseCanExecuteChanged();
        }

        private bool ValidateInput()
        {
            IsEmailRequiredVisible = string.IsNullOrWhiteSpace(Email);
            IsPasswordRequiredVisible = string.IsNullOrWhiteSpace(Password);
            return !IsEmailRequiredVisible && !IsPasswordRequiredVisible;
        }

        private void ShowMessageFailure(AccountIssue? issue = null, bool connectionFailure = false)
        {
            switch (issue)
            {
                case AccountIssue.Banned:
                    _dialogService.ShowDialog(
                        new DialogRequest {
                            Type = DialogType.Warning,
                            TitleKey = "MessageAuthentication_msgBannedAccountTitle",
                            MessageKey = "MessageAuthentication_msgBannedAccount" });
                    break;
                case AccountIssue.Suspended:
                    _dialogService.ShowDialog(
                        new DialogRequest {
                            Type = DialogType.Warning,
                            TitleKey = "MessageAuthentication_msgSuspendedAccountTitle",
                            MessageKey = "MessageAuthentication_msgSuspendedAccount" });
                    break;
                case AccountIssue.InvalidCredentials:
                    _dialogService.ShowDialog(
                        new DialogRequest {
                            Type = DialogType.Warning,
                            TitleKey = "MessageAuthentication_msgInvalidCredentialsTitle",
                            MessageKey = "MessageAuthentication_msgInvalidCredentials" });
                    break;
                case null when connectionFailure:
                    _dialogService.ShowDialog(
                        new DialogRequest {
                            Type = DialogType.Error,
                            TitleKey = "Global_msgConnectionErrorTitle",
                            MessageKey = "MessageAuthentication_msgConnectionError" });
                    break;
                default:
                    _dialogService.ShowDialog(
                        new DialogRequest {
                            Type = DialogType.Error,
                            TitleKey = "Global_msgDefaultErrorTitle",
                            MessageKey = "Global_msgDefaultError" });
                    break;
            }
        }
    }
}
