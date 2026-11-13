using System;
using System.Collections.Generic;
using System.ServiceModel;
using DominoAllFives.BusinessLogic.Controllers;
using Domain = DominoAllFives.BusinessLogic.Gameplay;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Gameplay;
using DominoAllFives.Contracts.Services;
using DominoAllFives.Server.Infrastructure;

namespace DominoAllFives.Server
{
    /// <summary>
    /// Validates the session identity before applying authoritative match operations.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode =
        ConcurrencyMode.Single, IncludeExceptionDetailInFaults = false)]
    public sealed class MatchService : IMatchService
    {
        private string _sessionKey;
        private int _playerId;
        private int _matchId;
        private IMatchCallback _callback;
        private ICommunicationObject _channel;
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
        public MatchView OpenMatchSession(string sessionKey, int matchId)
        {
            if (_sessionKey != null)
            {
                throw new FaultException<AccessIssue>(AccessIssue.PermissionDenied);
            }

            int playerId = ResolvePlayer(sessionKey);
            MatchView view = Execute(() => GameRuntime.Controller.GetMatch(matchId, playerId));
            _sessionKey = sessionKey;
            _playerId = playerId;
            _matchId = matchId;
            _callback = OperationContext.Current.GetCallbackChannel<IMatchCallback>();
            _channel = _callback as ICommunicationObject;
            GameRuntime.Publisher.Register(matchId, playerId, _callback, sessionKey);
            if (_channel != null)
            {
                _channel.Closed += OnChannelEnded;
                _channel.Faulted += OnChannelEnded;
            }

            // GetHand/GetMatch recover a mutation that raced subscription.
            return Execute(() => GameRuntime.Controller.GetMatch(matchId, playerId));
        }

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
        public MatchView GetMatch(int matchId)
        {
            EnsureSession(matchId);
            return Execute(() => GameRuntime.Controller.GetMatch(matchId, _playerId));
        }

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
        public PlayerHand GetHand(int matchId)
        {
            EnsureSession(matchId);
            return Execute(() => GameRuntime.Controller.GetHand(matchId, _playerId));
        }

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
        public MatchView PlayTile(int matchId, DominoTile tile, BoardEnd end, long expectedVersion)
        {
            EnsureSession(matchId);
            if (tile == null)
            {
                throw new FaultException<MatchIssue>(MatchIssue.TileNotOwned);
            }

            // Values and SpecialPower sent by the client are deliberately ignored.
            Move move = new Move
            {
                TileId = tile.TileId,
                End = (Domain.End)end,
                ExpectedVersion = expectedVersion
            };
            return Execute(() => GameRuntime.Controller.Play(matchId, _playerId, move));
        }

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
        public MatchView DrawTiles(int matchId, long expectedVersion)
        {
            EnsureSession(matchId);
            return Execute(() => GameRuntime.Controller.Draw(matchId, _playerId, expectedVersion));
        }

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
        public MatchView PassTurn(int matchId, long expectedVersion)
        {
            EnsureSession(matchId);
            return Execute(() => GameRuntime.Controller.Pass(matchId, _playerId, expectedVersion));
        }

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
        public void LeaveMatch(int matchId)
        {
            EnsureSession(matchId);
            Execute(() => CancelMatch(matchId));
        }

        private bool CancelMatch(int matchId)
        {
            GameRuntime.Controller.Leave(matchId, _playerId);
            return true;
        }

        private void EnsureSession(int matchId)
        {
            if (ResolvePlayer(_sessionKey) != _playerId || matchId != _matchId)
            {
                throw new FaultException<AccessIssue>(AccessIssue.PermissionDenied);
            }
        }

        private static int ResolvePlayer(string sessionKey)
        {
            if (string.IsNullOrWhiteSpace(sessionKey))
            {
                throw new FaultException<AccessIssue>(AccessIssue.SessionRequired);
            }

            try
            {
                return SessionManager.GetPlayerId(sessionKey);
            }
            catch (KeyNotFoundException)
            {
                throw new FaultException<AccessIssue>(AccessIssue.SessionRequired);
            }
        }

        private static T Execute<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (Domain.MatchRuleException ex)
            {
                throw new FaultException<MatchIssue>((MatchIssue)ex.Issue);
            }
        }

        private void OnChannelEnded(object sender, EventArgs eventArgs)
        {
            GameRuntime.Publisher.Remove(_matchId, _playerId, _callback);
            if (_channel != null)
            {
                _channel.Closed -= OnChannelEnded;
                _channel.Faulted -= OnChannelEnded;
            }
        }
    }
}
