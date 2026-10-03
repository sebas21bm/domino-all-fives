using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class LobbyViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;

        private string _roomCode;
        private bool _isHost = true;
        private ViewModelBase _currentModal;
        private bool _isModalVisible;
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

        public string RoomCode
        {
            get => _roomCode;
            set => SetProperty(ref _roomCode, value);
        }

        public bool IsHost
        {
            get => _isHost;
            set => SetProperty(ref _isHost, value);
        }

        public ObservableCollection<LobbyPlayerRecord> Players { get; set; }

        public RelayCommand GoBackCommand { get; }
        public RelayCommand InviteFriendsCommand { get; }
        public RelayCommand StartGameCommand { get; }
        public RelayCommand ViewProfileCommand { get; }
        public RelayCommand KickPlayerCommand { get; }


        public LobbyViewModel(IFrameNavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            GoBackCommand = new RelayCommand(_ => _navigationService.GoBack());
            InviteFriendsCommand = new RelayCommand(ExecuteInviteFriends);
            StartGameCommand = new RelayCommand(ExecuteStartGame);
            ViewProfileCommand = new RelayCommand(ExecuteViewProfile);
            KickPlayerCommand = new RelayCommand(ExecuteKickPlayer);

            Players = new ObservableCollection<LobbyPlayerRecord>();
        }
        private void ExecuteInviteFriends()
        {
            CurrentModal = new InviteFriendsViewModel(CloseModal);
            IsModalVisible = true;
        }
        private void CloseModal()
        {
            CurrentModal = null;
            IsModalVisible = false;
        }

        private void ExecuteStartGame()
        {
            CurrentModal = new StartingGameViewModel();
            IsModalVisible = true;
        }

        private void ExecuteViewProfile(object parameter)
        {
            if (parameter is LobbyPlayerRecord player)
            {

            }
        }

        private void ExecuteKickPlayer(object parameter)
        {
            if (parameter is LobbyPlayerRecord player)
            {

                Players.Remove(player);
            }
        }

    }
}
