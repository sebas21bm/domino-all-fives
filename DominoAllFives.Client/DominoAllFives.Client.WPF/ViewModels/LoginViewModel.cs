using System;
using System.Windows.Controls;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Services;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IAccountService _accountService;
        private readonly PlayerSession _playerSession;

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
            IAccountService accountService,
            PlayerSession playerSession,
            Action onLoginSuccess,
            Action onCancel,
            Action onGoToRegister,
            Action onGoToRecover)
        {
            _dialogService = dialogService
                ?? throw new ArgumentNullException(nameof(dialogService));

            _accountService = accountService
                ?? throw new ArgumentNullException(nameof(accountService));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));

            _onLoginSuccess = onLoginSuccess
                ?? throw new ArgumentNullException(nameof(onLoginSuccess));

            _onCancel = onCancel
                ?? throw new ArgumentNullException(nameof(onCancel));

            _onGoToRegister = onGoToRegister
                ?? throw new ArgumentNullException(nameof(onGoToRegister));

            _onGoToRecover = onGoToRecover
                ?? throw new ArgumentNullException(nameof(onGoToRecover));

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

            LoginRequestDto request = new LoginRequestDto
            {
                EmailOrUsername = Email.Trim(),
                Password = Password
            };

            LoginResultDto result = _accountService.Login(request);

            if (!result.IsSuccessful)
            {
                ShowLoginFailure(result.FailureReason);
                return;
            }

            _playerSession.Start(result.PlayerId);

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
                    _dialogService.ShowDialog(
                        DialogType.Warning,
                        "MessageAuthentication_msgBannedAccountTitle",
                        "MessageAuthentication_msgBannedAccount",
                        () => { });
                    break;
                case LoginFailureReason.Suspended:
                    _dialogService.ShowDialog(
                        DialogType.Warning,
                        "MessageAuthentication_msgSuspendedAccountTitle",
                        "MessageAuthentication_msgSuspendedAccount",
                        () => { });
                    break;

                case LoginFailureReason.ServiceUnavailable:
                    _dialogService.ShowDialog(
                        DialogType.Error,
                        "Global_msgConnectionError",
                        "Global_msgConnectionError",
                        () => { });
                    break;
                case LoginFailureReason.InvalidCredentials:
                    _dialogService.ShowDialog(
                        DialogType.Warning,
                        "MessageAuthentication_msgInvalidCredentialsTitle",
                        "MessageAuthentication_msgInvalidCredentials",
                        () => { });
                    break;
                default:
                    _dialogService.ShowDialog(
                        DialogType.Error,
                        "Global_msgDefaultErrorTitle",
                        "Global_msgDefaultError",
                        () => { });
                    break;

            }
        }
    }
}