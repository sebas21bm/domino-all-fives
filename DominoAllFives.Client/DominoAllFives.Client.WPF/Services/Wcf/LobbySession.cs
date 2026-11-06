using System;
using System.ServiceModel;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.Services.Wcf.Services;
using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services.Wcf
{
    /// <summary>Owns one participation across client pages, including callbacks received while navigating.</summary>
    public sealed class LobbySession : IDisposable
    {
        private readonly object _operations = new object();
        private readonly object _state = new object();
        private readonly Func<ILobbyClient> _createClient;
        private ILobbyClient _client;
        private Action<Lobby> _changedHandler;
        private Action _closedHandler;
        private Action<int> _startedHandler;
        private Lobby _currentLobby;
        private bool _created;
        private bool _closed;
        private bool _disposed;
        private int _matchId;

        public LobbySession() : this(() => new WcfLobbyService()) { }
        public LobbySession(Func<ILobbyClient> createClient)
        {
            _createClient = createClient ?? throw new ArgumentNullException(nameof(createClient));
        }

        public Lobby CurrentLobby { get { lock (_state) return _currentLobby; } }
        public bool WasCreatedByThisClient { get { lock (_state) return _created; } }
        public bool WasClosed { get { lock (_state) return _closed; } }
        public int ProvisionalMatchId { get { lock (_state) return _matchId; } }
        public ILobbyClient Client { get { lock (_state) return _client; } }
        public bool IsActive { get { lock (_state) return _client != null && _currentLobby != null; } }

        public Lobby Create(LobbySettings settings)
        {
            return Participate(client => client.CreateLobby(settings), true);
        }

        public Lobby Join(string code)
        {
            return Participate(client => client.JoinLobby(code), false);
        }

        private Lobby Participate(Func<ILobbyClient, Lobby> operation, bool created)
        {
            lock (_operations)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(LobbySession));
                if (Client != null) throw new InvalidOperationException("A lobby connection is already active.");
                try
                {
                    ILobbyClient client = _createClient();
                    if (client == null) throw new InvalidOperationException("The lobby client factory returned null.");
                    lock (_state)
                    {
                        _client = client;
                        _closed = false;
                        _matchId = 0;
                        _created = created;
                    }
                    _changedHandler = lobby =>
                    {
                        lock (_state)
                        {
                            if (ReferenceEquals(_client, client) && !_closed && lobby != null) _currentLobby = lobby;
                        }
                    };
                    _closedHandler = () => { lock (_state) { if (ReferenceEquals(_client, client)) _closed = true; } };
                    _startedHandler = id => { lock (_state) { if (ReferenceEquals(_client, client)) _matchId = id; } };
                    client.LobbyChanged += _changedHandler;
                    client.LobbyClosed += _closedHandler;
                    client.MatchStarted += _startedHandler;
                    client.Open();
                    Lobby response = operation(client);
                    lock (_state)
                    {
                        if (_currentLobby == null) _currentLobby = response;
                        return _currentLobby;
                    }
                }
                catch
                {
                    Reset();
                    throw;
                }
            }
        }

        public void Update(Lobby lobby)
        {
            lock (_state) { if (_client != null && lobby != null && !_closed) _currentLobby = lobby; }
        }

        public void StartMatch()
        {
            lock (_operations)
            {
                ILobbyClient client = Client;
                if (client == null) throw new InvalidOperationException("No active lobby connection.");
                client.StartMatch();
            }
        }

        public void Leave()
        {
            lock (_operations)
            {
                ILobbyClient client = Client;
                try
                {
                    if (client != null && CurrentLobby != null && !WasClosed &&
                        client.State == CommunicationState.Opened) client.LeaveLobby();
                }
                finally { Reset(); }
            }
        }

        public void Reset()
        {
            lock (_operations)
            {
                ILobbyClient client;
                lock (_state)
                {
                    client = _client;
                    _client = null;
                    _currentLobby = null;
                    _created = false;
                    _closed = false;
                    _matchId = 0;
                }
                if (client == null) return;
                client.LobbyChanged -= _changedHandler;
                client.LobbyClosed -= _closedHandler;
                client.MatchStarted -= _startedHandler;
                _changedHandler = null;
                _closedHandler = null;
                _startedHandler = null;
                client.Dispose();
            }
        }

        public void Dispose()
        {
            lock (_operations)
            {
                try { Leave(); }
                finally { _disposed = true; }
            }
        }
    }
}
