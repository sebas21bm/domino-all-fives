using System;
using System.Collections.Generic;

namespace DominoAllFives.Server.Infrastructure
{
    internal enum ConnectionPresence
    {
        Connected,
        InLobby,
        InMatch
    }

    internal sealed class PresenceManager
    {
        private readonly object _sync = new object();
        private readonly Dictionary<string, ConnectionPresence> _states =
            new Dictionary<string, ConnectionPresence>(StringComparer.Ordinal);

        internal void Set(string connectionId, ConnectionPresence presence)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Connection ID is required.", nameof(connectionId));
            lock (_sync) { _states[connectionId] = presence; }
        }

        internal bool TryGet(string connectionId, out ConnectionPresence presence)
        {
            lock (_sync) { return _states.TryGetValue(connectionId, out presence); }
        }

        internal void Remove(string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId)) return;
            lock (_sync) { _states.Remove(connectionId); }
        }
    }
}
