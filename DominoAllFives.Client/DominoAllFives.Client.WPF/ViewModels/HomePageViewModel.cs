using System;
using System.Collections.Generic;
using System.Windows;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Contracts.Services;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IAccountService _accountService;
        private readonly PlayerSession _playerSession;

        private ViewModelBase _currentModal;
        private bool _isModalVisible;
        private string _selectedLanguage;

        public ViewModelBase CurrentModal
        {
            get => _currentModal;
            private set => SetProperty(ref _currentModal, value);
        }

        public bool IsModalVisible
        {
            get => _isModalVisible;
            private set => SetProperty(ref _isModalVisible, value);
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (SetProperty(ref _selectedLanguage, value) &&
                    !string.IsNullOrEmpty(value))
                {
                    OnLanguageChanged(value);
                }
            }
        }

        public List<LanguageOption> AvailableLanguages { get; }

        public RelayCommand ShowLoginCommand { get; }
        public RelayCommand PlayAsGuestCommand { get; }
        public RelayCommand ExitGameCommand { get; }

        public HomePageViewModel(
            IFrameNavigationService navigationService,
            IDialogService dialogService,
            PlayerSession playerSession,
            IAccountService accountService)
        {
            _navigationService = navigationService
                ?? throw new ArgumentNullException(nameof(navigationService));

            _dialogService = dialogService
                ?? throw new ArgumentNullException(nameof(dialogService));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));

            _accountService = accountService
                ?? throw new ArgumentNullException(nameof(accountService));

            AvailableLanguages = new List<LanguageOption>
            {
                new LanguageOption
                {
                    Code = "es-MX",
                    DisplayName = "Español (MX)"
                },
                new LanguageOption
                {
                    Code = "en-US",
                    DisplayName = "English (USA)"
                },
                new LanguageOption
                {
                    Code = "pt-BR",
                    DisplayName = "Português (BR)"
                }
            };

            _selectedLanguage =
                LanguageManager.Instance.CurrentLanguageCode;

            ShowLoginCommand = new RelayCommand(OpenLoginModal);
            PlayAsGuestCommand = new RelayCommand(PlayAsGuest);
            ExitGameCommand = new RelayCommand(ExitGame);
        }

        private void OnLanguageChanged(string cultureCode)
        {
            LanguageManager.Instance.ChangeLanguage(cultureCode);
        }

        private void OpenLoginModal()
        {

            CurrentModal = new LoginViewModel(
                _dialogService,
                _accountService,
                _playerSession,
                OnLoginSuccess,
                CloseModal,
                OpenRegisterModal,
                OpenRecoverModal);

            IsModalVisible = true;
        }

        private void CloseModal()
        {
            CurrentModal = null;
            IsModalVisible = false;
        }

        private void OnLoginSuccess()
        {
            CloseModal();

            _navigationService.NavigateTo<MainMenuViewModel>();
        }

        private void OpenRegisterModal()
        {
            RegistrationController registrationController =
                new RegistrationController();

            CurrentModal = new RegisterAccountViewModel(
                _dialogService,
                registrationController,
                _playerSession,
                OpenUploadProfilePictureModal,
                CloseModal);

            IsModalVisible = true;
        }

        private void OpenUploadProfilePictureModal()
        {
            ProfilePictureController profilePictureController =
                new ProfilePictureController();

            CurrentModal = new UploadProfilePictureViewModel(
                _dialogService,
                profilePictureController,
                _playerSession,
                OnRegistrationSuccess,
                CloseModal);

            IsModalVisible = true;
        }

        private void OnRegistrationSuccess()
        {
            CloseModal();

            _navigationService.NavigateTo<MainMenuViewModel>();
        }

        private void OpenRecoverModal()
        {
            CurrentModal = new RecoverAccountViewModel(
                onGoToVerifyEmail: OpenVerifyEmailForRecoveryModal,
                onCancel: CloseModal);

            IsModalVisible = true;
        }

        private void OpenVerifyEmailForRecoveryModal(string email)
        {
            CurrentModal = new VerifyEmailViewModel(
                targetEmail: email,
                onVerificationSuccess: OpenChangePasswordModal,
                onCancel: CloseModal);

            IsModalVisible = true;
        }

        private void OpenChangePasswordModal()
        {
            CurrentModal = new ChangePasswordViewModel(
                onPasswordChangedSuccess: OnPasswordRecoverySuccess,
                onCancel: CloseModal);

            IsModalVisible = true;
        }

        private void OnPasswordRecoverySuccess()
        {
            CloseModal();

            _navigationService.NavigateTo<MainMenuViewModel>();
        }

        private void PlayAsGuest()
        {
            _playerSession.Clear();
            _navigationService.NavigateTo<MainMenuViewModel>();
        }

        private void ExitGame()
        {
            Application.Current.Shutdown();
        }
    }
}