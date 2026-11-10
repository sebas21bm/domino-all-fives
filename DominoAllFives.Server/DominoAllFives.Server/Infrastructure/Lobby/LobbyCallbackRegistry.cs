using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DominoAllFives.Contracts.Services;

namespace DominoAllFives.Server.Infrastructure.Lobby
{
    internal sealed class LobbyCallbackRegistry
    {
        private readonly ConcurrentDictionary<int, ILobbyCallback> _callbacks =
            new ConcurrentDictionary<int, ILobbyCallback>();

        public bool RegisterCallback(int playerId, ILobbyCallback callback)
        {
            if (playerId <= 0) throw new ArgumentOutOfRangeException(nameof(playerId));
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            return _callbacks.TryAdd(playerId, callback);
        }

        public ILobbyCallback GetCallback(int playerId)
        {
            ILobbyCallback callback;
            if (!_callbacks.TryGetValue(playerId, out callback)) throw new KeyNotFoundException();
            return callback;
        }

        public bool RemoveCallback(int playerId, ILobbyCallback callback)
        {
            ICollection<KeyValuePair<int, ILobbyCallback>> entries = _callbacks;
            return entries.Remove(new KeyValuePair<int, ILobbyCallback>(playerId, callback));
        }
    }
}
