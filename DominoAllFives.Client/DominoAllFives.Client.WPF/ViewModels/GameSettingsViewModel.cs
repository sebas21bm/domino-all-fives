using System;
using System.ServiceModel;
using System.Windows;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Threading;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFivesService;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Displays game settings and coordinates account language changes.</summary>
    public class GameSettingsViewModel : ViewModelBase
    {
        private const string SpanishLanguageCode = "es-MX";
        private const string EnglishLanguageCode = "en-US";
        private const string PortugueseLanguageCode = "pt-BR";

        private readonly PlayerSession _playerSession;
        private IFrameNavigationService _navigationService;
        private ViewModelFactory _viewModelFactory;
        private IDialogService _dialogService;
        private IAccountClient _accountClient;
        private ILogger<GameSettingsViewModel> _logger;
        private ViewModelBase _currentModal;
        private string _selectedLanguageCode;
        private bool _isAccountSectionVisible;
        private bool _isLanguageSectionVisible;
        private bool _isBusy;

        public ViewModelBase CurrentModal
        {
            get
            {
                return _currentModal;
            }
            private set
            {
                if (SetProperty(ref _currentModal, value))
                {
                    OnPropertyChanged(nameof(IsModalVisible));
                    GoBackCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsModalVisible
        {
            get
            {
                return CurrentModal != null;
            }
        }

        public bool IsAccountSectionVisible
        {
            get
            {
                return _isAccountSectionVisible;
            }
            private set
            {
                SetProperty(ref _isAccountSectionVisible, value);
            }
        }

        public bool IsLanguageSectionVisible
        {
            get
            {
                return _isLanguageSectionVisible;
            }
            private set
            {
                SetProperty(ref _isLanguageSectionVisible, value);
            }
        }

        private string SelectedLanguageCode
        {
            get
            {
                return _selectedLanguageCode;
            }
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

        public bool IsSpanishSelected
        {
            get
            {
                return SelectedLanguageCode == SpanishLanguageCode;
            }
        }

        public bool IsEnglishSelected
        {
            get
            {
                return SelectedLanguageCode == EnglishLanguageCode;
            }
        }

        public bool IsPortugueseSelected
        {
            get
            {
                return SelectedLanguageCode == PortugueseLanguageCode;
            }
        }

        private bool IsGuest
        {
            get
            {
                return !_playerSession.IsAuthenticated;
            }
        }

        public Visibility AccountFeaturesVisibility
        {
            get
            {
                return _playerSession.HasServerSession ? Visibility.Visible : Visibility.Collapsed;
            }
        }

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
            _playerSession = playerSession;
            _selectedLanguageCode = string.Empty;
            ShowAccountSectionCommand = new RelayCommand(ShowAccountSection, CanOpenAccountAction);
            ShowLanguageSectionCommand = new RelayCommand(ShowLanguageSection, CanOpenLanguage);
            SelectLanguageCommand = new RelayCommand(SelectLanguage, CanSelectLanguage);
            SaveLanguageCommand = new RelayCommand(SaveLanguage, CanSaveLanguage);
            GoBackCommand = new RelayCommand(GoBack, CanUseSettings);
            LogoutCommand = new RelayCommand(IgnoreUnavailableCommand, CanLogout);
            GoToChangePassword = new RelayCommand(OpenChangePassword, CanOpenAccountAction);
            GoToDeleteAccount = new RelayCommand(ConfirmAccountDeletion, CanOpenAccountAction);
        }

        /// <summary>Configures diagnostics before initializing the settings services.</summary>
        /// <param name = "logger">The settings logger.</param>
        public void ConfigureLogger(ILogger<GameSettingsViewModel> logger)
        {
            _logger = logger;
        }

        /// <summary>Initializes the services used by the settings commands.</summary>
        /// <param name = "navigationService">The application navigation service.</param>
        /// <param name = "viewModelFactory">The factory for account dialogs.</param>
        /// <param name = "dialogService">The localized message service.</param>
        /// <param name = "accountClient">The remote account client.</param>
        public void Initialize(
            IFrameNavigationService navigationService,
            ViewModelFactory viewModelFactory,
            IDialogService dialogService,
            IAccountClient accountClient)
        {
            _navigationService = navigationService;
            _viewModelFactory = viewModelFactory;
            _dialogService = dialogService;
            IsAccountSectionVisible = _playerSession.HasServerSession;
            IsLanguageSectionVisible = IsGuest;
            SelectedLanguageCode = LanguageManager.Instance.CurrentLanguageCode;
            _accountClient = accountClient;
            GoBackCommand.RaiseCanExecuteChanged();
        }

        private static bool CanLogout()
        {
            return false;
        }

        private static void IgnoreUnavailableCommand()
        {
        }

        private bool CanUseSettings()
        {
            return _accountClient != null && !_isBusy && !IsModalVisible;
        }

        private bool CanOpenAccountAction()
        {
            return CanUseSettings() && _playerSession.HasServerSession;
        }

        private bool CanOpenLanguage()
        {
            return CanUseSettings() && (IsGuest || _playerSession.HasServerSession);
        }

        private bool CanSelectLanguage(object parameter)
        {
            return CanOpenLanguage() && IsLanguageSectionVisible && parameter is string;
        }

        private bool CanSaveLanguage()
        {
            return CanOpenLanguage() && IsLanguageSectionVisible
                && !string.IsNullOrWhiteSpace(SelectedLanguageCode);
        }

        private void ShowAccountSection()
        {
            if (!CanOpenAccountAction())
            {
                return;
            }

            IsAccountSectionVisible = true;
            IsLanguageSectionVisible = false;
        }

        private void ShowLanguageSection()
        {
            if (!CanOpenLanguage() || IsLanguageSectionVisible)
            {
                return;
            }

            SelectedLanguageCode = LanguageManager.Instance.CurrentLanguageCode;
            IsAccountSectionVisible = false;
            IsLanguageSectionVisible = true;
        }

        private void SelectLanguage(object parameter)
        {
            if (CanSelectLanguage(parameter))
            {
                SelectedLanguageCode = (string)parameter;
            }
        }

        private void SaveLanguage()
        {
            if (!CanSaveLanguage())
            {
                return;
            }

            string preferredLanguageId = SelectedLanguageCode;
            if (IsGuest)
            {
                StartDeviceLanguageSave(preferredLanguageId);
                return;
            }

            string sessionKey = _playerSession.SessionKey;
            BackgroundJob<(string SessionKey, string PreferredLanguageId), bool> job =
                new BackgroundJob<(string SessionKey, string PreferredLanguageId), bool>(
                    SaveAccountLanguage,
                    (sessionKey, preferredLanguageId),
                    CompleteLanguageChange,
                    HandleLanguageFailure);
            SetBusy(true);
            BackgroundWork.Start(job);
        }

        private void StartDeviceLanguageSave(string preferredLanguageId)
        {
            BackgroundJob<string, string> job = new BackgroundJob<string, string>(
                LanguageManager.Instance.PersistDeviceLanguage,
                preferredLanguageId,
                CompleteDeviceLanguageChange,
                HandleDeviceLanguageFailure);
            SetBusy(true);
            BackgroundWork.Start(job);
        }

        private void CompleteDeviceLanguageChange(string savedLanguage, string unusedRequest)
        {
            SetBusy(false);
            if (IsGuest)
            {
                LanguageManager.Instance.ApplyLanguage(savedLanguage);
            }
        }

        private void HandleDeviceLanguageFailure(Exception ex, string unusedRequest)
        {
            SetBusy(false);
            _logger.LogError(ex, "Device language could not be saved.");
            if (IsGuest)
            {
                ShowMessageFailure(deviceSaveFailure: true);
            }
        }

        private bool SaveAccountLanguage((string SessionKey, string PreferredLanguageId) request)
        {
            _accountClient.ChangePreferredLanguage(
                request.SessionKey, request.PreferredLanguageId);
            return true;
        }

        private void CompleteLanguageChange(
            bool unusedResult,
            (string SessionKey, string PreferredLanguageId) request)
        {
            SetBusy(false);
            if (_playerSession.SessionKey != request.SessionKey)
            {
                return;
            }

            _playerSession.UpdatePreferredLanguage(request.PreferredLanguageId);
            LanguageManager.Instance.ApplyAccountLanguage(request.PreferredLanguageId);
        }

        private void HandleLanguageFailure(
            Exception ex,
            (string SessionKey, string PreferredLanguageId) request)
        {
            SetBusy(false);
            if (_playerSession.SessionKey != request.SessionKey)
            {
                return;
            }

            if (ex is FaultException<AccountIssue> accountFault)
            {
                _logger.LogDebug(
                    ex, "Preferred language change rejected. Account issue: {AccountIssue}.",
                    accountFault.Detail);
                ShowMessageFailure(accountFault.Detail);
            }
            else if (ex is FaultException<AccessIssue> accessFault)
            {
                _logger.LogDebug(
                    ex, "Preferred language change rejected. Access issue: {AccessIssue}.",
                    accessFault.Detail);
                ShowMessageFailure(accessIssue: accessFault.Detail);
            }
            else
            {
                _logger.LogError(ex, "Preferred language change could not complete.");
                bool connectionFailure = ex is TimeoutException
                    || (ex is CommunicationException && !(ex is FaultException));
                ShowMessageFailure(connectionFailure: connectionFailure);
            }
        }

        private void ShowMessageFailure(
            AccountIssue? issue = null,
            AccessIssue? accessIssue = null,
            bool connectionFailure = false,
            bool deviceSaveFailure = false)
        {
            DialogRequest request;
            switch (issue)
            {
                case AccountIssue.UnsupportedLanguage:
                    request = new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "Global_msgDefaultErrorTitle",
                        MessageKey = "MessageLanguage_msgSaveFailed"
                    };
                    break;
                case AccountIssue.PlayerNotFound:
                case null when accessIssue == AccessIssue.SessionRequired:
                    request = new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageAccount_msgAccountNotFoundTitle",
                        MessageKey = "MessageAccount_msgAccountNotFound"
                    };
                    break;
                case null when connectionFailure:
                    request = new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey = "Global_msgConnectionErrorTitle",
                        MessageKey = "MessageAuthentication_msgConnectionError"
                    };
                    break;
                case null when deviceSaveFailure:
                    request = new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey = "Global_msgDefaultErrorTitle",
                        MessageKey = "MessageLanguage_msgSaveFailed"
                    };
                    break;
                default:
                    request = new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey = "Global_msgDefaultErrorTitle",
                        MessageKey = "Global_msgDefaultError"
                    };
                    break;
            }

            _dialogService.ShowDialog(request);
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;
            SaveLanguageCommand.RaiseCanExecuteChanged();
        }

        private void GoBack()
        {
            if (CanUseSettings())
            {
                SelectedLanguageCode = LanguageManager.Instance.CurrentLanguageCode;
                _navigationService.GoBack();
            }
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
                OnAccept = OpenConfirmedDeletion
            });

            void OpenConfirmedDeletion()
            {
                if (CanOpenAccountAction() && _playerSession.SessionKey == sessionKey)
                {
                    CurrentModal = _viewModelFactory.CreateDeleteAccount(
                        CompleteAccountDeletion, CloseModal);
                }
            }
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
    }
}
