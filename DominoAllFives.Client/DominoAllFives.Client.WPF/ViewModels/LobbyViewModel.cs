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

            // Cargar datos mockup de 4 jugadores para pruebas
            LoadMockPlayers();
        }
        private void ExecuteInviteFriends()
        {
            // Lógica para abrir el modal o pantalla de invitar amigos (CU-19)
        }

        private void ExecuteStartGame()
        {
            // Lógica para iniciar la partida (CU-21)
            // _navigationService?.NavigateTo<GameBoardViewModel>();
        }

        private void ExecuteViewProfile(object parameter)
        {
            if (parameter is LobbyPlayerRecord player)
            {
                // Lógica para abrir el perfil del jugador seleccionado (HU-CU-11)
            }
        }

        private void ExecuteKickPlayer(object parameter)
        {
            if (parameter is LobbyPlayerRecord player)
            {
                // Remover jugador de la lista dinámica de prueba (CU-20)
                Players.Remove(player);
            }
        }

        private void LoadMockPlayers()
        {
            // Jugador 1: Anfitrión y usuario local
            Players.Add(new LobbyPlayerRecord
            {
                Username = "pepe123",
                AvatarPath = "/Assets/Images/Avatars/avatar1.png",
                IsHost = true,
                IsCurrentPlayer = true
            });

            // Jugador 2: Invitado
            Players.Add(new LobbyPlayerRecord
            {
                Username = "nickname29",
                AvatarPath = "/Assets/Images/Avatars/avatar2.png",
                IsHost = false,
                IsCurrentPlayer = false
            });

            // Jugador 3: Invitado
            Players.Add(new LobbyPlayerRecord
            {
                Username = "sebas_wow",
                AvatarPath = "/Assets/Images/Avatars/avatar3.png",
                IsHost = false,
                IsCurrentPlayer = false
            });

            // Jugador 4: Invitado
            Players.Add(new LobbyPlayerRecord
            {
                Username = "vegeta777",
                AvatarPath = "/Assets/Images/Avatars/avatar4.png",
                IsHost = false,
                IsCurrentPlayer = false
            });
        }
    }
}
