using System;
using System.ServiceModel;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using MatchService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Services
{
    /// <summary>
    /// Owns the duplex channel and forwards match snapshots to the UI process.
    /// </summary>
    [CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode =
        ConcurrencyMode.Multiple)]
    public sealed class MatchRemoteService : IMatchClient, IMatchServiceCallback
    {
        private MatchServiceClient _service;
        private bool _disposed;
        public event Action<MatchView> MatchChanged;
        public event Action<PlayerHand> HandChanged;
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
        public MatchView Open(string sessionKey, int matchId)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MatchRemoteService));
            }

            ResetConnection();
            try
            {
                _service = new MatchServiceClient(new InstanceContext(this), "MatchEndpoint");
                return _service.OpenMatchSession(sessionKey, matchId);
            }
            catch (CommunicationException)
            {
                ResetConnection();
                throw;
            }
            catch (TimeoutException)
            {
                ResetConnection();
                throw;
            }
        }

        /// <summary>
        /// Retrieves the public match snapshot without exposing opponents' hands.
        /// </summary>
        /// <param name = "matchId">
        /// The match identifier, which must agree with the bound session.
        /// </param>
        /// <returns>The authoritative snapshot after the requested operation.</returns>
        public MatchView GetMatch(int matchId)
        {
            return _service.GetMatch(matchId);
        }

        /// <summary>
        /// Retrieves only the authenticated player's hand and its snapshot version.
        /// </summary>
        /// <param name = "matchId">
        /// The match identifier, which must agree with the bound session.
        /// </param>
        /// <returns>The authoritative snapshot after the requested operation.</returns>
        public PlayerHand GetHand(int matchId)
        {
            return _service.GetHand(matchId);
        }

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
        public MatchView Play(int matchId, DominoTile tile, BoardEnd end, long version)
        {
            return _service.PlayTile(matchId, tile, end, version);
        }

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
        public MatchView Draw(int matchId, long version)
        {
            return _service.DrawTiles(matchId, version);
        }

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
        public MatchView Pass(int matchId, long version)
        {
            return _service.PassTurn(matchId, version);
        }

        /// <summary>
        /// Explicitly leaves and cancels the match; closing the transport alone does not leave.
        /// </summary>
        /// <param name = "matchId">
        /// The match identifier, which must agree with the bound session.
        /// </param>
        public void Leave(int matchId)
        {
            _service.LeaveMatch(matchId);
        }

        /// <summary>
        /// Forwards the public board, turn, scores and snapshot version.
        /// </summary>
        /// <param name = "match">The public snapshot shared with all participants.</param>
        public void OnMatchChanged(MatchView match)
        {
            MatchChanged?.Invoke(match);
        }

        /// <summary>
        /// Forwards the hand intended exclusively for this participant.
        /// </summary>
        /// <param name = "hand">The private hand and version for the authenticated player.</param>
        public void OnPrivateHandChanged(PlayerHand hand)
        {
            HandChanged?.Invoke(hand);
        }

        /// <summary>
        /// Receives the revealed hands and points awarded for the completed round.
        /// </summary>
        /// <param name = "result">The completed round or match outcome.</param>
         // These results are also retained in MatchView, allowing recovery after a missed callback.
        public void OnRoundFinished(RoundResult result)
        {
        }

        /// <summary>
        /// Receives the final outcome, including cancellation without a winner.
        /// </summary>
        /// <param name = "result">The completed round or match outcome.</param>
        public void OnMatchFinished(MatchResult result)
        {
        }

        /// <summary>
        /// Closes or aborts the transport without sending an explicit leave operation.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            ResetConnection();
        }

        private void ResetConnection()
        {
            Close(_service);
            _service = null;
        }

        private static void Close(ICommunicationObject channel)
        {
            if (channel == null)
            {
                return;
            }

            try
            {
                if (channel.State == CommunicationState.Faulted)
                {
                    channel.Abort();
                }
                else
                {
                    channel.Close();
                }
            }
            catch (CommunicationException)
            {
                channel.Abort();
            }
            catch (TimeoutException)
            {
                channel.Abort();
            }
        }
    }
}
