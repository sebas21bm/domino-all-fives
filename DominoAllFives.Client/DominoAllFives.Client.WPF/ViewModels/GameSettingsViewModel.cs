using System;
using System.Windows;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Controls;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class GameSettingsViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IAccountService _accountService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly PlayerSession _playerSession;

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

        public bool IsGuest => !_playerSession.IsAuthenticated;
        public Visibility AccountFeaturesVisibility => IsGuest ? 
            Visibility.Collapsed : Visibility.Visible;

        public RelayCommand ShowAccountSectionCommand { get; }
        public RelayCommand ShowLanguageSectionCommand { get; }
        public RelayCommand SelectLanguageCommand { get; }
        public RelayCommand SaveLanguageCommand { get; }
        public RelayCommand GoBackCommand { get; }
        public RelayCommand LogoutCommand { get; }
        public RelayCommand GoToChangePassword {  get; }
        public RelayCommand GoToDeleteAccount { get; }


        public GameSettingsViewModel(IFrameNavigationService navigationService,
            IDialogService dialogService,
            IAccountService accountService,
            ILoggerFactory loggerFactory,
            PlayerSession playerSession)
        {
            _navigationService = navigationService 
                ?? throw new ArgumentNullException(nameof(navigationService));

            _dialogService = dialogService
                ?? throw new ArgumentNullException(nameof(dialogService));

            _accountService = accountService
                ?? throw new ArgumentNullException(nameof(accountService));

            _loggerFactory = loggerFactory
                ?? throw new ArgumentNullException(
                    nameof(loggerFactory));

            _playerSession = playerSession 
                ?? throw new ArgumentNullException(nameof(playerSession));

            _selectedLanguageCode = LanguageManager.Instance.CurrentLanguageCode;

            if (string.IsNullOrWhiteSpace(_selectedLanguageCode))
            {
                _selectedLanguageCode = "es-MX";
            }

            if (IsGuest)
            {
                _isAccountSectionVisible = false;
                _isLanguageSectionVisible = true;
            }
            else
            {
                _isAccountSectionVisible = true;
                _isLanguageSectionVisible = false;
            }

            ShowAccountSectionCommand = new RelayCommand(ExecuteShowAccountSection);
            ShowLanguageSectionCommand = new RelayCommand(ExecuteShowLanguageSection);
            SelectLanguageCommand = new RelayCommand(ExecuteSelectLanguage);
            SaveLanguageCommand = new RelayCommand(ExecuteSaveLanguage);
            GoToChangePassword = new RelayCommand(OpenChangePasswordModal);
            GoToDeleteAccount = new RelayCommand(OpenDeleteAccount);

            GoBackCommand = new RelayCommand(_navigationService.GoBack);
            LogoutCommand = new RelayCommand(ExecuteLogout);
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
            _dialogService.ShowDialog(new DialogRequest
            {
                Type = DialogType.Confirmation,
                TitleKey = "MessageAccount_msgDeleteAccountTitle",
                MessageKey = "MessageAccount_msgDeleteConfirm",
                OnAccept = ShowDeleteAccountModal
            });
        }

        private void ShowDeleteAccountModal()
        {
            CurrentModal = new DeleteAccountViewModel(
                _dialogService,
                _accountService,
                _loggerFactory.CreateLogger<DeleteAccountViewModel>(),
                _playerSession,
                OnAccountDeletedSuccess,
                CloseModal);

            IsModalVisible = true;
        }

        private void OnAccountDeletedSuccess()
        {
            CloseModal();
            _playerSession.Clear();
            _navigationService.NavigateAsRoot<HomePageViewModel>();
        }

        private void CloseModal()
        {
            CurrentModal = null;
            IsModalVisible = false;
        }

        private void ExecuteLogout()
        {
            _playerSession.Clear();
            _navigationService.NavigateAsRoot<HomePageViewModel>();
        }
    }
}
