using System.Collections.Generic;

using DominoAllFives.DataAccess.Models;

namespace DominoAllFives.DataAccess.Interfaces
{
    /// <summary>
    /// Defines persistence operations for player statistics.
    /// </summary>
    public interface IPlayerStatsRepository
    {
        /// <summary>
        /// Retrieves the statistics associated with a player.
        /// </summary>
        /// <param name="playerId">The identifier of the player.</param>
        /// <returns>The player statistics, or null when no statistics are found.</returns>
        PlayerStats GetByPlayerId(int playerId);

        /// <summary>
        /// Retrieves the highest-ranked player statistics ordered by victories.
        /// </summary>
        /// <param name="limit">The maximum number of records to retrieve.</param>
        /// <returns>The ordered collection of player statistics.</returns>
        IEnumerable<PlayerStats> GetTopByGamesWon(int limit);

        /// <summary>
        /// Adds statistics for a player to the current persistence context.
        /// </summary>
        /// <param name="playerStatsToAdd">The player statistics to add.</param>
        void Add(PlayerStats playerStatsToAdd);

        /// <summary>
        /// Gets the ranking position of a player.
        /// </summary>
        /// <param name="playerId">The player identifier.</param>
        /// <returns>The player's position in the ranking.</returns>
        int GetRankByPlayerId(int playerId);
    }
}