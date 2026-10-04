using System;
using System.Windows;

using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly PlayerSession _playerSession;

        public string Username { get; private set; }
        public bool IsGuest => !_playerSession.IsAuthenticated;
        public Visibility AccountFeaturesVisibility => IsGuest ? 
            Visibility.Collapsed : Visibility.Visible;

        public RelayCommand GoToProfileCommand { get; }
        public RelayCommand GoToFriendsCommand { get; }
        public RelayCommand GoToSettingsCommand { get; }
        public RelayCommand ExitGameCommand { get; }
        public RelayCommand ShowHowToPlayCommand { get; }
        public RelayCommand ShowLeaderboardCommand { get; }
        public RelayCommand GoToCreateGameCommand {  get; }
        public RelayCommand GoToJoinRoomCommand { get; }

        public MainMenuViewModel(IFrameNavigationService navigationService, IDialogService dialogService, 
            PlayerSession playerSession)
        {
            _navigationService = navigationService ?? 
                throw new ArgumentNullException(nameof(navigationService));
            _dialogService = dialogService ?? 
                throw new ArgumentNullException(nameof(dialogService));
            _playerSession = playerSession ?? 
                throw new ArgumentNullException(nameof(playerSession));

            GoToProfileCommand = new RelayCommand(
                _ => _navigationService.NavigateTo<ProfileViewModel>());
            GoToFriendsCommand = new RelayCommand(
                _ => _navigationService.NavigateTo<FriendsViewModel>());
            GoToSettingsCommand = new RelayCommand(
                _ => _navigationService.NavigateTo<GameSettingsViewModel>());
            ShowHowToPlayCommand = new RelayCommand(
                _ => _navigationService.NavigateTo<HowToPlayViewModel>());
            ShowLeaderboardCommand = new RelayCommand(
                _ => _navigationService.NavigateTo<RankingsViewModel>());
            GoToCreateGameCommand = new RelayCommand(
                _ => _navigationService.NavigateTo<CreateGameViewModel>());
            GoToJoinRoomCommand = new RelayCommand(
                _ => _navigationService.NavigateTo<JoinGameViewModel>());
            ExitGameCommand = new RelayCommand(_ => Application.Current.Shutdown());
        }
    }
}
