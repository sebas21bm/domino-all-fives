using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DominoAllFives.Contracts.Services;

namespace DominoAllFives.Server.Infrastructure
{
    internal sealed class MatchCallbackRegistry
    {
        private readonly ConcurrentDictionary<Tuple<int, int>, MatchCallbackRegistration>
            _callbacks =
            new ConcurrentDictionary<Tuple<int, int>, MatchCallbackRegistration>();

        public void Register(int matchId, int playerId, IMatchCallback callback, string sessionKey)
        {
            _callbacks[Tuple.Create(matchId, playerId)] =
                new MatchCallbackRegistration(matchId, playerId, callback, sessionKey);
        }

        public void Remove(int matchId, int playerId, IMatchCallback callback)
        {
            Tuple<int, int> key = Tuple.Create(matchId, playerId);
            MatchCallbackRegistration current;
            if (_callbacks.TryGetValue(key, out current)
                && ReferenceEquals(current.Callback, callback))
            {
                ICollection<KeyValuePair<Tuple<int, int>, MatchCallbackRegistration>> entries =
                    _callbacks;
                entries.Remove(
                    new KeyValuePair<Tuple<int, int>, MatchCallbackRegistration>(key, current));
            }
        }

        public List<MatchCallbackRegistration> GetCallbacks(int matchId)
        {
            return _callbacks.Values
                .Where(registration => registration.MatchId == matchId)
                .ToList();
        }
    }

    internal sealed class MatchCallbackRegistration
    {
        public int MatchId { get; }
        public int PlayerId { get; }
        public IMatchCallback Callback { get; }
        public string SessionKey { get; }

        public MatchCallbackRegistration(
            int matchId, int playerId, IMatchCallback callback, string sessionKey)
        {
            MatchId = matchId;
            PlayerId = playerId;
            Callback = callback;
            SessionKey = sessionKey;
        }
    }
}
