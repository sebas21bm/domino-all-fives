using System;
using System.ServiceModel;
using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Callbacks
{
    [CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple)]
    internal sealed class LobbyCallbackHandler : ILobbyServiceCallback
    {
        public event Action<Lobby> LobbyChanged;
        public event Action LobbyClosed;
        public event Action<int> MatchStarted;

        public void OnLobbyChanged(Lobby lobby)
        {
            LobbyChanged?.Invoke(lobby);
        }

        public void OnLobbyClosed()
        {
            LobbyClosed?.Invoke();
        }

        public void OnMatchStarted(int matchId)
        {
            MatchStarted?.Invoke(matchId);
        }
    }
}
