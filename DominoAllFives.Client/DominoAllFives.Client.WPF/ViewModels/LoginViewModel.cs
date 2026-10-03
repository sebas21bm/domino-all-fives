using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.Contracts.Enums;
using System;
using System.Windows.Controls;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly AuthenticationController _authenticationController;
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
            AuthenticationController authenticationController,
            PlayerSession playerSession,
            Action onLoginSuccess,
            Action onCancel,
            Action onGoToRegister,
            Action onGoToRecover)
        {
            _dialogService = dialogService
                ?? throw new ArgumentNullException(nameof(dialogService));

            _authenticationController = authenticationController
                ?? throw new ArgumentNullException(nameof(authenticationController));

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

            try
            {
                LoginResultDto result =
                    _authenticationController.Login(
                        Email.Trim(),
                        Password);

                if (!result.IsSuccessful)
                {
                    ShowLoginFailure(result.FailureReason);
                    return;
                }

                _playerSession.Start(result.PlayerId);

                _onLoginSuccess.Invoke();
            }
            catch (Exception)
            {
                _dialogService.ShowDialog(
                    DialogType.Error,
                    "MessageAuthentication_msgConnectionErrorTitle",
                    "MessageAuthentication_msgConnectionError",
                    () => { });
            }
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
                case LoginFailureReason.Suspended:
                    _dialogService.ShowDialog(
                        DialogType.Warning,
                        "MessageAuthentication_msgDisabledAccountTitle",
                        "MessageAuthentication_msgDisabledAccount",
                        () => { });
                    break;

                case LoginFailureReason.InvalidCredentials:
                default:
                    _dialogService.ShowDialog(
                        DialogType.Warning,
                        "MessageAuthentication_msgInvalidCredentialsTitle",
                        "MessageAuthentication_msgInvalidCredentials",
                        () => { });
                    break;
            }
        }
    }
}