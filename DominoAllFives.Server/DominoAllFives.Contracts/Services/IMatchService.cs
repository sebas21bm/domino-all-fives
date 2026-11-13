using System.ServiceModel;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Gameplay;

namespace DominoAllFives.Contracts.Services
{
    /// <summary>
    /// Exposes authenticated match operations over a duplex WCF session.
    /// </summary>
    [ServiceContract(Namespace = "urn:domino-all-fives:services", CallbackContract = typeof(
        IMatchCallback), SessionMode = SessionMode.Required)]
    public interface IMatchService
    {
        /// <summary>
        /// Binds the authenticated player and callback to a match they participate in.
        /// </summary>
        /// <param name = "sessionKey">
        /// The authentication token, revalidated for each service operation.
        /// </param>
        /// <param name = "matchId">
        /// The match identifier, which must agree with the bound session.
        /// </param>
        /// <returns>The authoritative snapshot after the requested operation.</returns>
        /// <exception cref = "FaultException{AccessIssue}">
        /// The session is missing, revoked or bound to another match.
        /// </exception>
        /// <exception cref = "FaultException{MatchIssue}">
        /// Membership, version or a gameplay rule rejects the request.
        /// </exception>
        [OperationContract(IsInitiating = true)]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(MatchIssue))]
        MatchView OpenMatchSession(string sessionKey, int matchId);
        /// <summary>
        /// Retrieves the public match snapshot without exposing opponents' hands.
        /// </summary>
        /// <param name = "matchId">
        /// The match identifier, which must agree with the bound session.
        /// </param>
        /// <returns>The authoritative snapshot after the requested operation.</returns>
        /// <exception cref = "FaultException{AccessIssue}">
        /// The session is missing, revoked or bound to another match.
        /// </exception>
        /// <exception cref = "FaultException{MatchIssue}">
        /// Membership, version or a gameplay rule rejects the request.
        /// </exception>
        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(MatchIssue))]
        MatchView GetMatch(int matchId);
        /// <summary>
        /// Retrieves only the authenticated player's hand and its snapshot version.
        /// </summary>
        /// <param name = "matchId">
        /// The match identifier, which must agree with the bound session.
        /// </param>
        /// <returns>The authoritative snapshot after the requested operation.</returns>
        /// <exception cref = "FaultException{AccessIssue}">
        /// The session is missing, revoked or bound to another match.
        /// </exception>
        /// <exception cref = "FaultException{MatchIssue}">
        /// Membership, version or a gameplay rule rejects the request.
        /// </exception>
        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(MatchIssue))]
        PlayerHand GetHand(int matchId);
        /// <summary>
        /// Places an owned tile using server values and the expected match version.
        /// </summary>
        /// <param name = "matchId">
        /// The match identifier, which must agree with the bound session.
        /// </param>
        /// <param name = "tile">The selected tile; the server trusts only its identifier.</param>
        /// <param name = "end">The logical end on which to place the tile.</param>
        /// <param name = "expectedVersion">
        /// The snapshot version on which the player's intention is based.
        /// </param>
        /// <returns>The authoritative snapshot after the requested operation.</returns>
        /// <exception cref = "FaultException{AccessIssue}">
        /// The session is missing, revoked or bound to another match.
        /// </exception>
        /// <exception cref = "FaultException{MatchIssue}">
        /// Membership, version or a gameplay rule rejects the request.
        /// </exception>
        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(MatchIssue))]
        MatchView PlayTile(int matchId, DominoTile tile, BoardEnd end, long expectedVersion);
        /// <summary>
        /// Draws until a legal move exists or the stock is exhausted, preserving the turn.
        /// </summary>
        /// <param name = "matchId">
        /// The match identifier, which must agree with the bound session.
        /// </param>
        /// <param name = "expectedVersion">
        /// The snapshot version on which the player's intention is based.
        /// </param>
        /// <returns>The authoritative snapshot after the requested operation.</returns>
        /// <exception cref = "FaultException{AccessIssue}">
        /// The session is missing, revoked or bound to another match.
        /// </exception>
        /// <exception cref = "FaultException{MatchIssue}">
        /// Membership, version or a gameplay rule rejects the request.
        /// </exception>
        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(MatchIssue))]
        MatchView DrawTiles(int matchId, long expectedVersion);
        /// <summary>
        /// Passes only when the stock is empty and the player has no legal move.
        /// </summary>
        /// <param name = "matchId">
        /// The match identifier, which must agree with the bound session.
        /// </param>
        /// <param name = "expectedVersion">
        /// The snapshot version on which the player's intention is based.
        /// </param>
        /// <returns>The authoritative snapshot after the requested operation.</returns>
        /// <exception cref = "FaultException{AccessIssue}">
        /// The session is missing, revoked or bound to another match.
        /// </exception>
        /// <exception cref = "FaultException{MatchIssue}">
        /// Membership, version or a gameplay rule rejects the request.
        /// </exception>
        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(MatchIssue))]
        MatchView PassTurn(int matchId, long expectedVersion);
        /// <summary>
        /// Cancels the match without a winner when a participant explicitly leaves.
        /// </summary>
        /// <param name = "matchId">
        /// The match identifier, which must agree with the bound session.
        /// </param>
        /// <exception cref = "FaultException{AccessIssue}">
        /// The session is missing, revoked or bound to another match.
        /// </exception>
        /// <exception cref = "FaultException{MatchIssue}">
        /// Membership, version or a gameplay rule rejects the request.
        /// </exception>
        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(MatchIssue))]
        void LeaveMatch(int matchId);
    }
}
