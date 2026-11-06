using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace DominoAllFives.Server.Infrastructure
{
    internal sealed class CallbackRegistry<TCallback> where TCallback : class
    {
        private sealed class CallbackEntry
        {
            internal TCallback Callback { get; set; }

            internal ICommunicationObject Channel { get; set; }
        }

        private readonly Dictionary<string, CallbackEntry> _entries =
            new Dictionary<string, CallbackEntry>();

        internal void Register(
            string connectionId,
            TCallback callback,
            ICommunicationObject channel)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                throw new ArgumentException(
                    "Connection ID is required.",
                    nameof(connectionId));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            _entries[connectionId] = new CallbackEntry
            {
                Callback = callback,
                Channel = channel
            };
        }

        internal void Unregister(string connectionId)
        {
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                _entries.Remove(connectionId);
            }
        }

        internal bool TryGet(string connectionId, out TCallback callback)
        {
            bool isFound = false;
            callback = null;

            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                if (_entries.TryGetValue(connectionId, out CallbackEntry entry) &&
                    entry.Channel.State == CommunicationState.Opened)
                {
                    callback = entry.Callback;
                    isFound = true;
                }
                else
                {
                    _entries.Remove(connectionId);
                }
            }

            return isFound;
        }

        internal List<KeyValuePair<string, TCallback>> Snapshot(
            IEnumerable<string> connectionIds)
        {
            if (connectionIds == null)
            {
                throw new ArgumentNullException(nameof(connectionIds));
            }

            List<KeyValuePair<string, TCallback>> activeCallbacks =
                new List<KeyValuePair<string, TCallback>>();

            foreach (string id in connectionIds.Distinct())
            {
                if (TryGet(id, out TCallback callback))
                {
                    activeCallbacks.Add(
                        new KeyValuePair<string, TCallback>(id, callback));
                }
            }

            return activeCallbacks;
        }
    }
}