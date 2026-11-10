using System;
using System.ServiceModel;
using LobbyService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Interfaces
{
    public interface ILobbyClient : IDisposable
    {
        event Action<Lobby> LobbyChanged;
        event Action LobbyClosed;
        event Action<int> MatchStarted;

        CommunicationState State { get; }
        void Open();
        Lobby CreateLobby(LobbySettings settings);
        Lobby JoinLobby(string lobbyCode);
        Lobby GetLobby(string lobbyCode);
        void LeaveLobby();
        void CloseLobby();
        void StartMatch();
        void CloseConnection();
    }
}
