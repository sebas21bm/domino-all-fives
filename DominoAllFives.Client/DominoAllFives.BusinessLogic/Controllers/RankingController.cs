using System.Collections.Generic;
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;

namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>
    /// Coordinates operations related to player rankings.
    /// </summary>
    public class RankingController
    {
        private readonly IPlayerStatsRepository _playerStatsRepository;

        public RankingController(IPlayerStatsRepository playerStatsRepository)
        {
            _playerStatsRepository = playerStatsRepository;
        }

        /// <summary>
        /// Gets the top ten players and the ranking position of the current player.
        /// </summary>
        /// <param name="playerId">The identifier of the current player.</param>
        /// <returns>The ranking information.</returns>
        public RankingResultDto GetRanking(int playerId)
        {
            IEnumerable<PlayerStats> topPlayerStats =
                _playerStatsRepository.GetTopByGamesWon(10);

            RankingResultDto rankingResult = new RankingResultDto
            {
                CurrentPlayerRank =
                    _playerStatsRepository.GetRankByPlayerId(playerId)
            };

            int rank = 1;
            foreach (PlayerStats playerStats in topPlayerStats)
            {
                RankingEntryDto rankingEntry = new RankingEntryDto
                {
                    Rank = rank,
                    Username = playerStats.Player.Username,
                    GamesWon = playerStats.GamesWon,
                    ProfilePicture = playerStats.Player.ProfilePicture
                };

                rankingResult.TopPlayers.Add(rankingEntry);

                rank++;
            }

            return rankingResult;
        }
    }
}
