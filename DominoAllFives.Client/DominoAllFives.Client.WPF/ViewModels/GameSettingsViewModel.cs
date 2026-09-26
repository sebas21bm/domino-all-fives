using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class GameSettingsViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;

        private bool _isAccountSectionVisible;
        private bool _isLanguageSectionVisible;
        private string _selectedLanguageCode;

        public bool IsAccountSectionVisible
        {
            get => _isAccountSectionVisible;
            set => SetProperty(ref _isAccountSectionVisible, value);
        }

        public bool IsLanguageSectionVisible
        {
            get => _isLanguageSectionVisible;
            set => SetProperty(ref _isLanguageSectionVisible, value);
        }

        public string SelectedLanguageCode
        {
            get => _selectedLanguageCode;
            set
            {
                if (SetProperty(ref _selectedLanguageCode, value))
                {
                    OnPropertyChanged(nameof(IsSpanishSelected));
                    OnPropertyChanged(nameof(IsEnglishSelected));
                    OnPropertyChanged(nameof(IsPortugueseSelected));
                }
            }
        }

        public bool IsSpanishSelected => SelectedLanguageCode == "es-MX";
        public bool IsEnglishSelected => SelectedLanguageCode == "en-US";
        public bool IsPortugueseSelected => SelectedLanguageCode == "pt-BR";

        public RelayCommand ShowAccountSectionCommand { get; }
        public RelayCommand ShowLanguageSectionCommand { get; }
        public RelayCommand SelectLanguageCommand { get; }
        public RelayCommand SaveLanguageCommand { get; }
        public RelayCommand GoBackCommand { get; }
        public RelayCommand LogoutCommand { get; }


        public GameSettingsViewModel(IFrameNavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            _selectedLanguageCode = LanguageManager.Instance.CurrentLanguageCode;
            if (string.IsNullOrWhiteSpace(_selectedLanguageCode))
            {
                _selectedLanguageCode = "es-MX";
            }

            _isAccountSectionVisible = true;
            _isLanguageSectionVisible = false;

            ShowAccountSectionCommand = new RelayCommand(_ => ExecuteShowAccountSection());
            ShowLanguageSectionCommand = new RelayCommand(_ => ExecuteShowLanguageSection());
            SelectLanguageCommand = new RelayCommand(ExecuteSelectLanguage);
            SaveLanguageCommand = new RelayCommand(_ => ExecuteSaveLanguage());

            GoBackCommand = new RelayCommand(_ => _navigationService.NavigateTo<MainMenuViewModel>());
            LogoutCommand = new RelayCommand(_ => ExecuteLogout());
        }

        private void ExecuteShowAccountSection()
        {
            IsAccountSectionVisible = true;
            IsLanguageSectionVisible = false;
        }

        private void ExecuteShowLanguageSection()
        {
            IsAccountSectionVisible = false;
            IsLanguageSectionVisible = true;
        }

        private void ExecuteSelectLanguage(object parameter)
        {
            if (parameter is string languageCode)
            {
                SelectedLanguageCode = languageCode;
            }
        }

        private void ExecuteSaveLanguage()
        {
            if (!string.IsNullOrWhiteSpace(SelectedLanguageCode))
            {
                LanguageManager.Instance.ChangeLanguage(SelectedLanguageCode);
            }
        }

        private void ExecuteLogout()
        {
            // TODO: Invalidad sesión en el cliente
            _navigationService.NavigateTo<HomePageViewModel>();
        }
    }
}
