using System;
using System.Collections.Generic;
using System.Windows;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        private IFrameNavigationService _navigationService;

        private ViewModelFactory _viewModelFactory;

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

        public HomePageViewModel(PlayerSession playerSession)
        {

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

        public void Initialize(
            IFrameNavigationService navigationService,
            ViewModelFactory viewModelFactory)
        {
            if (_navigationService != null)
            {
                throw new InvalidOperationException(
                    "HomePageViewModel has already been initialized.");
            }

            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));
            if (viewModelFactory == null)
                throw new ArgumentNullException(nameof(viewModelFactory));

            _navigationService = navigationService;
            _viewModelFactory = viewModelFactory;
        }

        private void OnLanguageChanged(string cultureCode)
        {
            LanguageManager.Instance.ChangeLanguage(cultureCode);
        }

        private void OpenLoginModal()
        {
            CurrentModal = _viewModelFactory.CreateLogin(
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
            CurrentModal = _viewModelFactory.CreateRegisterAccount(
                CloseModal);

            IsModalVisible = true;
        }

        private void OpenRecoverModal()
        {
            CurrentModal = _viewModelFactory.CreateRecoverAccount(
                CloseModal);

            IsModalVisible = true;
        }

        private void ExitGame()
        {
            Application.Current.Shutdown();
        }
    }
}
