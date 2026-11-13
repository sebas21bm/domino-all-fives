using System.ServiceModel;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Gameplay;

namespace DominoAllFives.Contracts.Services
{
    /// <summary>
    /// Delivers public snapshots and the recipient's private hand independently.
    /// </summary>
    public interface IMatchCallback
    {
        /// <summary>
        /// Forwards the public board, turn, scores and snapshot version.
        /// </summary>
        /// <param name = "match">The public snapshot shared with all participants.</param>
        [OperationContract(IsOneWay = true)]
        void OnMatchChanged(MatchView match);
        /// <summary>
        /// Forwards the hand intended exclusively for this participant.
        /// </summary>
        /// <param name = "hand">The private hand and version for the authenticated player.</param>
        [OperationContract(IsOneWay = true)]
        void OnPrivateHandChanged(PlayerHand hand);
        /// <summary>
        /// Receives the revealed hands and points awarded for the completed round.
        /// </summary>
        /// <param name = "result">The completed round or match outcome.</param>
        [OperationContract(IsOneWay = true)]
        void OnRoundFinished(RoundResult result);
        /// <summary>
        /// Receives the final outcome, including cancellation without a winner.
        /// </summary>
        /// <param name = "result">The completed round or match outcome.</param>
        [OperationContract(IsOneWay = true)]
        void OnMatchFinished(MatchResult result);
    }
}
