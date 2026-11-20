using System;
using System.Collections.Generic;
using System.ServiceModel;

using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Server.Infrastructure.Chat
{
    internal sealed class ChatEventPublisher
    {
        private readonly ChatCallbackRegistry _callbacks;
        private readonly ILogger<ChatEventPublisher> _logger;

        public ChatEventPublisher(
            ChatCallbackRegistry callbacks,
            ILogger<ChatEventPublisher> logger)
        {
            _callbacks = callbacks
                ?? throw new ArgumentNullException(nameof(callbacks));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        public void PublishMessageReceived(
            IEnumerable<int> recipientIds,
            ChatMessage message)
        {
            if (recipientIds == null)
            {
                throw new ArgumentNullException(nameof(recipientIds));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            foreach (int playerId in recipientIds)
            {
                IChatCallback callback;
                if (!_callbacks.TryGetCallback(playerId, out callback))
                {
                    continue;
                }

                try
                {
                    callback.OnMessageReceived(message);
                }
                catch (CommunicationException ex)
                {
                    RemoveFailedCallback(playerId, callback, ex);
                }
                catch (TimeoutException ex)
                {
                    RemoveFailedCallback(playerId, callback, ex);
                }
                catch (ObjectDisposedException ex)
                {
                    RemoveFailedCallback(playerId, callback, ex);
                }
            }
        }

        private void RemoveFailedCallback(
            int playerId,
            IChatCallback callback,
            Exception ex)
        {
            _callbacks.RemoveCallback(playerId, callback);
            _logger.LogDebug(
                ex,
                "Chat callback failed for player {PlayerId}.",
                playerId);
        }
    }
}