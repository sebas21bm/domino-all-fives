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
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            return _callbacks.TryAdd(playerId, callback);
        }

        public bool TryGetCallback(int playerId, out ILobbyCallback callback)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            return _callbacks.TryGetValue(playerId, out callback);
        }

        public bool RemoveCallback(int playerId, ILobbyCallback callback)
        {
            ICollection<KeyValuePair<int, ILobbyCallback>> entries = _callbacks;
            return entries.Remove(new KeyValuePair<int, ILobbyCallback>(playerId, callback));
        }
    }
}
