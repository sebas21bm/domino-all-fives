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
using Microsoft.Extensions.Logging;
using Domain = DominoAllFives.BusinessLogic.Gameplay;

namespace DominoAllFives.Server
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant,
        IncludeExceptionDetailInFaults = false)]
    public sealed class MatchService : IMatchService
    {
        private static readonly MatchController controller;
        private static readonly MatchCallbackRegistry callbacks;
        private static readonly MatchEventPublisher publisher;
        private static readonly ILogger<MatchService> logger;
        private string _sessionKey;
        private int _playerId;
        private int _matchId;
        private IMatchCallback _callback;
        private ICommunicationObject _callbackChannel;

        static MatchService()
        {
            controller = new MatchController(MatchManager.Instance);
            callbacks = new MatchCallbackRegistry();
            ILogger<MatchEventPublisher> publisherLogger =
                LoggerFactoryProvider.CreateLogger<MatchEventPublisher>();
            publisher = new MatchEventPublisher(callbacks, publisherLogger);
            logger = LoggerFactoryProvider.CreateLogger<MatchService>();
            publisher.MatchEnded += LobbyManager.Instance.ReleaseMatch;
        }

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

            MatchView current = GetMatchView(matchId, playerId);
            if (current.Phase != MatchStatus.InProgress)
            {
                publisher.Remove(matchId, playerId, _callback);
            }

            return current;
        }

        public MatchView GetMatch(int matchId)
        {
            ValidateSessionCanAccessMatch(matchId);
            return GetMatchView(matchId, _playerId);
        }

        public PlayerHand GetHand(int matchId)
        {
            ValidateSessionCanAccessMatch(matchId);
            return GetPlayerHand(matchId, _playerId);
        }

        public void PlayTile(int matchId, DominoTile tile, BoardEnd end, long expectedVersion)
        {
            ValidateSessionCanAccessMatch(matchId);
            if (tile == null)
            {
                throw new FaultException<MatchIssue>(MatchIssue.TileNotOwned);
            }

            Move move = new Move(tile.TileId, (Domain.BoardEndPosition)end, expectedVersion);
            try
            {
                MatchUpdate update = controller.PlayTile(matchId, _playerId, move);
                publisher.PublishPending(update);
            }
            catch (Domain.MatchRuleException ex)
            {
                throw new FaultException<MatchIssue>((MatchIssue)ex.Issue);
            }
        }

        public void DrawTiles(int matchId, long expectedVersion)
        {
            ValidateSessionCanAccessMatch(matchId);
            try
            {
                MatchUpdate update = controller.DrawTiles(matchId, _playerId, expectedVersion);
                publisher.PublishPending(update);
            }
            catch (Domain.MatchRuleException ex)
            {
                throw new FaultException<MatchIssue>((MatchIssue)ex.Issue);
            }
        }

        public void PassTurn(int matchId, long expectedVersion)
        {
            ValidateSessionCanAccessMatch(matchId);
            try
            {
                MatchUpdate update = controller.PassTurn(matchId, _playerId, expectedVersion);
                publisher.PublishPending(update);
            }
            catch (Domain.MatchRuleException ex)
            {
                throw new FaultException<MatchIssue>((MatchIssue)ex.Issue);
            }
        }

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
            if (GetAuthenticatedPlayerId(_sessionKey) != _playerId || matchId != _matchId)
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
            catch (KeyNotFoundException ex)
            {
                logger.LogDebug(ex, "The match session could not resolve the player.");
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
