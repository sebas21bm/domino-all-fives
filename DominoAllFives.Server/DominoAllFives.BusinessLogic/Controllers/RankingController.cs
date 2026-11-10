using System;
using System.Collections.Generic;

using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.Contracts.Domain;
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

        private readonly ProfilePictureStorage _profilePictureStorage;

        public RankingController(
            ProfilePictureStorage profilePictureStorage)
        {
            _profilePictureStorage = profilePictureStorage
                ?? throw new ArgumentNullException(
                    nameof(profilePictureStorage));
        }

        /// <summary>
        /// Gets the top players ordered by games won.
        /// </summary>
        /// <returns>
        /// The ranking information.
        /// </returns>
        public List<RankingEntry> GetTopWinsRanking()
        {
            using (DominoAllFivesEntities context =
                new DominoAllFivesEntities())
            {
                IPlayerStatsRepository playerStatsRepository =
                    new PlayerStatsRepository(context);

                IEnumerable<PlayerStats> topPlayerStats =
                    playerStatsRepository.GetTopByGamesWon(
                        RankingLimit);

                List<RankingEntry> entries = new List<RankingEntry>();

                int rank = 1;

                foreach (PlayerStats playerStats in topPlayerStats)
                {
                    RankingEntry rankingEntry = RankingEntry.FromRankedPlayer(
                        rank, playerStats.Player.Username, playerStats.GamesWon,
                        ProfilePicture.FromStoredContent(
                            _profilePictureStorage.GetProfilePictureData(
                                playerStats.Player.ProfilePicture)));
                    entries.Add(rankingEntry);

                    rank++;
                }

                return entries;
            }
        }

        /// <summary>
        /// Gets the player's position by games won, or zero when absent.
        /// </summary>
        public int GetWinsRankingPositionById(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            
            using (DominoAllFivesEntities context = new DominoAllFivesEntities())
            {
                IPlayerStatsRepository playerStatsRepository =
                    new PlayerStatsRepository(context);
                return playerStatsRepository.GetRankByPlayerId(playerId);
            }
        }
    }
}
