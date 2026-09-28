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

        private ViewModelBase _currentModal;
        private bool _isModalVisible;

        public ViewModelBase CurrentModal
        {
            get => _currentModal;
            private set => SetProperty(ref _currentModal, value);
        }

        public bool IsModalVisible
        {
            get { return _isModalVisible; }
            private set { SetProperty(ref _isModalVisible, value); }
        }
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
        public RelayCommand GoToChangePassword {  get; }
        public RelayCommand GoToDeleteAccount { get; }


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
            GoToChangePassword = new RelayCommand(_ => OpenChangePasswordModal());
            GoToDeleteAccount = new RelayCommand(_ => OpenDeleteAccount());

            GoBackCommand = new RelayCommand(_ => _navigationService.GoBack());
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

        private void OpenChangePasswordModal()
        {
            CurrentModal = new ChangePasswordViewModel(
                onPasswordChangedSuccess: OnPasswordChangeFromSettingsSuccess,
                onCancel: CloseModal,
                isFromSettings: true
            );

            IsModalVisible = true;
        }

        private void OnPasswordChangeFromSettingsSuccess()
        {
            CloseModal();
        }

        private void OpenDeleteAccount()
        {
            CurrentModal = new DeleteAccountViewModel(
                onAccountDeletedSuccess: OnAccountDeletedSuccess,
                onCancel: CloseModal
            );
            IsModalVisible = true;
        }

        private void OnAccountDeletedSuccess()
        {
            CloseModal();
            // TODO: Eliminar el registro en la BD
            _navigationService.NavigateTo<HomePageViewModel>();
        }

        private void CloseModal()
        {
            CurrentModal = null;
            IsModalVisible = false;
        }

        private void ExecuteLogout()
        {
            // TODO: Invalidad sesión en el cliente
            _navigationService.NavigateTo<HomePageViewModel>();
        }
    }
}
