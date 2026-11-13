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
        private readonly object _syncRoot = new object ();
        private readonly Dictionary<string, Lobby> _lobbies = new Dictionary<string, Lobby>(
            StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, string> _playerLobbyCodes = new Dictionary<int, string>();
        private int _nextLobbyId = 1;
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
                EnsurePlayerIsAvailable(host.PlayerId);
                string code = GenerateLobbyCode();
                Lobby lobby = new Lobby
                {
                    LobbyId = _nextLobbyId++,
                    LobbyCode = code,
                    Host = CopyPlayer(host),
                    Participants = new List<Player>
                    {
                        CopyPlayer(host)
                    },
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

            if (string.IsNullOrWhiteSpace(lobbyCode))
            {
                throw new ArgumentException(null, nameof(lobbyCode));
            }

            lock (_syncRoot)
            {
                EnsurePlayerIsAvailable(player.PlayerId);
                Lobby lobby = GetLobbyInternal(lobbyCode);
                if (lobby.State != LobbyState.Waiting)
                {
                    throw new LobbyRuleException(LobbyIssue.Unavailable);
                }

                if (ContainsPlayer(lobby.KickedPlayers, player.PlayerId))
                {
                    throw new LobbyRuleException(LobbyIssue.PlayerExpelled);
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
                Lobby lobby = GetPlayerLobbyInternal(playerId);
                if (lobby.Host.PlayerId == playerId)
                {
                    return CloseLobbyInternal(lobby);
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
                return CreateSnapshot(GetLobbyInternal(lobbyCode));
            }
        }

        public Lobby StartLobby(int playerId, Action<Lobby> initializeMatch = null)
        {
            lock (_syncRoot)
            {
                Lobby lobby = GetPlayerLobbyInternal(playerId);
                EnsureHost(lobby, playerId);
                if (lobby.State != LobbyState.Waiting)
                {
                    throw new LobbyRuleException(LobbyIssue.Unavailable);
                }

                if (lobby.Participants.Count < MinimumPlayersToStart)
                {
                    throw new LobbyRuleException(LobbyIssue.InsufficientPlayers);
                }

                initializeMatch?.Invoke(CreateSnapshot(lobby));
                lobby.State = initializeMatch == null ? LobbyState.Starting : LobbyState.InProgress;
                return CreateSnapshot(lobby);
            }
        }

        public void ReleaseMatch(int lobbyId)
        {
            lock (_syncRoot)
            {
                Lobby lobby = null;
                foreach (Lobby candidate in _lobbies.Values)
                {
                    if (candidate.LobbyId == lobbyId)
                    {
                        lobby = candidate;
                        break;
                    }
                }

                if (lobby != null)
                {
                    CloseLobbyInternal(lobby);
                }
            }
        }

        public Lobby CloseLobby(int playerId)
        {
            lock (_syncRoot)
            {
                Lobby lobby = GetPlayerLobbyInternal(playerId);
                EnsureHost(lobby, playerId);
                return CloseLobbyInternal(lobby);
            }
        }

        private void EnsurePlayerIsAvailable(int playerId)
        {
            if (_playerLobbyCodes.ContainsKey(playerId))
            {
                throw new LobbyRuleException(LobbyIssue.PlayerAlreadyInLobby);
            }
        }

        private Lobby GetLobbyInternal(string code)
        {
            Lobby lobby;
            if (string.IsNullOrWhiteSpace(code) || !_lobbies.TryGetValue(code.Trim(), out lobby))
            {
                throw new LobbyRuleException(LobbyIssue.NotFound);
            }

            return lobby;
        }

        private Lobby GetPlayerLobbyInternal(int playerId)
        {
            string code;
            if (!_playerLobbyCodes.TryGetValue(playerId, out code))
            {
                throw new LobbyRuleException(LobbyIssue.PlayerNotInLobby);
            }

            return GetLobbyInternal(code);
        }

        private Lobby CloseLobbyInternal(Lobby lobby)
        {
            lobby.State = LobbyState.Closed;
            Lobby snapshot = CreateSnapshot(lobby);
            foreach (Player player in lobby.Participants)
            {
                _playerLobbyCodes.Remove(player.PlayerId);
            }

            _lobbies.Remove(lobby.LobbyCode);
            return snapshot;
        }

        private string GenerateLobbyCode()
        {
            string code;
            do
            {
                code = Guid.NewGuid().ToString("N").Substring(0,
                    LobbyCodeLength).ToUpperInvariant();
            }
            while (_lobbies.ContainsKey(code));
            return code;
        }

        private static void EnsureHost(Lobby lobby, int playerId)
        {
            if (lobby.Host.PlayerId != playerId)
            {
                throw new UnauthorizedAccessException();
            }
        }

        private static bool ContainsPlayer(IEnumerable<Player> players, int playerId)
        {
            foreach (Player player in players)
            {
                if (player.PlayerId == playerId)
                {
                    return true;
                }
            }

            return false;
        }

        private static Player FindPlayer(IEnumerable<Player> players, int playerId)
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
    }
}
