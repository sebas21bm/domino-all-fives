using System;
using System.ServiceModel;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services.Wcf.Callbacks;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using LobbyService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Services
{
    public sealed class WcfLobbyService : ILobbyClient
    {
        private readonly PlayerSession _playerSession;
        private readonly LobbyCallbackHandler _callback = new LobbyCallbackHandler();
        private LobbyServiceClient _client;

        public event Action<Lobby> LobbyChanged;
        public event Action LobbyClosed;
        public event Action<int> MatchStarted;
        public CommunicationState State => _client?.State ?? CommunicationState.Closed;

        public WcfLobbyService(PlayerSession playerSession)
        {
            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));
            _callback.LobbyChanged += lobby => LobbyChanged?.Invoke(lobby);
            _callback.LobbyClosed += () => LobbyClosed?.Invoke();
            _callback.MatchStarted += matchId => MatchStarted?.Invoke(matchId);
        }

        public void Open()
        {
            if (!_playerSession.HasServerSession)
                throw new InvalidOperationException("A server session is required.");
            if (_client != null) throw new InvalidOperationException("Connection already opened.");
            _client = new LobbyServiceClient(new InstanceContext(_callback), "LobbyEndpoint");
            try
            {
                _client.Open();
                _client.OpenLobbySession(_playerSession.SessionKey);
            }
            catch
            {
                Abort();
                throw;
            }
        }

        public Lobby CreateLobby(LobbySettings settings) => GetClient().CreateLobby(settings);
        public Lobby JoinLobby(string lobbyCode) => GetClient().JoinLobby(lobbyCode);
        public Lobby GetLobby(string lobbyCode) => GetClient().GetLobby(lobbyCode);
        public void LeaveLobby() => GetClient().LeaveLobby();
        public void CloseLobby() => GetClient().CloseLobby();
        public void StartMatch() => GetClient().StartMatch();

        public void CloseConnection()
        {
            if (_client == null) return;
            try
            {
                if (_client.State == CommunicationState.Faulted) _client.Abort();
                else _client.Close();
            }
            catch (CommunicationException) { _client.Abort(); }
            catch (TimeoutException) { _client.Abort(); }
            finally { _client = null; }
        }

        public void Dispose() => CloseConnection();

        private LobbyServiceClient GetClient()
        {
            if (_client == null || _client.State != CommunicationState.Opened)
                throw new InvalidOperationException("Lobby connection is not open.");
            return _client;
        }

        private void Abort()
        {
            _client?.Abort();
            _client = null;
        }
    }
}
