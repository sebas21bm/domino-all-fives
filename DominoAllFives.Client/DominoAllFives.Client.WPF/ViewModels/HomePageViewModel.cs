using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;

        private ViewModelBase _currentModal;
        private bool _isModalVisible;
        private string _selectedLanguage;

        public ViewModelBase CurrentModal
        {
            get { return _currentModal; }
            private set { SetProperty(ref _currentModal, value); }
        }

        public bool IsModalVisible
        {
            get { return _isModalVisible; }
            private set { SetProperty(ref _isModalVisible, value); }
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (SetProperty(ref _selectedLanguage, value) && !string.IsNullOrEmpty(value))
                {
                    OnLanguageChanged(value);
                }
            }
        }
        public List<LanguageOption> AvailableLanguages { get; }

        public RelayCommand ShowLoginCommand { get; }
        public RelayCommand PlayAsGuestCommand { get; }
        public RelayCommand ExitGameCommand { get; }

        public HomePageViewModel(IFrameNavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            AvailableLanguages = new List<LanguageOption>
            {
                new LanguageOption { Code = "es-MX", DisplayName = "Español (MX)" },
                new LanguageOption { Code = "en-US", DisplayName = "English (USA)" },
                new LanguageOption { Code = "pt-BR", DisplayName = "Português (BR)" }
            };

            _selectedLanguage = LanguageManager.Instance.CurrentLanguageCode;

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
                onLoginSuccess: () => _navigationService.NavigateTo<MainMenuViewModel>(),
                onCancel: CloseModal,
                onGoToRegister: OpenRegisterModal,
                onGoToRecover: OpenRecoverModal
            );
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
            // Instantiates RegisterAccountViewModel with its specific forward/backward flows
        }

        private void OpenRecoverModal()
        {
            // Instantiates RecoverAccountViewModel with its specific forward/backward flows
        }

        private void PlayAsGuest()
        {
            // TODO: Implement guest logic and navigate to MainMenu
            // _navigationService.NavigateTo<MainMenuViewModel>();
        }

        private void ExitGame()
        {
            Application.Current.Shutdown();
        }
    }
}
