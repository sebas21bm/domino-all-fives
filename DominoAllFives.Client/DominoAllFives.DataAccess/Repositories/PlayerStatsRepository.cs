using System.Collections.Generic;
using System.Linq;

using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;

namespace DominoAllFives.DataAccess.Repositories
{
    /// <summary>
    /// Provides Entity Framework persistence operations
    /// for player statistics.
    /// </summary>
    public class PlayerStatsRepository : IPlayerStatsRepository
    {
        private readonly DominoAllFivesEntities _context;

        /// <summary>
        /// Initializes a new repository with the specified
        /// database context.
        /// </summary>
        /// <param name="databaseContext">
        /// The database context used by the repository.
        /// </param>
        public PlayerStatsRepository(
            DominoAllFivesEntities databaseContext)
        {
            _context = databaseContext;
        }

        /// <inheritdoc />
        public PlayerStats GetByPlayerId(int playerId)
        {
            return _context.PlayerStats.FirstOrDefault(
                stats => stats.IdPlayer == playerId);
        }

        /// <inheritdoc />
        public IEnumerable<PlayerStats> GetTopByGamesWon(int limit)
        {
            return _context.PlayerStats
                .Where(stats =>
                    stats.Player.IsGuest == false &&
                    stats.GamesWon > 0)
                .OrderByDescending(stats => stats.GamesWon)
                .ThenByDescending(stats => stats.LastVictoryDate)
                .Take(limit)
                .ToList();
        }

        /// <inheritdoc />
        public void Add(PlayerStats playerStatsToAdd)
        {
            _context.PlayerStats.Add(playerStatsToAdd);
        }

        /// <inheritdoc />
        public int GetRankByPlayerId(int playerId)
        {
            PlayerStats playerStats =
                GetByPlayerId(playerId);

            if (playerStats == null ||
                playerStats.GamesWon <= 0)
            {
                return 0;
            }

            int playersAhead =
                _context.PlayerStats.Count(
                    stats =>
                        stats.Player.IsGuest == false &&
                        stats.GamesWon > 0 &&
                        (stats.GamesWon > playerStats.GamesWon ||
                        (stats.GamesWon == playerStats.GamesWon &&
                         stats.LastVictoryDate >
                         playerStats.LastVictoryDate)));

            return playersAhead + 1;
        }
    }
}