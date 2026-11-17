using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Gameplay;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Server.Infrastructure
{
    internal sealed class MatchEventPublisher
    {
        private readonly MatchCallbackRegistry _callbacks;
        private readonly ILogger<MatchEventPublisher> _logger;

        public event Action<int> MatchEnded;

        public MatchEventPublisher(
            MatchCallbackRegistry callbacks,
            ILogger<MatchEventPublisher> logger)
        {
            _callbacks = callbacks
                ?? throw new ArgumentNullException(nameof(callbacks));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Register(int matchId, int playerId, IMatchCallback callback, string sessionKey)
        {
            _callbacks.Register(matchId, playerId, callback, sessionKey);
        }

        public void Remove(int matchId, int playerId, IMatchCallback callback)
        {
            _callbacks.Remove(matchId, playerId, callback);
        }

        public void PublishPending(MatchUpdate notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            MatchUpdateQueue queue = notification.PendingUpdates;
            if (queue == null)
            {
                throw new ArgumentException("The update has no committed publication queue.");
            }

            if (queue.TryStartPublishing())
            {
                ThreadPool.QueueUserWorkItem(DrainQueue, queue);
            }
        }

        private void DrainQueue(object state)
        {
            MatchUpdateQueue queue = (MatchUpdateQueue)state;
            MatchUpdate notification;
            while (queue.TryDequeue(out notification))
            {
                if (notification.Result != null)
                {
                    MatchEnded?.Invoke(notification.View.MatchId);
                }

                try
                {
                    Deliver(notification);
                }
                finally
                {
                    if (notification.Result != null)
                    {
                        Release(notification.View.MatchId);
                    }
                }
            }
        }

        private void Deliver(MatchUpdate notification)
        {
            List<MatchCallbackRegistration> recipients =
                _callbacks.GetCallbacks(notification.View.MatchId);

            foreach (MatchCallbackRegistration recipient in recipients)
            {
                if (!IsAuthenticated(recipient))
                {
                    Remove(recipient.MatchId, recipient.PlayerId, recipient.Callback);
                    continue;
                }

                try
                {
                    IMatchCallback callback = recipient.Callback;
                    callback.OnMatchChanged(notification.View);
                    callback.OnPrivateHandChanged(
                        notification.Hands.Single(hand => hand.PlayerId == recipient.PlayerId));
                    if (notification.Round != null)
                    {
                        callback.OnRoundFinished(notification.Round);
                    }

                    if (notification.Result != null)
                    {
                        callback.OnMatchFinished(notification.Result);
                    }
                }
                catch (CommunicationException ex)
                {
                    RemoveFailed(recipient, ex);
                }
                catch (TimeoutException ex)
                {
                    RemoveFailed(recipient, ex);
                }
                catch (ObjectDisposedException ex)
                {
                    RemoveFailed(recipient, ex);
                }
            }
        }

        private static bool IsAuthenticated(MatchCallbackRegistration registration)
        {
            try
            {
                return SessionManager.GetPlayerId(registration.SessionKey)
                    == registration.PlayerId;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        private void Release(int matchId)
        {
            List<MatchCallbackRegistration> callbacks =
                _callbacks.GetCallbacks(matchId);

            foreach (MatchCallbackRegistration callback in callbacks)
            {
                Remove(matchId, callback.PlayerId, callback.Callback);
            }
        }

        private void RemoveFailed(MatchCallbackRegistration recipient, Exception ex)
        {
            Remove(recipient.MatchId, recipient.PlayerId, recipient.Callback);
            _logger.LogDebug(
                ex, "Match callback failed for player {PlayerId}.", recipient.PlayerId);
        }
    }
}
