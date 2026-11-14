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
            _callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void PublishMessageReceived(
            IEnumerable<Player> recipients,
            ChatMessage message) =>
            Publish(
            recipients,
            message,
            callback => callback.OnMessageReceived(
            message));
        private void Publish(
            IEnumerable<Player> recipients,
            ChatMessage message,
            Action<IChatCallback> notification)
        {
            if (recipients == null)
            {
                throw new ArgumentNullException(nameof(recipients));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            foreach (Player player in recipients)
            {
                IChatCallback callback;
                try
                {
                    callback = _callbacks.GetCallback(player.PlayerId);
                }
                catch (KeyNotFoundException)
                {
                    continue;
                }

                try
                {
                    notification(callback);
                }
                catch (CommunicationException ex)
                {
                    _callbacks.RemoveCallback(player.PlayerId, callback);
                    _logger.LogWarning(
                        ex,
                        "Chat callback failed for player {PlayerId}.",
                        player.PlayerId);
                }
                catch (TimeoutException ex)
                {
                    _callbacks.RemoveCallback(player.PlayerId, callback);
                    _logger.LogWarning(
                        ex,
                        "Chat callback failed for player {PlayerId}.",
                        player.PlayerId);
                }
                catch (ObjectDisposedException ex)
                {
                    _callbacks.RemoveCallback(player.PlayerId, callback);
                    _logger.LogWarning(
                        ex,
                        "Chat callback failed for player {PlayerId}.",
                        player.PlayerId);
                }
            }
        }
    }
}
