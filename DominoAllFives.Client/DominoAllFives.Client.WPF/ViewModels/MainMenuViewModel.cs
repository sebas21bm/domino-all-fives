using System;
using System.Windows;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        private IFrameNavigationService _navigationService;
        private readonly PlayerSession _playerSession;
        public string Username
        {
            get
            {
                return _playerSession.Username;
            }
        }
        public bool IsGuest
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
                return IsGuest ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        public RelayCommand GoToProfileCommand { get; }
        public RelayCommand GoToFriendsCommand { get; }
        public RelayCommand GoToSettingsCommand { get; }
        public RelayCommand LogoutCommand { get; }
        public RelayCommand ShowHowToPlayCommand { get; }
        public RelayCommand ShowLeaderboardCommand { get; }
        public RelayCommand GoToCreateGameCommand { get; }
        public RelayCommand GoToJoinRoomCommand { get; }

        public MainMenuViewModel(PlayerSession playerSession)
        {
            _playerSession = playerSession ?? throw new ArgumentNullException(
                nameof(playerSession));
            GoToProfileCommand = new RelayCommand(
                unusedParameter => _navigationService.NavigateTo<ProfileViewModel>());
            GoToFriendsCommand = new RelayCommand(
                unusedParameter => _navigationService.NavigateTo<FriendsViewModel>());
            GoToSettingsCommand = new RelayCommand(
                unusedParameter => _navigationService.NavigateTo<GameSettingsViewModel>());
            ShowHowToPlayCommand = new RelayCommand(
                unusedParameter => _navigationService.NavigateTo<HowToPlayViewModel>());
            ShowLeaderboardCommand = new RelayCommand(
                unusedParameter => _navigationService.NavigateTo<RankingsViewModel>());
            GoToCreateGameCommand = new RelayCommand(
                unusedParameter => _navigationService.NavigateTo<CreateGameViewModel>());
            GoToJoinRoomCommand = new RelayCommand(
                unusedParameter => _navigationService.NavigateTo<JoinGameViewModel>());
            LogoutCommand = new RelayCommand(unusedParameter => ExecuteLogout());
        }

        public void Initialize(IFrameNavigationService navigationService)
        {
            if (_navigationService != null)
            {
                throw new InvalidOperationException(
                    "MainMenuViewModel has already been initialized.");
            }

            if (navigationService == null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            _navigationService = navigationService;
        }

        private void ExecuteLogout()
        {
            _playerSession.Clear();
            LanguageManager.Instance.RestoreDeviceLanguage();
            _navigationService.NavigateAsRoot<HomePageViewModel>();
        }
    }
}
