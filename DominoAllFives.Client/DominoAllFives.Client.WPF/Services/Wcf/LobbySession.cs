using System;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using LobbyService;

namespace DominoAllFives.Client.WPF.Services.Wcf
{
    public sealed class LobbySession : IDisposable
    {
        private readonly Func<ILobbyClient> _clientFactory;
        private bool _disposed;
        private Action<Lobby> _lobbyChangedHandler;
        private Action _lobbyClosedHandler;
        private Action<int> _matchStartedHandler;

        public ILobbyClient Client { get; private set; }
        public Lobby CurrentLobby { get; private set; }
        public bool IsActive => Client != null && CurrentLobby != null && !WasClosed;
        public bool WasCreatedByThisClient { get; private set; }
        public bool WasClosed { get; private set; }
        public int ProvisionalMatchId { get; private set; }

        public LobbySession(Func<ILobbyClient> clientFactory)
        {
            _clientFactory = clientFactory
                ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        public Lobby Create(LobbySettings settings)
        {
            ILobbyClient client = OpenClient();
            try
            {
                Lobby response = client.CreateLobby(settings);
                WasCreatedByThisClient = true;
                if (CurrentLobby == null) CurrentLobby = response;
                return CurrentLobby;
            }
            catch { Reset(); throw; }
        }

        public Lobby Join(string lobbyCode)
        {
            ILobbyClient client = OpenClient();
            try
            {
                Lobby response = client.JoinLobby(lobbyCode);
                WasCreatedByThisClient = false;
                if (CurrentLobby == null) CurrentLobby = response;
                return CurrentLobby;
            }
            catch { Reset(); throw; }
        }

        public void StartMatch()
        {
            EnsureActive();
            Client.StartMatch();
        }

        public void Leave()
        {
            EnsureNotDisposed();
            try { if (Client != null && CurrentLobby != null) Client.LeaveLobby(); }
            finally { Reset(); }
        }

        public void Reset()
        {
            if (Client != null)
            {
                Client.LobbyChanged -= _lobbyChangedHandler;
                Client.LobbyClosed -= _lobbyClosedHandler;
                Client.MatchStarted -= _matchStartedHandler;
                Client.Dispose();
            }
            Client = null;
            CurrentLobby = null;
            WasCreatedByThisClient = false;
            WasClosed = false;
            ProvisionalMatchId = 0;
            _lobbyChangedHandler = null;
            _lobbyClosedHandler = null;
            _matchStartedHandler = null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            try { if (IsActive) Leave(); else Reset(); }
            finally { _disposed = true; }
        }

        private ILobbyClient OpenClient()
        {
            EnsureNotDisposed();
            if (Client != null)
                throw new InvalidOperationException("A lobby session is already active.");
            ILobbyClient client = _clientFactory();
            Client = client
                ?? throw new InvalidOperationException("Lobby client factory returned null.");
            _lobbyChangedHandler = lobby =>
            {
                if (ReferenceEquals(Client, client)) CurrentLobby = lobby;
            };
            _lobbyClosedHandler = () =>
            {
                if (ReferenceEquals(Client, client)) WasClosed = true;
            };
            _matchStartedHandler = matchId =>
            {
                if (ReferenceEquals(Client, client)) ProvisionalMatchId = matchId;
            };
            Client.LobbyChanged += _lobbyChangedHandler;
            Client.LobbyClosed += _lobbyClosedHandler;
            Client.MatchStarted += _matchStartedHandler;
            try { Client.Open(); return Client; }
            catch { Reset(); throw; }
        }

        private void EnsureActive()
        {
            EnsureNotDisposed();
            if (!IsActive) throw new InvalidOperationException("No active lobby session.");
        }

        private void EnsureNotDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(LobbySession));
        }
    }
}
