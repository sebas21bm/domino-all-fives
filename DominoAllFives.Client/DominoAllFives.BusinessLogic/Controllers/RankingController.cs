using System;
using System.Collections.Generic;
using System.IO;
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;

namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>
    /// Coordinates operations related to player rankings.
    /// </summary>
    public class RankingController
    {
        private const int RankingLimit = 10;

        private const string ApplicationFolderName = "DominoAllFives";

        private const string ProfilePicturesFolderName = "ProfilePictures";

        /// <summary>
        /// Gets the top players and the ranking position
        /// of the current player.
        /// </summary>
        /// <param name="playerId">
        /// The identifier of the current player.
        /// </param>
        /// <returns>
        /// The ranking information.
        /// </returns>
        public RankingResultDto GetRanking(int playerId)
        {
            using (DominoAllFivesEntities context =
                new DominoAllFivesEntities())
            {
                IPlayerStatsRepository playerStatsRepository =
                    new PlayerStatsRepository(context);

                IEnumerable<PlayerStats> topPlayerStats =
                    playerStatsRepository.GetTopByGamesWon(RankingLimit);

                RankingResultDto rankingResult =
                    new RankingResultDto
                    {
                        CurrentPlayerRank =
                            playerStatsRepository.GetRankByPlayerId(playerId)
                    };

                int rank = 1;
                foreach (PlayerStats playerStats in topPlayerStats)
                {
                    RankingEntryDto rankingEntry =
                        new RankingEntryDto
                        {
                            Rank = rank,
                            Username = playerStats.Player.Username,
                            GamesWon = playerStats.GamesWon,
                            ProfilePictureData =
                                GetProfilePictureData(playerStats.Player.ProfilePicture)
                        };

                    rankingResult.TopPlayers.Add(rankingEntry);
                    rank++;
                }

                return rankingResult;
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
    }
}