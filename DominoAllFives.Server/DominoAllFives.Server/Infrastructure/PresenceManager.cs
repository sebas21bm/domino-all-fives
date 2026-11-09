using System;
using System.Collections.Generic;

namespace DominoAllFives.Server.Infrastructure
{
    public enum PlayerPresence
    {
        Online,
        Offline,
        InGame
    }

    internal sealed class PresenceManager
    {
        private readonly Dictionary<int, PlayerPresence> _presences = new Dictionary<int, PlayerPresence>();

        internal void SetPresence(int playerId, PlayerPresence presence)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            _presences[playerId] = presence;
        }

        internal bool TryGetPresence(int playerId, out PlayerPresence presence)
        {
            bool isFound = false;
            presence = PlayerPresence.Offline;

            if (playerId > 0)
            {
                isFound = _presences.TryGetValue(playerId, out presence);
            }

            return isFound;
        }

        internal void Remove(int playerId)
        {
            if (playerId > 0)
            {
                _presences.Remove(playerId);
            }
        }
    }
}