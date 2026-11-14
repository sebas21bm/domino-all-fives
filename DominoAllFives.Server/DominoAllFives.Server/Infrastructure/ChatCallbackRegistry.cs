using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DominoAllFives.Contracts.Services;

namespace DominoAllFives.Server.Infrastructure.Chat
{
    internal sealed class ChatCallbackRegistry
    {
        private readonly ConcurrentDictionary<int, IChatCallback> _callbacks =
            new ConcurrentDictionary<int, IChatCallback>();
        public bool RegisterCallback(int playerId, IChatCallback callback)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            _callbacks[playerId] = callback;
            return true;
        }

        public IChatCallback GetCallback(int playerId)
        {
            IChatCallback callback;
            if (!_callbacks.TryGetValue(playerId, out callback))
            {
                throw new KeyNotFoundException();
            }

            return callback;
        }

        public bool RemoveCallback(int playerId, IChatCallback callback)
        {
            ICollection<KeyValuePair<int, IChatCallback>> entries = _callbacks;
            return entries.Remove(new KeyValuePair<int, IChatCallback>(playerId, callback));
        }
    }
}
