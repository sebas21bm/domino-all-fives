using System;
using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.BusinessLogic.Controllers
{
    public sealed class LobbyController
    {
        private const int MinimumPlayers = 2;
        private const int MaximumPlayers = 4;
        private static readonly HashSet<int> validTargetPoints = new HashSet<int>
        {
            100,
            150,
            200,
            250
        };
        private readonly LobbyManager _lobbyManager;
        public LobbyController(LobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager ?? throw new ArgumentNullException(nameof(lobbyManager));
        }

        public Lobby CreateLobby(Player host, LobbySettings settings)
        {
            ValidatePlayer(host);
            ValidateSettings(settings);
            return _lobbyManager.CreateLobby(host, settings);
        }

        public Lobby JoinLobby(Player player, string code)
        {
            ValidatePlayer(player);
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException(null, nameof(code));
            }

            return _lobbyManager.JoinPlayer(player, code.Trim());
        }

        public Lobby LeaveLobby(int playerId) => _lobbyManager.LeavePlayer(ValidateId(playerId));
        public Lobby StartLobby(int playerId, Action<Lobby> initializeMatch = null)
            => _lobbyManager.StartLobby(ValidateId(playerId), initializeMatch);
        public Lobby CloseLobby(int playerId) => _lobbyManager.CloseLobby(ValidateId(playerId));
        public Lobby GetLobby(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException(null, nameof(code));
            }

            return _lobbyManager.GetLobby(code.Trim());
        }

        private static void ValidateSettings(LobbySettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (settings.MaxPlayers < MinimumPlayers || settings.MaxPlayers > MaximumPlayers)
            {
                throw new LobbyRuleException(LobbyIssue.InvalidPlayerLimit);
            }

            if (!validTargetPoints.Contains(settings.TargetPoints))
            {
                throw new LobbyRuleException(LobbyIssue.InvalidTargetPoints);
            }
        }

        private static void ValidatePlayer(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            ValidateId(player.PlayerId);
            if (string.IsNullOrWhiteSpace(player.Username))
            {
                throw new ArgumentException(null, nameof(player));
            }
        }

        private static int ValidateId(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            return playerId;
        }
    }
}
