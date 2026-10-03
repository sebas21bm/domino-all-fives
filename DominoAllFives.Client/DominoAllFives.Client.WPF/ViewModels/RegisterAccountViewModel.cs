using System;
using System.Data.SqlClient;
using System.Windows.Controls;

using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Contracts.DTOs;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class RegisterAccountViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly RegistrationController _registrationController;
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
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public bool IsUsernameRequiredVisible
        {
            get => _isUsernameRequiredVisible;
            set => SetProperty(ref _isUsernameRequiredVisible, value);
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

        public bool IsConfirmPasswordRequiredVisible
        {
            get => _isConfirmPasswordRequiredVisible;
            set => SetProperty(ref _isConfirmPasswordRequiredVisible, value);
        }

        public RelayCommand RegisterCommand { get; }

        public RelayCommand CancelCommand { get; }

        public RegisterAccountViewModel(
            IDialogService dialogService,
            RegistrationController registrationController,
            PlayerSession playerSession,
            Action onRegistrationSuccess,
            Action onCancel)
        {
            _dialogService = dialogService
                ?? throw new ArgumentNullException(nameof(dialogService));

            _registrationController = registrationController
                ?? throw new ArgumentNullException(
                    nameof(registrationController));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));

            _onRegistrationSuccess = onRegistrationSuccess
                ?? throw new ArgumentNullException(
                    nameof(onRegistrationSuccess));

            _onCancel = onCancel
                ?? throw new ArgumentNullException(nameof(onCancel));

            RegisterCommand = new RelayCommand(ExecuteRegister);
            CancelCommand = new RelayCommand(_onCancel);
        }

        private void ExecuteRegister(object parameter)
        {
            PasswordBox[] passwordBoxes =
                parameter as PasswordBox[];

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
                ShowIncorrectPasswordMessage();
                return;
            }

            RegisterAccountDto registrationData =
                new RegisterAccountDto
                {
                    Username = Username.Trim(),
                    Email = Email.Trim(),
                    Password = password,
                    PreferredLanguageId =
                        LanguageManager.Instance.CurrentLanguageCode
                };

            try
            {
                RegistrationResultDto result =
                    _registrationController.Register(
                        registrationData);

                if (!result.IsSuccessful)
                {
                    ShowRegistrationFailure(
                        result.FailureReason);

                    return;
                }

                _playerSession.Start(result.PlayerId);

                _dialogService.ShowDialog(
                    DialogType.Success,
                    "MessageAccount_msgAccountCreatedTitle",
                    "MessageAccount_msgAccountCreated",
                    _onRegistrationSuccess);
            }
            catch (SqlException)
            {
                _dialogService.ShowDialog(
                    DialogType.Error,
                    "MessageAccount_msgAccountCreationErrorTitle",
                    "MessageAccount_msgAccountCreationError",
                    () => { });
            }
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

        private void ShowIncorrectPasswordMessage()
        {
            _dialogService.ShowDialog(
                DialogType.Warning,
                "MessageAccount_msgIncorrectPasswordTitle",
                "MessageAccount_msgIncorrectPassword",
                () => { });
        }

        private void ShowRegistrationFailure(
            RegistrationFailureReason failureReason)
        {
            switch (failureReason)
            {
                case RegistrationFailureReason.InvalidPassword:
                    _dialogService.ShowDialog(
                        DialogType.Warning,
                        "MessageAccount_msgUnsafePasswordTitle",
                        "MessageAccount_msgUnsafePassword",
                        () => { });
                    break;

                case RegistrationFailureReason.UsernameAlreadyExists:
                    _dialogService.ShowDialog(
                        DialogType.Warning,
                        "MessageAccount_msgUsernameUsedTitle",
                        "MessageAccount_msgUsernameUsed",
                        () => { });
                    break;

                case RegistrationFailureReason.EmailAlreadyExists:
                    _dialogService.ShowDialog(
                        DialogType.Warning,
                        "MessageAccount_msgEmailUsedTitle",
                        "MessageAccount_msgEmailUsed",
                        () => { });
                    break;

                case RegistrationFailureReason.InvalidUsername:
                case RegistrationFailureReason.InvalidEmail:
                case RegistrationFailureReason.InvalidRegistrationData:
                default:
                    _dialogService.ShowDialog(
                        DialogType.Warning,
                        "MessageAccount_msgInvalidDataTitle",
                        "MessageAccount_msgInvalidData",
                        () => { });
                    break;
            }
        }
    }
}
