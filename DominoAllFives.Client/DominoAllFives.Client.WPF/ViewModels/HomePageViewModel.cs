using System;
using System.Collections.Generic;
using System.Windows;

using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ILoginClient _loginClient;
        private readonly PlayerSession _playerSession;
        private readonly ILoggerFactory _loggerFactory;

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
            ILoginClient loginClient,
            ILoggerFactory loggerFactory)
        {
            _navigationService = navigationService
                ?? throw new ArgumentNullException(
                    nameof(navigationService));

            _dialogService = dialogService
                ?? throw new ArgumentNullException(
                    nameof(dialogService));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(
                    nameof(playerSession));


            _loginClient = loginClient ?? throw new ArgumentNullException(nameof(loginClient));

            _loggerFactory = loggerFactory
                ?? throw new ArgumentNullException(
                    nameof(loggerFactory));

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
            PlayAsGuestCommand = new RelayCommand(() => { }, () => false);
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
                _loginClient,
                _playerSession,
                _loggerFactory.CreateLogger<LoginViewModel>(),
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
            CurrentModal = new RegisterAccountViewModel(CloseModal);
            IsModalVisible = true;
        }

        private void OpenRecoverModal()
        {
            CurrentModal = new RecoverAccountViewModel(CloseModal);
            IsModalVisible = true;
        }

        private void ExitGame()
        {
            Application.Current.Shutdown();
        }
    }
}
