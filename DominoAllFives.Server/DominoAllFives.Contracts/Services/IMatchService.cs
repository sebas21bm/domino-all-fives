using System.ServiceModel;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Gameplay;

namespace DominoAllFives.Contracts.Services
{
    [ServiceContract(
        Namespace = "urn:domino-all-fives:services",
        CallbackContract = typeof(IMatchCallback),
        SessionMode = SessionMode.Required)]
    public interface IMatchService
    {
        /// <summary>
        /// Binds an authenticated player and callback to a match.
        /// </summary>
        /// <param name="sessionKey">The authentication token.</param>
        /// <param name="matchId">The match identifier.</param>
        /// <returns>The current public match snapshot.</returns>
        /// <exception cref="FaultException{AccessIssue}">
        /// The token is invalid or this connection is already bound.
        /// </exception>
        /// <exception cref="FaultException{MatchIssue}">
        /// The match is unavailable or the player does not belong to it.
        /// </exception>
        [OperationContract(IsInitiating = true)]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(MatchIssue))]
        MatchView OpenMatchSession(string sessionKey, int matchId);

        /// <summary>
        /// Retrieves the public state of the match.
        /// </summary>
        /// <param name="matchId">The bound match identifier.</param>
        /// <returns>The board, turn and scores at the current version.</returns>
        /// <exception cref="FaultException{AccessIssue}">
        /// The session is invalid or cannot access the match.
        /// </exception>
        /// <exception cref="FaultException{MatchIssue}">
        /// The match is unavailable or the player does not belong to it.
        /// </exception>
        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(MatchIssue))]
        MatchView GetMatch(int matchId);

        /// <summary>
        /// Retrieves the authenticated player's hand.
        /// </summary>
        /// <param name="matchId">The bound match identifier.</param>
        /// <returns>The player's private tiles and their snapshot version.</returns>
        /// <exception cref="FaultException{AccessIssue}">
        /// The session is invalid or cannot access the match.
        /// </exception>
        /// <exception cref="FaultException{MatchIssue}">
        /// The match is unavailable or the player does not belong to it.
        /// </exception>
        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(MatchIssue))]
        PlayerHand GetHand(int matchId);

        /// <summary>
        /// Places an owned tile on the board.
        /// </summary>
        /// <param name="matchId">The bound match identifier.</param>
        /// <param name="tile">The selected tile, identified by its ID.</param>
        /// <param name="end">The board end to play on.</param>
        /// <param name="expectedVersion">The version displayed when the move was selected.</param>
        /// <exception cref="FaultException{AccessIssue}">
        /// The session is invalid or cannot access the match.
        /// </exception>
        /// <exception cref="FaultException{MatchIssue}">
        /// The match, version, turn or placement does not allow the move.
        /// </exception>
        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(MatchIssue))]
        void PlayTile(int matchId, DominoTile tile, BoardEnd end, long expectedVersion);

        /// <summary>
        /// Draws until a legal move exists or the stock is exhausted.
        /// </summary>
        /// <param name="matchId">The bound match identifier.</param>
        /// <param name="expectedVersion">The version used to request the action.</param>
        /// <exception cref="FaultException{AccessIssue}">
        /// The session is invalid or cannot access the match.
        /// </exception>
        /// <exception cref="FaultException{MatchIssue}">
        /// The match, version, turn or drawing rules disallow the action.
        /// </exception>
        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(MatchIssue))]
        void DrawTiles(int matchId, long expectedVersion);

        /// <summary>
        /// Passes when the stock is empty and there is no legal move.
        /// </summary>
        /// <param name="matchId">The bound match identifier.</param>
        /// <param name="expectedVersion">The version used to request the action.</param>
        /// <exception cref="FaultException{AccessIssue}">
        /// The session is invalid or cannot access the match.
        /// </exception>
        /// <exception cref="FaultException{MatchIssue}">
        /// The match, version, turn or passing rules disallow the action.
        /// </exception>
        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(MatchIssue))]
        void PassTurn(int matchId, long expectedVersion);

        /// <summary>
        /// Cancels an active match without a winner.
        /// </summary>
        /// <param name="matchId">The bound match identifier.</param>
        /// <exception cref="FaultException{AccessIssue}">
        /// The session is invalid or cannot access the match.
        /// </exception>
        /// <exception cref="FaultException{MatchIssue}">
        /// The match is unavailable or the player does not belong to it.
        /// </exception>
        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(MatchIssue))]
        void LeaveMatch(int matchId);
    }
}
