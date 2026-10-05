using System;
using System.Data.Entity.Core;
using System.Data.SqlClient;

using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>
    /// Coordinates operations related to player profile information.
    /// </summary>
    public class ProfileController
    {
        private readonly ProfilePictureStorage _profilePictureStorage;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            ProfilePictureStorage profilePictureStorage,
            ILogger<ProfileController> logger)
        {
            _profilePictureStorage = profilePictureStorage
                ?? throw new ArgumentNullException(
                    nameof(profilePictureStorage));

            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves the profile information of the specified player.
        /// </summary>
        /// <param name="playerId">
        /// The identifier of the player whose profile will be retrieved.
        /// </param>
        /// <returns>
        /// The profile information and the result of the retrieval operation.
        /// </returns>
        public ProfileDto GetProfile(int playerId)
        {
            if (playerId <= 0)
            {
                return CreateFailureResult(
                    RetrieveInformationFailureReason.NotFound);
            }

            try
            {
                using (DominoAllFivesEntities context = new DominoAllFivesEntities())
                {
                    IPlayerRepository playerRepository = new PlayerRepository(context);

                    Player player = playerRepository.GetById(playerId);

                    if (player == null ||
                        player.PlayerStats == null)
                    {
                        return CreateFailureResult(
                            RetrieveInformationFailureReason.NotFound);
                    }

                    return new ProfileDto
                    {
                        Username = player.Username,
                        ProfilePictureData = _profilePictureStorage.GetProfilePictureData(
                                                                        player.ProfilePicture),
                        Wins = player.PlayerStats.GamesWon,
                        TotalPoints = player.PlayerStats.TotalPointsScored,
                        GamesPlayed = player.PlayerStats.GamesPlayed,
                        FailureReason = RetrieveInformationFailureReason.None
                    };
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to retrieve profile for player {PlayerId}.",
                    playerId);

                return CreateFailureResult(
                    RetrieveInformationFailureReason.ServiceUnavailable);
            }
            catch (EntityException ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to retrieve profile for player {PlayerId}.",
                    playerId);

                return CreateFailureResult(
                    RetrieveInformationFailureReason.ServiceUnavailable);
            }
        }

        private ProfileDto CreateFailureResult(
            RetrieveInformationFailureReason reason)
        {
            return new ProfileDto
            {
                Username = string.Empty,
                ProfilePictureData = Array.Empty<byte>(),
                Wins = 0,
                TotalPoints = 0,
                GamesPlayed = 0,
                FailureReason = reason
            };
        }
    }
}
