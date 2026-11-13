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

        Lobby CurrentLobby { get; }

        bool IsActive { get; }

        bool WasCreatedByThisClient { get; }

        bool WasClosed { get; }

        int ProvisionalMatchId { get; }

        Lobby CreateLobby(LobbySettings settings);
        Lobby JoinLobby(string lobbyCode);
        Lobby GetLobby(string lobbyCode);
        void LeaveLobby();
        void StartMatch();
        void CloseLobby();
        void Reset();
    }
}
