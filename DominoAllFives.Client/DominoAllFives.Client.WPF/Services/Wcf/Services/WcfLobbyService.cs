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
        private readonly Func<LobbyCallbackHandler, LobbyServiceClient> _clientFactory;
        private LobbyServiceClient _client;
        private LobbyCallbackHandler _callback;
        private Action<Lobby> _lobbyChangedHandler;
        private Action _lobbyClosedHandler;
        private Action<int> _matchStartedHandler;
        private bool _disposed;
        public CommunicationState State
        {
            get
            {
                return _client?.State ?? CommunicationState.Closed;
            }
        }

        public Lobby CurrentLobby { get; private set; }

        public bool IsActive
        {
            get
            {
                return _client != null && CurrentLobby != null && !WasClosed;
            }
        }

        public bool WasCreatedByThisClient { get; private set; }
        public bool WasClosed { get; private set; }
        public int ProvisionalMatchId { get; private set; }

        public WcfLobbyService(
            PlayerSession playerSession) : this(
            playerSession,
            callback => new LobbyServiceClient(
            new InstanceContext(
            callback),
            "LobbyEndpoint"))
        {
        }

        internal WcfLobbyService(
            PlayerSession playerSession,
            Func<LobbyCallbackHandler, LobbyServiceClient> clientFactory)
        {
            _playerSession = playerSession ?? throw new ArgumentNullException(
                nameof(
                playerSession));
            _clientFactory = clientFactory ?? throw new ArgumentNullException(
                nameof(
                clientFactory));
        }

        public event Action<Lobby> LobbyChanged;
        public event Action LobbyClosed;
        public event Action<int> MatchStarted;
        public Lobby CreateLobby(LobbySettings settings)
        {
            OpenConnection();
            bool completed = false;
            try
            {
                Lobby response = _client.CreateLobby(settings);
                WasCreatedByThisClient = true;
                if (CurrentLobby == null)
                {
                    CurrentLobby = response;
                }

                completed = true;
                return CurrentLobby;
            }
            finally
            {
                if (!completed)
                {
                    Reset();
                }
            }
        }

        public Lobby JoinLobby(string lobbyCode)
        {
            OpenConnection();
            bool completed = false;
            try
            {
                Lobby response = _client.JoinLobby(lobbyCode);
                WasCreatedByThisClient = false;
                if (CurrentLobby == null)
                {
                    CurrentLobby = response;
                }

                completed = true;
                return CurrentLobby;
            }
            finally
            {
                if (!completed)
                {
                    Reset();
                }
            }
        }

        public Lobby GetLobby(string lobbyCode)
        {
            EnsureActive();
            return _client.GetLobby(lobbyCode);
        }

        public void LeaveLobby()
        {
            EnsureNotDisposed();
            try
            {
                if (IsActive)
                {
                    _client.LeaveLobby();
                }
            }
            finally
            {
                Reset();
            }
        }

        public void StartMatch()
        {
            EnsureActive();
            _client.StartMatch();
        }

        public void CloseLobby()
        {
            EnsureActive();
            try
            {
                _client.CloseLobby();
            }
            finally
            {
                Reset();
            }
        }

        public void Reset()
        {
            UnsubscribeCallbacks();
            CloseConnection();
            CurrentLobby = null;
            WasCreatedByThisClient = false;
            WasClosed = false;
            ProvisionalMatchId = 0;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                if (IsActive)
                {
                    LeaveLobby();
                }
                else
                {
                    Reset();
                }
            }
            finally
            {
                _disposed = true;
            }
        }

        private void OpenConnection()
        {
            EnsureNotDisposed();
            if (!_playerSession.HasServerSession)
            {
                throw new InvalidOperationException("A server session is required.");
            }

            if (_client != null)
            {
                throw new InvalidOperationException("A lobby connection is already open.");
            }

            LobbyCallbackHandler callback = new LobbyCallbackHandler();
            LobbyServiceClient client = _clientFactory(callback);
            if (client == null)
            {
                throw new InvalidOperationException("Lobby client factory returned null.");
            }

            ConfigureCallbacks(callback, client);
            _callback = callback;
            _client = client;
            bool completed = false;
            try
            {
                _client.Open();
                _client.OpenLobbySession(_playerSession.SessionKey);
                completed = true;
            }
            finally
            {
                if (!completed)
                {
                    Reset();
                }
            }
        }

        private void ConfigureCallbacks(LobbyCallbackHandler callback, LobbyServiceClient client)
        {
            _lobbyChangedHandler = lobby => OnLobbyChanged(client, lobby);
            _lobbyClosedHandler = () => OnLobbyClosed(client);
            _matchStartedHandler = matchId => OnMatchStarted(client, matchId);
            callback.LobbyChanged += _lobbyChangedHandler;
            callback.LobbyClosed += _lobbyClosedHandler;
            callback.MatchStarted += _matchStartedHandler;
        }

        private void UnsubscribeCallbacks()
        {
            if (_callback == null)
            {
                return;
            }

            _callback.LobbyChanged -= _lobbyChangedHandler;
            _callback.LobbyClosed -= _lobbyClosedHandler;
            _callback.MatchStarted -= _matchStartedHandler;
            _callback = null;
            _lobbyChangedHandler = null;
            _lobbyClosedHandler = null;
            _matchStartedHandler = null;
        }

        private void CloseConnection()
        {
            if (_client == null)
            {
                return;
            }

            try
            {
                if (_client.State == CommunicationState.Faulted)
                {
                    _client.Abort();
                }
                else
                {
                    _client.Close();
                }
            }
            catch (CommunicationException)
            {
                _client.Abort();
            }
            catch (TimeoutException)
            {
                _client.Abort();
            }
            finally
            {
                _client = null;
            }
        }

        private void EnsureActive()
        {
            EnsureNotDisposed();
            if (!IsActive)
            {
                throw new InvalidOperationException("No active lobby connection.");
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(WcfLobbyService));
            }
        }

        private void OnLobbyChanged(LobbyServiceClient client, Lobby lobby)
        {
            if (!ReferenceEquals(_client, client))
            {
                return;
            }

            CurrentLobby = lobby;
            LobbyChanged?.Invoke(lobby);
        }

        private void OnLobbyClosed(LobbyServiceClient client)
        {
            if (!ReferenceEquals(_client, client))
            {
                return;
            }

            WasClosed = true;
            LobbyClosed?.Invoke();
        }

        private void OnMatchStarted(LobbyServiceClient client, int matchId)
        {
            if (!ReferenceEquals(_client, client))
            {
                return;
            }

            ProvisionalMatchId = matchId;
            MatchStarted?.Invoke(matchId);
        }
    }
}
