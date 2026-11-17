using System;
using System.Collections.Generic;
using System.ServiceModel;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Gameplay;
using DominoAllFives.Contracts.Services;
using DominoAllFives.Server.Infrastructure;
using DominoAllFives.Server.Logging;
using Domain = DominoAllFives.BusinessLogic.Gameplay;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Server
{
    /// <summary>
    /// Validates the session identity before applying authoritative match operations.
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant,
        IncludeExceptionDetailInFaults = false)]
    public sealed class MatchService : IMatchService
    {
        private static readonly MatchController controller;
        private static readonly MatchCallbackRegistry callbacks;
        private static readonly MatchEventPublisher publisher;
        private static readonly ILogger<MatchEventPublisher> publisherLogger;
        private string _sessionKey;
        private int _playerId;
        private int _matchId;
        private IMatchCallback _callback;
        private ICommunicationObject _callbackChannel;

        static MatchService()
        {
            controller = new MatchController(MatchManager.Instance);
            callbacks = new MatchCallbackRegistry();
            publisherLogger = LoggerFactoryProvider.CreateLogger<MatchEventPublisher>();
            publisher = new MatchEventPublisher(
                callbacks,
                publisherLogger);
            publisher.MatchEnded += LobbyManager.Instance.ReleaseMatch;
        }

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

            int playerId = GetAuthenticatedPlayerId(sessionKey);
            GetMatchView(matchId, playerId);
            _sessionKey = sessionKey;
            _playerId = playerId;
            _matchId = matchId;
            _callback = OperationContext.Current.GetCallbackChannel<IMatchCallback>();
            _callbackChannel = _callback as ICommunicationObject;
            publisher.Register(matchId, playerId, _callback, sessionKey);
            if (_callbackChannel != null)
            {
                _callbackChannel.Closed += OnChannelEnded;
                _callbackChannel.Faulted += OnChannelEnded;
            }

            // The second snapshot recovers a mutation that raced callback registration.
            MatchView current = GetMatchView(matchId, playerId);
            if (current.Phase != MatchStatus.InProgress)
            {
                publisher.Remove(matchId, playerId, _callback);
            }

            return current;
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
            ValidateSessionCanAccessMatch(matchId);
            return GetMatchView(matchId, _playerId);
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
            ValidateSessionCanAccessMatch(matchId);
            return GetPlayerHand(matchId, _playerId);
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
        /// <remarks>State changes are delivered through callbacks; faults use the reply.</remarks>
        /// <exception cref = "FaultException{AccessIssue}">
        /// The session is missing, revoked or bound to another match.
        /// </exception>
        /// <exception cref = "FaultException{MatchIssue}">
        /// Membership, version or a gameplay rule rejects the request.
        /// </exception>
        public void PlayTile(int matchId, DominoTile tile, BoardEnd end, long expectedVersion)
        {
            ValidateSessionCanAccessMatch(matchId);
            if (tile == null)
            {
                throw new FaultException<MatchIssue>(MatchIssue.TileNotOwned);
            }

            // Values and SpecialPower sent by the client are deliberately ignored.
            Move move = new Move
            {
                TileId = tile.TileId,
                End = (Domain.BoardEndPosition)end,
                ExpectedVersion = expectedVersion
            };
            try
            {
                MatchUpdate update = controller.PlayTile(
                    matchId,
                    _playerId,
                    move);
                publisher.PublishPending(update);
            }
            catch (Domain.MatchRuleException ex)
            {
                throw new FaultException<MatchIssue>((MatchIssue)ex.Issue);
            }
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
        /// <remarks>State changes are delivered through callbacks; faults use the reply.</remarks>
        /// <exception cref = "FaultException{AccessIssue}">
        /// The session is missing, revoked or bound to another match.
        /// </exception>
        /// <exception cref = "FaultException{MatchIssue}">
        /// Membership, version or a gameplay rule rejects the request.
        /// </exception>
        public void DrawTiles(int matchId, long expectedVersion)
        {
            ValidateSessionCanAccessMatch(matchId);
            try
            {
                MatchUpdate update = controller.DrawTiles(
                    matchId,
                    _playerId,
                    expectedVersion);
                publisher.PublishPending(update);
            }
            catch (Domain.MatchRuleException ex)
            {
                throw new FaultException<MatchIssue>((MatchIssue)ex.Issue);
            }
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
        /// <remarks>State changes are delivered through callbacks; faults use the reply.</remarks>
        /// <exception cref = "FaultException{AccessIssue}">
        /// The session is missing, revoked or bound to another match.
        /// </exception>
        /// <exception cref = "FaultException{MatchIssue}">
        /// Membership, version or a gameplay rule rejects the request.
        /// </exception>
        public void PassTurn(int matchId, long expectedVersion)
        {
            ValidateSessionCanAccessMatch(matchId);
            try
            {
                MatchUpdate update = controller.PassTurn(
                    matchId,
                    _playerId,
                    expectedVersion);
                publisher.PublishPending(update);
            }
            catch (Domain.MatchRuleException ex)
            {
                throw new FaultException<MatchIssue>((MatchIssue)ex.Issue);
            }
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
            ValidateSessionCanAccessMatch(matchId);
            try
            {
                MatchUpdate update = controller.LeaveMatch(matchId, _playerId);
                publisher.PublishPending(update);
            }
            catch (Domain.MatchRuleException ex)
            {
                throw new FaultException<MatchIssue>((MatchIssue)ex.Issue);
            }
        }

        private void ValidateSessionCanAccessMatch(int matchId)
        {
            if (GetAuthenticatedPlayerId(_sessionKey) != _playerId
                || matchId != _matchId)
            {
                throw new FaultException<AccessIssue>(AccessIssue.PermissionDenied);
            }
        }

        private static int GetAuthenticatedPlayerId(string sessionKey)
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

        private static MatchView GetMatchView(int matchId, int playerId)
        {
            try
            {
                return controller.GetMatch(matchId, playerId);
            }
            catch (Domain.MatchRuleException ex)
            {
                throw new FaultException<MatchIssue>((MatchIssue)ex.Issue);
            }
        }

        private static PlayerHand GetPlayerHand(int matchId, int playerId)
        {
            try
            {
                return controller.GetPlayerHand(matchId, playerId);
            }
            catch (Domain.MatchRuleException ex)
            {
                throw new FaultException<MatchIssue>((MatchIssue)ex.Issue);
            }
        }

        private void OnChannelEnded(object sender, EventArgs eventArgs)
        {
            publisher.Remove(_matchId, _playerId, _callback);
            if (_callbackChannel != null)
            {
                _callbackChannel.Closed -= OnChannelEnded;
                _callbackChannel.Faulted -= OnChannelEnded;
            }
        }
    }
}
