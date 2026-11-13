using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Gameplay;

namespace DominoAllFives.BusinessLogic.Managers
{
    public sealed class MatchManager
    {
        private const int RetentionMinutes = 30;
        private readonly ConcurrentDictionary<int, ActiveMatch> _matches =
            new ConcurrentDictionary<int, ActiveMatch>();
        public ActiveMatch GetMatch(int matchId)
        {
            ActiveMatch active;
            if (!_matches.TryGetValue(matchId, out active))
            {
                throw new MatchRuleException(MatchIssue.MatchNotFound);
            }

            lock (active.SyncRoot)
            {
                if (HasExpired(active))
                {
                    RemoveMatch(matchId);
                    throw new MatchRuleException(MatchIssue.MatchNotFound);
                }
            }

            return active;
        }

        public ActiveMatch CreateMatch(int matchId, IEnumerable<int> players, int targetPoints)
        {
            foreach (KeyValuePair<int, ActiveMatch> entry in _matches)
            {
                lock (entry.Value.SyncRoot)
                {
                    if (HasExpired(entry.Value))
                    {
                        RemoveMatch(entry.Key);
                    }
                }
            }

            return _matches.GetOrAdd(matchId, id => new ActiveMatch(new Match(id, players,
                targetPoints, Deck.Create)));
        }

        public void RemoveMatch(int matchId)
        {
            ActiveMatch removed;
            _matches.TryRemove(matchId, out removed);
        }

        private static bool HasExpired(ActiveMatch active)
        {
            return active.FinishedAt.HasValue
                && DateTime.UtcNow - active.FinishedAt.Value > TimeSpan.FromMinutes(30);
        }
    }
}
