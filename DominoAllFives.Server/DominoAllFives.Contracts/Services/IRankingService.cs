using System.Collections.Generic;
using DominoAllFives.Contracts.Domain;

namespace DominoAllFives.Contracts.Services
{
    /// <summary>
    /// Represents the service responsible for handling ranking-related operations.
    /// </summary>
    public interface IRankingService
    {
        /// <summary>
        /// Retrieves the ranking information for the top players by games won.
        /// </summary>
        /// <returns>The ranking result.</returns>
        List<RankingEntry> GetTopWinsRanking();

        /// <summary>
        /// Retrieves the player's position by games won, or zero when absent.
        /// </summary>
        int GetWinsRankingPositionById(int playerId);
    }
}
