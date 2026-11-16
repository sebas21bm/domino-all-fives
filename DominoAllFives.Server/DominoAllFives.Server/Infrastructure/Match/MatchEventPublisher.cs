using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Gameplay;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.Contracts.Services;
using DominoAllFives.Server.Logging;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Server.Infrastructure
{
    public sealed class MatchEventPublisher
    {
        private readonly MatchCallbackRegistry _callbacks = new MatchCallbackRegistry();
        private readonly ConcurrentDictionary<int, MatchDeliveryManager> _queues =
            new ConcurrentDictionary<int, MatchDeliveryManager>();
        private readonly ILogger<MatchEventPublisher> _logger =
            LoggerFactoryProvider.CreateLogger<MatchEventPublisher>();
        public Action<int> MatchEnded { get; set; }

        public void Register(int matchId, int playerId, IMatchCallback callback, string sessionKey)
        {
            _callbacks.Register(matchId, playerId, callback, sessionKey);
        }

        public void Remove(int matchId, int playerId, IMatchCallback callback)
        {
            _callbacks.Remove(matchId, playerId, callback);
        }

        public void Enqueue(MatchUpdate notification)
        {
            MatchDeliveryManager queue = _queues.GetOrAdd(notification.View.MatchId,
                matchId => new MatchDeliveryManager());
            if (queue.Enqueue(notification))
            {
                ThreadPool.QueueUserWorkItem(unused => Drain(queue));
            }
        }

        private void Drain(MatchDeliveryManager queue)
        {
            MatchUpdate notification;
            while (queue.TryDequeue(out notification))
            {
                Deliver(notification);
                if (notification.Result != null)
                {
                    MatchEnded?.Invoke(notification.View.MatchId);
                    Release(notification.View.MatchId);
                    MatchDeliveryManager removed;
                    _queues.TryRemove(notification.View.MatchId, out removed);
                }
            }
        }

        private void Deliver(MatchUpdate notification)
        {
            List<MatchCallbackRegistration> recipients =
                _callbacks.GetCallbacks(notification.View.MatchId);

            foreach (MatchCallbackRegistration recipient in recipients)
            {
                if (!IsAuthenticated(recipient, recipient.PlayerId))
                {
                    Remove(recipient.MatchId, recipient.PlayerId, recipient.Callback);
                    continue;
                }

                try
                {
                    IMatchCallback callback = recipient.Callback;
                    callback.OnMatchChanged(notification.View);
                    callback.OnPrivateHandChanged(notification.Hands.Single(hand
                        => hand.PlayerId == recipient.PlayerId));
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

        private bool IsAuthenticated(MatchCallbackRegistration registration, int playerId)
        {
            try
            {
                return SessionManager.GetPlayerId(registration.SessionKey) == playerId;
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

        private sealed class MatchDeliveryManager
        {
            private readonly object _syncRoot = new object();
            private readonly Queue<MatchUpdate> _items = new Queue<MatchUpdate>();
            private bool _isRunning;

            public bool Enqueue(MatchUpdate notification)
            {
                lock (_syncRoot)
                {
                    _items.Enqueue(notification);
                    if (_isRunning)
                    {
                        return false;
                    }

                    _isRunning = true;
                    return true;
                }
            }

            public bool TryDequeue(out MatchUpdate notification)
            {
                lock (_syncRoot)
                {
                    if (_items.Count == 0)
                    {
                        notification = null;
                        _isRunning = false;
                        return false;
                    }

                    notification = _items.Dequeue();
                    return true;
                }
            }
        }
    }
}
