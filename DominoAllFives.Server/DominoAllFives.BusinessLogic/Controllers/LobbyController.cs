using System;
using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using Player = DominoAllFives.Contracts.Domain.Player;
using StoredPlayer = DominoAllFives.DataAccess.Models.Player;

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
            _lobbyManager = lobbyManager
                ?? throw new ArgumentNullException(nameof(lobbyManager));
        }

        public Player GetAuthenticatedPlayer(
            string sessionKey,
            ProfilePictureStorage pictureStorage)
        {
            if (pictureStorage == null)
            {
                throw new ArgumentNullException(nameof(pictureStorage));
            }

            int playerId = SessionManager.GetPlayerId(sessionKey);
            using (DominoAllFivesEntities context = new DominoAllFivesEntities())
            {
                IPlayerRepository playerRepository = new PlayerRepository(context);
                StoredPlayer storedPlayer = playerRepository.GetById(playerId);
                if (storedPlayer == null
                    || storedPlayer.IsDeleted)
                {
                    SessionManager.RemoveSession(sessionKey);
                    throw new KeyNotFoundException();
                }

                return CreatePlayer(storedPlayer, pictureStorage);
            }
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
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException(null, nameof(code));
            }

            return _lobbyManager.JoinPlayer(player, code.Trim());
        }

        public Lobby LeaveLobby(int playerId)
        {
            ValidatePlayerId(playerId);
            return _lobbyManager.LeavePlayer(playerId);
        }

        public Lobby PrepareLobbyForMatch(int playerId)
        {
            ValidatePlayerId(playerId);
            return _lobbyManager.PrepareLobbyForMatch(playerId);
        }

        public Lobby ConfirmMatchStarted(int playerId)
        {
            ValidatePlayerId(playerId);
            return _lobbyManager.ConfirmMatchStarted(playerId);
        }

        public void RestoreLobbyAfterMatchStartFailure(int playerId)
        {
            ValidatePlayerId(playerId);
            _lobbyManager.RestoreLobbyAfterMatchStartFailure(playerId);
        }

        public Lobby CloseLobby(int playerId)
        {
            ValidatePlayerId(playerId);
            return _lobbyManager.CloseLobby(playerId);
        }

        public Lobby GetLobby(string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

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

            if (settings.MaxPlayers < MinimumPlayers
                || settings.MaxPlayers > MaximumPlayers)
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

            ValidatePlayerId(player.PlayerId);
            if (string.IsNullOrWhiteSpace(player.Username))
            {
                throw new ArgumentException(null, nameof(player));
            }
        }

        private static void ValidatePlayerId(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }
        }

        private static Player CreatePlayer(
            StoredPlayer storedPlayer,
            ProfilePictureStorage pictureStorage)
        {
            byte[] pictureData = pictureStorage.GetProfilePictureData(
                storedPlayer.ProfilePicture);

            return new Player
            {
                PlayerId = storedPlayer.IdPlayer,
                Username = storedPlayer.Username,
                Picture = ProfilePicture.FromStoredContent(pictureData),
                Presence = PlayerPresence.Online,
                IsGuest = storedPlayer.IsGuest
            };
        }
    }
}
