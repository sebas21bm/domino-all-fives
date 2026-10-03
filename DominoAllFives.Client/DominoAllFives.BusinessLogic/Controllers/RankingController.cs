using System.Collections.Generic;

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
                    playerStatsRepository.GetTopByGamesWon(
                        RankingLimit);

                RankingResultDto rankingResult =
                    new RankingResultDto
                    {
                        CurrentPlayerRank =
                            playerStatsRepository
                                .GetRankByPlayerId(playerId)
                    };

                int rank = 1;

                foreach (PlayerStats playerStats
                    in topPlayerStats)
                {
                    RankingEntryDto rankingEntry =
                        new RankingEntryDto
                        {
                            Rank = rank,
                            Username =
                                playerStats.Player.Username,
                            GamesWon =
                                playerStats.GamesWon,
                            ProfilePicture =
                                playerStats.Player.ProfilePicture
                        };

                    rankingResult.TopPlayers.Add(
                        rankingEntry);

                    rank++;
                }

                return rankingResult;
            }
        }
    }
}