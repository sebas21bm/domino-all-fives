using System;
using System.ServiceModel;
using DominoAllFives.Client.WPF.Services.Wcf.Callbacks;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.Services.Wcf.Proxies;
using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Services
{
    public sealed class WcfLobbyService : ILobbyClient
    {
        private readonly LobbyCallbackHandler _callback;
        private readonly WcfDuplexConnectionManager<LobbyServiceClient> _connection;

        public event Action<Lobby> LobbyChanged;
        public event Action LobbyClosed;
        public event Action<int> MatchStarted;

        public CommunicationState State => _connection.State;

        public WcfLobbyService()
        {
            _callback = new LobbyCallbackHandler();
            _callback.LobbyChanged += lobby => LobbyChanged?.Invoke(lobby);
            _callback.LobbyClosed += () => LobbyClosed?.Invoke();
            _callback.MatchStarted += matchId => MatchStarted?.Invoke(matchId);

            InstanceContext context = new InstanceContext(_callback);
            _connection = new WcfDuplexConnectionManager<LobbyServiceClient>(
                () => new LobbyServiceClient(context, "LobbyEndpoint"));
        }

        public void Open() => _connection.Open();

        public Lobby CreateLobby(LobbySettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            return _connection.Invoke(proxy => proxy.CreateLobby(settings));
        }

        public Lobby JoinLobby(string lobbyCode)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode))
                throw new ArgumentException("Lobby code is required.", nameof(lobbyCode));
            return _connection.Invoke(proxy => proxy.JoinLobby(lobbyCode.Trim()));
        }

        public Lobby GetLobby(string lobbyCode)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode))
                throw new ArgumentException("Lobby code is required.", nameof(lobbyCode));
            return _connection.Invoke(proxy => proxy.GetLobby(lobbyCode.Trim()));
        }

        public void LeaveLobby() => _connection.Invoke(proxy => proxy.LeaveLobby());
        public void CloseLobby() => _connection.Invoke(proxy => proxy.CloseLobby());
        public void StartMatch() => _connection.Invoke(proxy => proxy.StartMatch());
        public void CloseConnection() => _connection.Close();
        public void Dispose() => _connection.Dispose();
    }
}
