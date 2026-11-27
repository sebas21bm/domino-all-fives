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
    /// <summary>Presents registration input, remote outcomes and session navigation.</summary>
    public class RegisterAccountViewModel : ViewModelBase
    {
        private IDialogService _dialogService;
        private IRegistrationClient _registrationClient;
        private ILogger<RegisterAccountViewModel> _logger;
        private bool _isBusy;
        private readonly PlayerSession _playerSession;
        private readonly Action _onRegistrationSuccess;
        private readonly Action _onCancel;

        private string _username;
        private string _email;

        private bool _isUsernameRequiredVisible;
        private bool _isEmailRequiredVisible;
        private bool _isPasswordRequiredVisible;
        private bool _isConfirmPasswordRequiredVisible;

        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                SetProperty(ref _username, value);
            }
        }

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

        public bool IsUsernameRequiredVisible
        {
            get
            {
                return _isUsernameRequiredVisible;
            }
            set
            {
                SetProperty(ref _isUsernameRequiredVisible, value);
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

        public bool IsConfirmPasswordRequiredVisible
        {
            get
            {
                return _isConfirmPasswordRequiredVisible;
            }
            set
            {
                SetProperty(ref _isConfirmPasswordRequiredVisible, value);
            }
        }

        public RelayCommand RegisterCommand { get; }

        public RelayCommand CancelCommand { get; }

        public RegisterAccountViewModel(
            PlayerSession playerSession,
            Action onRegistrationSuccess,
            Action onCancel)
        {
            _playerSession = playerSession;
            _onRegistrationSuccess = onRegistrationSuccess;
            _onCancel = onCancel;
            _username = string.Empty;
            _email = string.Empty;
            RegisterCommand = new RelayCommand(ExecuteRegister, CanRegister);
            CancelCommand = new RelayCommand(_onCancel, CanCancel);
        }

        /// <summary>Connects the presentation to its remote client and dialog service.</summary>
        /// <param name = "dialogService">Displays localized messages.</param>
        /// <param name = "registrationClient">Requests account creation.</param>
        /// <param name = "logger">Records registration failures.</param>
        public void Initialize(
            IDialogService dialogService,
            IRegistrationClient registrationClient,
            ILogger<RegisterAccountViewModel> logger)
        {
            _dialogService = dialogService;
            _logger = logger;
            _registrationClient = registrationClient;
        }

        private void ExecuteRegister(object parameter)
        {
            if (_isBusy)
            {
                return;
            }

            PasswordBox[] passwordBoxes = parameter as PasswordBox[];

            if (passwordBoxes == null ||
                passwordBoxes.Length < 2)
            {
                return;
            }

            string password = passwordBoxes[0]?.Password;
            string confirmPassword = passwordBoxes[1]?.Password;

            if (!ValidateRequiredFields(
                password,
                confirmPassword))
            {
                return;
            }

            if (password != confirmPassword)
            {
                ShowRegistrationFailure(passwordMismatch: true);
                return;
            }

            AccountRegistration registrationData = new AccountRegistration
            {
                Username = Username.Trim(),
                Email = Email.Trim(),
                Password = password,
                PreferredLanguageId = LanguageManager.Instance.CurrentLanguageCode
            };
            BackgroundJob<AccountRegistration, PlayerAccount> job =
                new BackgroundJob<AccountRegistration, PlayerAccount>(
                    _registrationClient.Register,
                    registrationData,
                    CompleteRegistration,
                    HandleRegistrationFailure);
            SetBusy(true);
            BackgroundWork.Start(job);
        }

        private void CompleteRegistration(PlayerAccount account, AccountRegistration registrationData)
        {
            SetBusy(false);
            if (account == null
                || account.PlayerId <= 0
                || string.IsNullOrWhiteSpace(account.Username)
                || string.IsNullOrWhiteSpace(account.SessionKey))
            {
                _logger.LogError("Registration service returned an invalid account.");
                ShowRegistrationFailure();
                return;
            }

            _playerSession.Start(
                account.PlayerId,
                account.Username,
                account.PreferredLanguageId,
                account.SessionKey);
            _dialogService.ShowDialog(new DialogRequest
            {
                Type = DialogType.Success,
                TitleKey = "MessageAccount_msgAccountCreatedTitle",
                MessageKey = "MessageAccount_msgAccountCreated",
                OnAccept = _onRegistrationSuccess
            });
        }

        private void HandleRegistrationFailure(Exception ex, AccountRegistration registrationData)
        {
            SetBusy(false);
            if (ex is FaultException<RegistrationIssue> registrationFault)
            {
                _logger.LogDebug(ex, "Registration rejected. Issue: {RegistrationIssue}.",
                    registrationFault.Detail);
                ShowRegistrationFailure(registrationFault.Detail);
                return;
            }

            _logger.LogError(ex, "Registration service could not complete the operation.");
            bool connectionFailure = ex is TimeoutException
                || (ex is CommunicationException && !(ex is FaultException));
            ShowRegistrationFailure(connectionFailure: connectionFailure);
        }

        private bool CanRegister(object parameter)
        {
            return !_isBusy;
        }

        private bool CanCancel()
        {
            return !_isBusy;
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;
            RegisterCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }

        private bool ValidateRequiredFields(
            string password,
            string confirmPassword)
        {
            IsUsernameRequiredVisible =
                string.IsNullOrWhiteSpace(Username);

            IsEmailRequiredVisible =
                string.IsNullOrWhiteSpace(Email);

            IsPasswordRequiredVisible =
                string.IsNullOrWhiteSpace(password);

            IsConfirmPasswordRequiredVisible =
                string.IsNullOrWhiteSpace(confirmPassword);

            return !IsUsernameRequiredVisible &&
                   !IsEmailRequiredVisible &&
                   !IsPasswordRequiredVisible &&
                   !IsConfirmPasswordRequiredVisible;
        }

        private void ShowRegistrationFailure(
            RegistrationIssue? issue = null,
            bool connectionFailure = false,
            bool passwordMismatch = false)
        {
            switch (issue)
            {
                case null when passwordMismatch:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageAccount_msgIncorrectPasswordTitle",
                        MessageKey = "MessageAccount_msgIncorrectPassword"
                    });
                    break;

                case RegistrationIssue.InvalidPassword:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageAccount_msgUnsafePasswordTitle",
                        MessageKey = "MessageAccount_msgUnsafePassword"
                    });
                    break;

                case RegistrationIssue.UsernameAlreadyExists:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageAccount_msgUsernameUsedTitle",
                        MessageKey = "MessageAccount_msgUsernameUsed"
                    });
                    break;

                case RegistrationIssue.EmailAlreadyExists:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageAccount_msgEmailUsedTitle",
                        MessageKey = "MessageAccount_msgEmailUsed"
                    });
                    break;

                case RegistrationIssue.InvalidUsername:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageAccount_msgInvalidUsernameTitle",
                        MessageKey = "MessageAccount_msgInvalidUsername"
                    });
                    break;

                case RegistrationIssue.InvalidEmail:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageAccount_msgInvalidEmailTitle",
                        MessageKey = "MessageAccount_msgInvalidEmail"
                    });
                    break;

                case RegistrationIssue.InvalidRegistrationData:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageAccount_msgInvalidDataTitle",
                        MessageKey = "MessageAccount_msgInvalidData"
                    });
                    break;

                case null when connectionFailure:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey = "Global_msgConnectionErrorTitle",
                        MessageKey = "MessageAccount_msgAccountCreationError"
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
