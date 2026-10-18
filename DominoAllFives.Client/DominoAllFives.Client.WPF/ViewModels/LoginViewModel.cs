using System;
using System.ServiceModel;
using System.Windows.Controls;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFivesService;
using Microsoft.Extensions.Logging;


namespace DominoAllFives.Client.WPF.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ILoginClient _loginClient;
        private readonly PlayerSession _playerSession;
        private readonly ILogger<LoginViewModel> _logger;
        private readonly LanguageManager _languageManager;

        private readonly Action _onLoginSuccess;
        private readonly Action _onCancel;
        private readonly Action _onGoToRegister;
        private readonly Action _onGoToRecover;

        private string _email;
        private string _password;
        private bool _isEmailRequiredVisible;
        private bool _isPasswordRequiredVisible;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public bool IsEmailRequiredVisible
        {
            get => _isEmailRequiredVisible;
            set => SetProperty(ref _isEmailRequiredVisible, value);
        }

        public bool IsPasswordRequiredVisible
        {
            get => _isPasswordRequiredVisible;
            set => SetProperty(ref _isPasswordRequiredVisible, value);
        }

        public RelayCommand LoginCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand GoToRegisterCommand { get; }
        public RelayCommand GoToRecoverCommand { get; }

        public LoginViewModel(
            IDialogService dialogService,
            ILoginClient loginClient,
            PlayerSession playerSession,
            ILogger<LoginViewModel> logger,
            Action onLoginSuccess,
            Action onCancel,
            Action onGoToRegister,
            Action onGoToRecover,
            LanguageManager languageManager = null)
        {
            _dialogService = dialogService
                ?? throw new ArgumentNullException(nameof(dialogService));

            _loginClient = loginClient
                ?? throw new ArgumentNullException(nameof(loginClient));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));

            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));

            _onLoginSuccess = onLoginSuccess
                ?? throw new ArgumentNullException(nameof(onLoginSuccess));

            _onCancel = onCancel
                ?? throw new ArgumentNullException(nameof(onCancel));

            _onGoToRegister = onGoToRegister
                ?? throw new ArgumentNullException(nameof(onGoToRegister));

            _onGoToRecover = onGoToRecover
                ?? throw new ArgumentNullException(nameof(onGoToRecover));

            _languageManager = languageManager ?? LanguageManager.Instance;
            _email = string.Empty;
            _password = string.Empty;
            _isEmailRequiredVisible = false;
            _isPasswordRequiredVisible = false;

            LoginCommand = new RelayCommand(ExecuteLogin);
            CancelCommand = new RelayCommand(_onCancel);
            GoToRegisterCommand = new RelayCommand(_onGoToRegister);
            GoToRecoverCommand = new RelayCommand(_onGoToRecover);
        }

        private void ExecuteLogin(object parameter)
        {
            if (parameter is PasswordBox passwordBox)
            {
                Password = passwordBox.Password;
            }

            if (!ValidateInput())
            {
                return;
            }

            _logger.LogInformation("Attempting login for user: {EmailOrUsername}", Email.Trim());

            LoginResult result;

            try
            {
                LoginCredentials credentials = new LoginCredentials
                {
                    EmailOrUsername = Email.Trim(),
                    Password = Password
                };

                result = _loginClient.Login(credentials);
            }
            catch (FaultException ex)
            {
                _logger.LogError(
                    ex,
                    "Authentication service unavailable for user {EmailOrUsername}.",
                    Email.Trim());

                ShowLoginFailure(LoginFailureReason.ServiceUnavailable);
                return;
            }


            if (result.Reason != LoginFailureReason.None)
            {
                _logger.LogWarning("Login rejected. Reason: {FailureReason}.", result.Reason);
                ShowLoginFailure(result.Reason);
                return;
            }

            PlayerAccount authenticatedAccount = result.Account;
            _languageManager.ApplyAccountLanguage(authenticatedAccount.PreferredLanguageId);
            if (_languageManager.CurrentLanguageCode != authenticatedAccount.PreferredLanguageId)
            {
                _logger.LogWarning(
                    "Unsupported stored account language {PreferredLanguageId}; using fallback.",
                    authenticatedAccount.PreferredLanguageId);
            }
            _playerSession.Start(
                authenticatedAccount.PlayerId,
                authenticatedAccount.Username,
                authenticatedAccount.PreferredLanguageId);

            _logger.LogInformation("User {EmailOrUsername} logged in successfully with " +
                "PlayerId: {PlayerId}", Email.Trim(), authenticatedAccount.PlayerId);

            _onLoginSuccess.Invoke();
        }

        private bool ValidateInput()
        {
            IsEmailRequiredVisible =
                string.IsNullOrWhiteSpace(Email);

            IsPasswordRequiredVisible =
                string.IsNullOrWhiteSpace(Password);

            return !IsEmailRequiredVisible &&
                   !IsPasswordRequiredVisible;
        }

        private void ShowLoginFailure(
            LoginFailureReason failureReason)
        {
            switch (failureReason)
            {
                case LoginFailureReason.Banned:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageAuthentication_msgBannedAccountTitle",
                        MessageKey = "MessageAuthentication_msgBannedAccount"
                    });
                    break;

                case LoginFailureReason.Suspended:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageAuthentication_msgSuspendedAccountTitle",
                        MessageKey = "MessageAuthentication_msgSuspendedAccount"
                    });
                    break;

                case LoginFailureReason.ServiceUnavailable:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey = "Global_msgConnectionErrorTitle",
                        MessageKey = "MessageAuthentication_msgConnectionError"
                    });
                    break;

                case LoginFailureReason.InvalidCredentials:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageAuthentication_msgInvalidCredentialsTitle",
                        MessageKey = "MessageAuthentication_msgInvalidCredentials"
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
