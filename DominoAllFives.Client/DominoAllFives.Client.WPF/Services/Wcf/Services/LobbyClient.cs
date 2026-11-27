using System;
using System.ServiceModel;
using System.Threading;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services.Wcf.Callbacks;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using LobbyService;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.Services.Wcf.Services
{
    public sealed class LobbyClient : ILobbyClient
    {
        private static readonly TimeSpan lobbyCallbackTimeout = TimeSpan.FromSeconds(10);
        private readonly PlayerSession _playerSession;
        private readonly Func<LobbyCallbackHandler, LobbyServiceClient> _clientFactory;
        private readonly ManualResetEventSlim _lobbyChangedSignal =
            new ManualResetEventSlim(false);
        private LobbyServiceClient _client;
        private LobbyCallbackHandler _callback;
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

        public LobbyClient(
            PlayerSession playerSession) : this(
            playerSession,
            CreateClient)
        {
        }

        internal LobbyClient(
            PlayerSession playerSession,
            Func<LobbyCallbackHandler, LobbyServiceClient> clientFactory)
        {
            _playerSession = playerSession ?? throw new ArgumentNullException(
                nameof(playerSession));
            _clientFactory = clientFactory ?? throw new ArgumentNullException(
                nameof(clientFactory));
        }

        public event Action<Lobby> LobbyChanged;
        public event Action LobbyClosed;
        public event Action<int> MatchStarted;
        public void CreateLobby(LobbySettings settings)
        {
            OpenConnection();
            _lobbyChangedSignal.Reset();
            bool completed = false;
            try
            {
                _client.CreateLobby(settings);
                WaitForLobbyChanged();
                WasCreatedByThisClient = true;
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

        public void JoinLobby(string lobbyCode)
        {
            OpenConnection();
            _lobbyChangedSignal.Reset();
            bool completed = false;
            try
            {
                _client.JoinLobby(lobbyCode);
                WaitForLobbyChanged();
                WasCreatedByThisClient = false;
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

        public void SendLobbyInvitation(string recipientEmail)
        {
            EnsureActive();
            _client.SendLobbyInvitation(recipientEmail);
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
                _lobbyChangedSignal.Dispose();
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

            ConfigureCallbacks(callback);
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

        private static LobbyServiceClient CreateClient(LobbyCallbackHandler callback)
        {
            return new LobbyServiceClient(
                new InstanceContext(callback),
                "LobbyEndpoint");
        }

        private void ConfigureCallbacks(LobbyCallbackHandler callback)
        {
            callback.LobbyChanged += OnLobbyChanged;
            callback.LobbyClosed += OnLobbyClosed;
            callback.MatchStarted += OnMatchStarted;
        }

        private void UnsubscribeCallbacks()
        {
            if (_callback == null)
            {
                return;
            }

            _callback.LobbyChanged -= OnLobbyChanged;
            _callback.LobbyClosed -= OnLobbyClosed;
            _callback.MatchStarted -= OnMatchStarted;
            _callback = null;
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
            catch (CommunicationException ex)
            {
                LoggerFactoryProvider.CreateLogger<LobbyClient>().LogWarning(
                    ex, "The WCF channel could not be closed cleanly.");
                _client.Abort();
            }
            catch (TimeoutException ex)
            {
                LoggerFactoryProvider.CreateLogger<LobbyClient>().LogWarning(
                    ex, "The WCF channel could not be closed cleanly.");
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
                throw new ObjectDisposedException(nameof(LobbyClient));
            }
        }

        private void WaitForLobbyChanged()
        {
            if (!_lobbyChangedSignal.Wait(lobbyCallbackTimeout))
            {
                throw new TimeoutException("The lobby callback was not received in time.");
            }
        }

        private void OnLobbyChanged(LobbyCallbackHandler callback, Lobby lobby)
        {
            if (!ReferenceEquals(_callback, callback))
            {
                return;
            }

            CurrentLobby = lobby;
            _lobbyChangedSignal.Set();
            LobbyChanged?.Invoke(lobby);
        }

        private void OnLobbyClosed(LobbyCallbackHandler callback)
        {
            if (!ReferenceEquals(_callback, callback))
            {
                return;
            }

            WasClosed = true;
            LobbyClosed?.Invoke();
        }

        private void OnMatchStarted(LobbyCallbackHandler callback, int matchId)
        {
            if (!ReferenceEquals(_callback, callback))
            {
                return;
            }

            ProvisionalMatchId = matchId;
            MatchStarted?.Invoke(matchId);
        }
    }
}
