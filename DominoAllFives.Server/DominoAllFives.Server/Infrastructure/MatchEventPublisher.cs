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
        private readonly object _syncRoot = new object ();
        private readonly Dictionary<Tuple<int, int>, IMatchCallback> _callbacks =
            new Dictionary<Tuple<int, int>, IMatchCallback>();
        private readonly ConcurrentDictionary<int, DeliveryQueue> _queues =
            new ConcurrentDictionary<int, DeliveryQueue>();
        private readonly Dictionary<IMatchCallback, string> _sessionKeys =
            new Dictionary<IMatchCallback, string>();
        private readonly ILogger<MatchEventPublisher> _logger =
            new ConsoleLogger<MatchEventPublisher>();
        public Action<int> MatchEnded { get; set; }

        public void Register(int matchId, int playerId, IMatchCallback callback, string sessionKey)
        {
            lock (_syncRoot)
            {
                Tuple<int, int> key = Tuple.Create(matchId, playerId);
                IMatchCallback previous;
                if (_callbacks.TryGetValue(key, out previous))
                {
                    _sessionKeys.Remove(previous);
                }

                _callbacks[key] = callback;
                _sessionKeys[callback] = sessionKey;
            }
        }

        public void Remove(int matchId, int playerId, IMatchCallback callback)
        {
            Tuple<int, int> key = Tuple.Create(matchId, playerId);
            lock (_syncRoot)
            {
                IMatchCallback current;
                if (_callbacks.TryGetValue(key, out current) && ReferenceEquals(current, callback))
                {
                    _callbacks.Remove(key);
                    _sessionKeys.Remove(callback);
                }
            }
        }

        public void Enqueue(MatchUpdate notification)
        {
            DeliveryQueue queue = _queues.GetOrAdd(notification.View.MatchId, id
                => new DeliveryQueue());
            lock (queue)
            {
                queue.Items.Enqueue(notification);
                if (queue.IsRunning)
                {
                    return;
                }

                queue.IsRunning = true;
                ThreadPool.QueueUserWorkItem(unused => Drain(queue));
            }
        }

        private void Drain(DeliveryQueue queue)
        {
            while (true)
            {
                MatchUpdate notification;
                lock (queue)
                {
                    if (queue.Items.Count == 0)
                    {
                        queue.IsRunning = false;
                        return;
                    }

                    notification = queue.Items.Dequeue();
                }

                Deliver(notification);
                if (notification.Result != null)
                {
                    MatchEnded?.Invoke(notification.View.MatchId);
                    Release(notification.View.MatchId);
                    DeliveryQueue removed;
                    _queues.TryRemove(notification.View.MatchId, out removed);
                }
            }
        }

        private void Deliver(MatchUpdate notification)
        {
            List<KeyValuePair<Tuple<int, int>, IMatchCallback>> recipients;
            lock (_syncRoot)
            {
                recipients = _callbacks.Where(entry => entry.Key.Item1 == notification.View.MatchId)
                    .ToList();
            }

            foreach (KeyValuePair<Tuple<int, int>, IMatchCallback> recipient in recipients)
            {
                if (!IsAuthenticated(recipient.Value, recipient.Key.Item2))
                {
                    Remove(recipient.Key.Item1, recipient.Key.Item2, recipient.Value);
                    continue;
                }

                try
                {
                    IMatchCallback callback = recipient.Value;
                    callback.OnMatchChanged(notification.View);
                    callback.OnPrivateHandChanged(notification.Hands.Single(hand
                        => hand.PlayerId == recipient.Key.Item2));
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

        private bool IsAuthenticated(IMatchCallback callback, int playerId)
        {
            string key;
            lock (_syncRoot)
            {
                if (!_sessionKeys.TryGetValue(callback, out key))
                {
                    return false;
                }
            }

            try
            {
                return SessionManager.GetPlayerId(key) == playerId;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        private void Release(int matchId)
        {
            List<KeyValuePair<Tuple<int, int>, IMatchCallback>> callbacks;
            lock (_syncRoot)
            {
                callbacks = _callbacks.Where(entry => entry.Key.Item1 == matchId)
                    .ToList();
            }

            foreach (KeyValuePair<Tuple<int, int>, IMatchCallback> callback in callbacks)
            {
                Remove(matchId, callback.Key.Item2, callback.Value);
            }
        }

        private void RemoveFailed(KeyValuePair<Tuple<int, int>, IMatchCallback> recipient,
            Exception ex)
        {
            Remove(recipient.Key.Item1, recipient.Key.Item2, recipient.Value);
            _logger.LogWarning(ex, "Match callback failed for player {PlayerId}.",
                recipient.Key.Item2);
        }

        private sealed class DeliveryQueue
        {
            public Queue<MatchUpdate> Items { get; } = new Queue<MatchUpdate>();
            public bool IsRunning { get; set; }
        }
    }
}
