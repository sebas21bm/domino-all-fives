using System;
using System.Windows;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Displays game settings and opens account management dialogs.</summary>
    public class GameSettingsViewModel : ViewModelBase
    {
        private readonly PlayerSession _playerSession;
        private IFrameNavigationService _navigationService;
        private ViewModelFactory _viewModelFactory;
        private IDialogService _dialogService;
        private ViewModelBase _currentModal;

        public ViewModelBase CurrentModal
        {
            get => _currentModal;
            private set
            {
                if (SetProperty(ref _currentModal, value))
                {
                    OnPropertyChanged(nameof(IsModalVisible));
                    GoBackCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsModalVisible => CurrentModal != null;
        public bool IsAccountSectionVisible
        {
            get => _isAccountSectionVisible;
            private set => SetProperty(ref _isAccountSectionVisible, value);
        }

        private bool _isAccountSectionVisible;
        public bool IsLanguageSectionVisible { get; set; }
        public string SelectedLanguageCode { get; set; } = string.Empty;
        public bool IsSpanishSelected { get; set; }
        public bool IsEnglishSelected { get; set; }
        public bool IsPortugueseSelected { get; set; }
        public bool IsGuest => !_playerSession.IsAuthenticated;
        public Visibility AccountFeaturesVisibility =>
            _playerSession.HasServerSession ? Visibility.Visible : Visibility.Collapsed;
        public RelayCommand ShowAccountSectionCommand { get; }
        public RelayCommand ShowLanguageSectionCommand { get; }
        public RelayCommand SelectLanguageCommand { get; }
        public RelayCommand SaveLanguageCommand { get; }
        public RelayCommand GoBackCommand { get; }
        public RelayCommand LogoutCommand { get; }
        public RelayCommand GoToChangePassword { get; }
        public RelayCommand GoToDeleteAccount { get; }

        public GameSettingsViewModel(PlayerSession playerSession)
        {
            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));
            ShowAccountSectionCommand = new RelayCommand(ShowAccountSection, CanOpenAccountAction);
            ShowLanguageSectionCommand = new RelayCommand(IgnoreUnavailableCommand, () => false);
            SelectLanguageCommand = new RelayCommand(IgnoreUnavailableCommand, () => false);
            SaveLanguageCommand = new RelayCommand(IgnoreUnavailableCommand, () => false);
            GoBackCommand = new RelayCommand(() => _navigationService.GoBack(),
                () => _navigationService != null && !IsModalVisible);
            LogoutCommand = new RelayCommand(IgnoreUnavailableCommand, () => false);
            GoToChangePassword = new RelayCommand(OpenChangePassword, CanOpenAccountAction);
            GoToDeleteAccount = new RelayCommand(ConfirmAccountDeletion, CanOpenAccountAction);
        }

        public void Initialize(
            IFrameNavigationService navigationService,
            ViewModelFactory viewModelFactory,
            IDialogService dialogService)
        {
            if (_navigationService != null)
            {
                throw new InvalidOperationException(
                    "GameSettingsViewModel has already been initialized.");
            }

            _dialogService = dialogService 
                ?? throw new ArgumentNullException(nameof(dialogService));
            _viewModelFactory = viewModelFactory 
                ?? throw new ArgumentNullException(nameof(viewModelFactory));
            _navigationService = navigationService 
                ?? throw new ArgumentNullException(nameof(navigationService));
            IsAccountSectionVisible = _playerSession.HasServerSession;
        }

        private bool CanOpenAccountAction()
        {
            return _navigationService != null && _playerSession.HasServerSession && !IsModalVisible;
        }

        private void ShowAccountSection()
        {
            IsAccountSectionVisible = true;
        }

        private void OpenChangePassword()
        {
            if (CanOpenAccountAction())
            {
                CurrentModal = _viewModelFactory.CreateChangePassword(CloseModal, CloseModal);
            }
        }

        private void ConfirmAccountDeletion()
        {
            if (!CanOpenAccountAction())
            {
                return;
            }

            string sessionKey = _playerSession.SessionKey;
            _dialogService.ShowDialog(new DialogRequest
            {
                Type = DialogType.Confirmation,
                TitleKey = "MessageAccount_msgDeleteAccountTitle",
                MessageKey = "MessageAccount_msgDeleteConfirm",
                OnAccept = () =>
                {
                    if (CanOpenAccountAction() && _playerSession.SessionKey == sessionKey)
                    {
                        CurrentModal = _viewModelFactory.CreateDeleteAccount(
                            CompleteAccountDeletion, CloseModal);
                    }
                }
            });
        }

        private void CompleteAccountDeletion()
        {
            CloseModal();
            LanguageManager.Instance.RestoreDeviceLanguage();
            _navigationService.NavigateAsRoot<HomePageViewModel>();
        }

        private void CloseModal()
        {
            CurrentModal = null;
        }

        private static void IgnoreUnavailableCommand()
        {
        }
    }
}
