using DominoAllFives.Contracts.Domain;

namespace DominoAllFives.Contracts.Services
{
    /// <summary>
    /// Represents the service responsible for handling ranking-related operations.
    /// </summary>
    public interface IRankingService
    {
        /// <summary>
        /// Retrieves the ranking information for the top players and the current player's rank.
        /// </summary>
        /// <param name="playerId">The ID of the player for whom 
        /// to retrieve ranking information.</param>
        /// <returns>The ranking result.</returns>
        Ranking GetTopRanking(int playerId);
    }
}
