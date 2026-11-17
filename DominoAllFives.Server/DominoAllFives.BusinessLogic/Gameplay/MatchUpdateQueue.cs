using System;
using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Controllers;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class MatchUpdateQueue
    {
        private readonly object _syncRoot = new object();
        private readonly int _matchId;
        private readonly Queue<MatchUpdate> _items = new Queue<MatchUpdate>();
        private long _lastVersion;
        private bool _isRunning;
        private bool _isClosed;

        internal MatchUpdateQueue(int matchId)
        {
            if (matchId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(matchId));
            }

            _matchId = matchId;
        }

        internal void Enqueue(MatchUpdate update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            if (update.View == null
                || update.View.MatchId != _matchId
                || !update.HasStateChange)
            {
                throw new ArgumentException("A changed snapshot of this match is required.");
            }

            lock (_syncRoot)
            {
                if (_isClosed)
                {
                    throw new InvalidOperationException("The match publication queue is closed.");
                }

                if (update.View.Version != _lastVersion + 1)
                {
                    throw new InvalidOperationException("Match versions must be consecutive.");
                }

                update.PendingUpdates = this;
                _items.Enqueue(update);
                _lastVersion = update.View.Version;
                _isClosed = update.Result != null;
            }
        }

        public bool TryStartPublishing()
        {
            lock (_syncRoot)
            {
                if (_isRunning || _items.Count == 0)
                {
                    return false;
                }

                _isRunning = true;
                return true;
            }
        }

        public bool TryDequeue(out MatchUpdate update)
        {
            lock (_syncRoot)
            {
                if (!_isRunning)
                {
                    update = null;
                    return false;
                }

                if (_items.Count == 0)
                {
                    _isRunning = false;
                    update = null;
                    return false;
                }

                update = _items.Dequeue();
                return true;
            }
        }
    }
}
