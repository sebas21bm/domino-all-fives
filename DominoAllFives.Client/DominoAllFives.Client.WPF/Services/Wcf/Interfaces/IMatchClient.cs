using System;
using MatchService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Interfaces
{
    /// <summary>
    /// Provides match operations and snapshots independently of the WCF transport.
    /// </summary>
    public interface IMatchClient : IDisposable
    {
        event Action<MatchView> MatchChanged;
        event Action<PlayerHand> HandChanged;
        /// <summary>
        /// Reopens the transport and binds the authenticated player to the requested match.
        /// </summary>
        /// <param name = "sessionKey">
        /// The authentication token, revalidated for each service operation.
        /// </param>
        /// <param name = "matchId">
        /// The match identifier, which must agree with the bound session.
        /// </param>
        /// <returns>The authoritative snapshot after the requested operation.</returns>
        MatchView Open(string sessionKey, int matchId);
        /// <summary>
        /// Retrieves the public match snapshot without exposing opponents' hands.
        /// </summary>
        /// <param name = "matchId">
        /// The match identifier, which must agree with the bound session.
        /// </param>
        /// <returns>The authoritative snapshot after the requested operation.</returns>
        MatchView GetMatch(int matchId);
        /// <summary>
        /// Retrieves only the authenticated player's hand and its snapshot version.
        /// </summary>
        /// <param name = "matchId">
        /// The match identifier, which must agree with the bound session.
        /// </param>
        /// <returns>The authoritative snapshot after the requested operation.</returns>
        PlayerHand GetHand(int matchId);
        /// <summary>
        /// Requests placement of the selected tile against the displayed match version.
        /// </summary>
        /// <param name = "matchId">
        /// The match identifier, which must agree with the bound session.
        /// </param>
        /// <param name = "tile">The selected tile; the server trusts only its identifier.</param>
        /// <param name = "end">The logical end on which to place the tile.</param>
        /// <param name = "version">
        /// The snapshot version on which the player's intention is based.
        /// </param>
        /// <returns>The authoritative snapshot after the requested operation.</returns>
        MatchView Play(int matchId, DominoTile tile, BoardEnd end, long version);
        /// <summary>
        /// Requests drawing from the stock without advancing the player's turn.
        /// </summary>
        /// <param name = "matchId">
        /// The match identifier, which must agree with the bound session.
        /// </param>
        /// <param name = "version">
        /// The snapshot version on which the player's intention is based.
        /// </param>
        /// <returns>The authoritative snapshot after the requested operation.</returns>
        MatchView Draw(int matchId, long version);
        /// <summary>
        /// Requests a pass against the displayed match version.
        /// </summary>
        /// <param name = "matchId">
        /// The match identifier, which must agree with the bound session.
        /// </param>
        /// <param name = "version">
        /// The snapshot version on which the player's intention is based.
        /// </param>
        /// <returns>The authoritative snapshot after the requested operation.</returns>
        MatchView Pass(int matchId, long version);
        /// <summary>
        /// Explicitly leaves and cancels the match; closing the transport alone does not leave.
        /// </summary>
        /// <param name = "matchId">
        /// The match identifier, which must agree with the bound session.
        /// </param>
        void Leave(int matchId);
    }
}
