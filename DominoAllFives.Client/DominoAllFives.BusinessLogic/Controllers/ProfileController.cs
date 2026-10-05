using DominoAllFives.Contracts.DTOs;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;

namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>
    /// Cordinates profile operations for everything related to the profile of a uses, 
    /// including: viewing, editing, and deleting the profile.
    /// </summary>
    public class ProfileController
    {
        private const string ApplicationFolderName = "DominoAllFives";

        private const string ProfilePicturesFolderName = "ProfilePictures";

        public ProfileDto GetProfile(int playerId)
        {
            try
            {
                using (DominoAllFivesEntities context = new DominoAllFivesEntities())
                {
                    IPlayerRepository playerRepository = new PlayerRepository(context);

                    Player player = playerRepository.GetById(playerId);

                    if (player == null ||
                       player.PlayerStats == null)
                    {
                        return CreateFailureResult(RetrieveInformationFailureReason.NotFound);
                    }

                    return new ProfileDto
                    {
                        Username = player.Username,
                        ProfilePictureData = GetProfilePictureData(player.ProfilePicture),
                        Wins = player.PlayerStats.GamesWon,
                        TotalPoints = player.PlayerStats.TotalPointsScored,
                        GamesPlayed = player.PlayerStats.GamesPlayed,
                        FailureReason = RetrieveInformationFailureReason.None
                    };

                }
            }
            catch (Exception ex) when (ex is SqlException ||
                                       ex is EntityException)
            {
                return CreateFailureResult(
                    RetrieveInformationFailureReason.ServiceUnavailable);
            }
        }

        /// <summary>
        /// Gets the profile picture data from the stored
        /// profile picture file.
        /// </summary>
        /// <param name="profilePictureFileName">
        /// The stored profile picture file name.
        /// </param>
        /// <returns>
        /// The profile picture data, or null when the player
        /// does not have an available profile picture.
        /// </returns>
        private byte[] GetProfilePictureData(
            string profilePictureFileName)
        {
            if (string.IsNullOrWhiteSpace(profilePictureFileName))
            {
                return null;
            }

            string localApplicationData =
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string profilePicturePath =
                Path.Combine(
                    localApplicationData,
                    ApplicationFolderName,
                    ProfilePicturesFolderName,
                    profilePictureFileName);

            if (!File.Exists(profilePicturePath))
            {
                return null;
            }

            try
            {
                return File.ReadAllBytes(
                    profilePicturePath);
            }
            catch (Exception ex) when (
                ex is UnauthorizedAccessException ||
                ex is PathTooLongException ||
                ex is DirectoryNotFoundException ||
                ex is IOException)
            {
                return null;
            }
        }

        private ProfileDto CreateFailureResult(RetrieveInformationFailureReason reason)
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
