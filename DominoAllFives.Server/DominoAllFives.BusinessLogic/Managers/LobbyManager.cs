using System;
using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.BusinessLogic.Managers
{
    public sealed class LobbyManager
    {
        private const int MinimumPlayersToStart = 2;
        private const int LobbyCodeLength = 6;

        private static readonly LobbyManager instance = new LobbyManager();

        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, Lobby> _lobbies =
            new Dictionary<string, Lobby>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, string> _playerLobbyCodes = new Dictionary<int, string>();
        private int _nextLobbyId = 1;

        public static LobbyManager Instance
        {
            get
            {
                return instance;
            }
        }

        public Lobby CreateLobby(Player host, LobbySettings settings)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            lock (_syncRoot)
            {
                ValidatePlayerCanEnterLobby(host.PlayerId);
                string code = GenerateLobbyCode();
                Lobby lobby = new Lobby
                {
                    LobbyId = _nextLobbyId++,
                    LobbyCode = code,
                    Host = CopyPlayer(host),
                    Participants = new List<Player> { CopyPlayer(host) },
                    KickedPlayers = new List<Player>(),
                    Settings = CopySettings(settings),
                    State = LobbyState.Waiting
                };
                _lobbies.Add(code, lobby);
                _playerLobbyCodes.Add(host.PlayerId, code);
                return CreateSnapshot(lobby);
            }
        }

        public Lobby JoinPlayer(Player player, string lobbyCode)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (lobbyCode == null)
            {
                throw new ArgumentNullException(nameof(lobbyCode));
            }

            if (string.IsNullOrWhiteSpace(lobbyCode))
            {
                throw new ArgumentException(
                    "The lobby code cannot be empty.",
                    nameof(lobbyCode));
            }

            lock (_syncRoot)
            {
                ValidatePlayerCanEnterLobby(player.PlayerId);
                Lobby lobby = GetRegisteredLobby(lobbyCode);
                ValidateLobbyIsWaiting(lobby);
                foreach (Player kickedPlayer in lobby.KickedPlayers)
                {
                    if (kickedPlayer.PlayerId == player.PlayerId)
                    {
                        throw new LobbyRuleException(LobbyIssue.PlayerExpelled);
                    }
                }

                if (lobby.Participants.Count >= lobby.Settings.MaxPlayers)
                {
                    throw new LobbyRuleException(LobbyIssue.Full);
                }

                lobby.Participants.Add(CopyPlayer(player));
                _playerLobbyCodes.Add(player.PlayerId, lobby.LobbyCode);
                return CreateSnapshot(lobby);
            }
        }

        public Lobby LeavePlayer(int playerId)
        {
            lock (_syncRoot)
            {
                Lobby lobby = GetPlayerLobby(playerId);
                ValidateLobbyIsWaiting(lobby);
                if (lobby.Host.PlayerId == playerId)
                {
                    RemoveLobby(lobby);
                    return CreateSnapshot(lobby);
                }

                lobby.Participants.Remove(FindPlayer(lobby.Participants, playerId));
                _playerLobbyCodes.Remove(playerId);
                return CreateSnapshot(lobby);
            }
        }

        public Lobby GetLobby(string lobbyCode)
        {
            lock (_syncRoot)
            {
                return CreateSnapshot(GetRegisteredLobby(lobbyCode));
            }
        }

        public Lobby GetLobbyForInvitation(int playerId)
        {
            lock (_syncRoot)
            {
                Lobby lobby = GetPlayerLobby(playerId);
                ValidatePlayerIsHost(lobby, playerId);
                ValidateLobbyIsWaiting(lobby);
                return CreateSnapshot(lobby);
            }
        }

        public Lobby PrepareLobbyForMatch(int playerId)
        {
            lock (_syncRoot)
            {
                Lobby lobby = GetPlayerLobby(playerId);
                ValidatePlayerIsHost(lobby, playerId);
                ValidateLobbyIsWaiting(lobby);
                if (lobby.Participants.Count < MinimumPlayersToStart)
                {
                    throw new LobbyRuleException(LobbyIssue.InsufficientPlayers);
                }

                lobby.State = LobbyState.Starting;
                return CreateSnapshot(lobby);
            }
        }

        public Lobby ConfirmMatchStarted(int playerId)
        {
            lock (_syncRoot)
            {
                Lobby lobby = GetPlayerLobby(playerId);
                ValidatePlayerIsHost(lobby, playerId);
                if (lobby.State != LobbyState.Starting)
                {
                    throw new LobbyRuleException(LobbyIssue.Unavailable);
                }

                lobby.State = LobbyState.InProgress;
                return CreateSnapshot(lobby);
            }
        }

        public void RestoreLobbyAfterMatchStartFailure(int playerId)
        {
            lock (_syncRoot)
            {
                string lobbyCode;
                if (!_playerLobbyCodes.TryGetValue(playerId, out lobbyCode))
                {
                    return;
                }

                Lobby lobby;
                if (!_lobbies.TryGetValue(lobbyCode, out lobby))
                {
                    return;
                }

                if (lobby.Host.PlayerId == playerId && lobby.State == LobbyState.Starting)
                {
                    lobby.State = LobbyState.Waiting;
                }
            }
        }

        public void ReleaseMatch(int lobbyId)
        {
            if (lobbyId <= 0)
            {
                return;
            }

            lock (_syncRoot)
            {
                foreach (Lobby lobby in _lobbies.Values)
                {
                    if (lobby.LobbyId == lobbyId)
                    {
                        RemoveLobby(lobby);
                        break;
                    }
                }
            }
        }

        public Lobby CloseLobby(int playerId)
        {
            lock (_syncRoot)
            {
                Lobby lobby = GetPlayerLobby(playerId);
                ValidatePlayerIsHost(lobby, playerId);
                ValidateLobbyIsWaiting(lobby);
                RemoveLobby(lobby);
                return CreateSnapshot(lobby);
            }
        }

        public List<int> GetChatRecipientIds(int lobbyId, int senderId)
        {
            if (lobbyId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lobbyId));
            }

            if (senderId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(senderId));
            }

            lock (_syncRoot)
            {
                string lobbyCode;
                Lobby lobby;
                if (!_playerLobbyCodes.TryGetValue(senderId, out lobbyCode)
                    || !_lobbies.TryGetValue(lobbyCode, out lobby)
                    || lobby.LobbyId != lobbyId
                    || lobby.State != LobbyState.Waiting)
                {
                    throw new ChatRuleException(ChatIssue.PlayerNotInChat);
                }

                List<int> recipientIds = new List<int>(lobby.Participants.Count);
                foreach (Player participant in lobby.Participants)
                {
                    recipientIds.Add(participant.PlayerId);
                }

                return recipientIds;
            }
        }

        private static void ValidatePlayerIsHost(Lobby lobby, int playerId)
        {
            if (lobby.Host.PlayerId != playerId)
            {
                throw new UnauthorizedAccessException();
            }
        }

        private static void ValidateLobbyIsWaiting(Lobby lobby)
        {
            if (lobby.State != LobbyState.Waiting)
            {
                throw new LobbyRuleException(LobbyIssue.Unavailable);
            }
        }

        private static Player FindPlayer(List<Player> players, int playerId)
        {
            foreach (Player player in players)
            {
                if (player.PlayerId == playerId)
                {
                    return player;
                }
            }

            throw new LobbyRuleException(LobbyIssue.PlayerNotInLobby);
        }

        private static Lobby CreateSnapshot(Lobby lobby)
        {
            List<Player> participants = new List<Player>();
            foreach (Player player in lobby.Participants)
            {
                participants.Add(CopyPlayer(player));
            }

            List<Player> kicked = new List<Player>();
            foreach (Player player in lobby.KickedPlayers)
            {
                kicked.Add(CopyPlayer(player));
            }

            return new Lobby
            {
                LobbyId = lobby.LobbyId,
                LobbyCode = lobby.LobbyCode,
                Host = CopyPlayer(lobby.Host),
                Participants = participants,
                KickedPlayers = kicked,
                Settings = CopySettings(lobby.Settings),
                State = lobby.State
            };
        }

        private static Player CopyPlayer(Player player)
        {
            byte[] picture = player.Picture?.ImageData ?? new byte[0];
            return new Player
            {
                PlayerId = player.PlayerId,
                Username = player.Username,
                Picture = ProfilePicture.FromStoredContent(picture),
                Presence = player.Presence,
                IsGuest = player.IsGuest
            };
        }

        private static LobbySettings CopySettings(LobbySettings settings)
        {
            return new LobbySettings
            {
                MaxPlayers = settings.MaxPlayers,
                TargetPoints = settings.TargetPoints
            };
        }

        private void ValidatePlayerCanEnterLobby(int playerId)
        {
            if (_playerLobbyCodes.ContainsKey(playerId))
            {
                throw new LobbyRuleException(LobbyIssue.PlayerAlreadyInLobby);
            }
        }

        private Lobby GetRegisteredLobby(string code)
        {
            Lobby lobby;
            if (string.IsNullOrWhiteSpace(code) || !_lobbies.TryGetValue(code.Trim(), out lobby))
            {
                throw new LobbyRuleException(LobbyIssue.NotFound);
            }

            return lobby;
        }

        private Lobby GetPlayerLobby(int playerId)
        {
            string code;
            if (!_playerLobbyCodes.TryGetValue(playerId, out code))
            {
                throw new LobbyRuleException(LobbyIssue.PlayerNotInLobby);
            }

            return GetRegisteredLobby(code);
        }

        private void RemoveLobby(Lobby lobby)
        {
            lobby.State = LobbyState.Closed;
            foreach (Player player in lobby.Participants)
            {
                _playerLobbyCodes.Remove(player.PlayerId);
            }

            _lobbies.Remove(lobby.LobbyCode);
        }

        private string GenerateLobbyCode()
        {
            string code;
            do
            {
                code = Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, LobbyCodeLength)
                    .ToUpperInvariant();
            }
            while (_lobbies.ContainsKey(code));
            return code;
        }
    }
}
