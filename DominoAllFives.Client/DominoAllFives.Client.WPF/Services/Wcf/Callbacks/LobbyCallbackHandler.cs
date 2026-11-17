using System;
using LobbyService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Callbacks
{
    internal sealed class LobbyCallbackHandler : ILobbyServiceCallback
    {
        public event Action<LobbyCallbackHandler, Lobby> LobbyChanged;
        public event Action<LobbyCallbackHandler> LobbyClosed;
        public event Action<LobbyCallbackHandler, int> MatchStarted;

        public void OnLobbyChanged(Lobby lobby)
        {
            LobbyChanged?.Invoke(this, lobby);
        }

        public void OnLobbyClosed()
        {
            LobbyClosed?.Invoke(this);
        }

        public void OnMatchStarted(int matchId)
        {
            MatchStarted?.Invoke(this, matchId);
        }
    }
}
