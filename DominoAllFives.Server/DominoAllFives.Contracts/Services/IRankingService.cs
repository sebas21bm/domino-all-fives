using System.Collections.Generic;
using System.ServiceModel;
using DominoAllFives.Contracts.Domain;

namespace DominoAllFives.Contracts.Services
{
    /// <summary>
    /// Exposes the leaderboard and individual player positions over WCF.
    /// </summary>
    [ServiceContract(Name = "IRankingRemoteService", Namespace = "urn:domino-all-fives:services")]
    public interface IRankingService
    {
        /// <summary>
        /// Retrieves the leading players ordered by games won.
        /// </summary>
        /// <returns>The ordered leaderboard entries.</returns>
        /// <exception cref="FaultException">
        /// The server cannot read the ranking because persistence failed.
        /// </exception>
        [OperationContract]
        List<RankingEntry> GetTopWinsRanking();
        /// <summary>
        /// Retrieves a player position in the games won leaderboard.
        /// </summary>
        /// <param name = "playerId">The identifier of the player to locate.</param>
        /// <returns>The ranking position, or zero when the player is absent.</returns>
        /// <exception cref="FaultException">
        /// The player identifier is invalid or the server cannot read the ranking.
        /// </exception>
        [OperationContract]
        int GetWinsRankingPositionById(int playerId);
    }
}
