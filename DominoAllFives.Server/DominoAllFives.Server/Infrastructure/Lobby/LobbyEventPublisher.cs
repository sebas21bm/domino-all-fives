using System;
using System.Collections.Generic;
using System.ServiceModel;
using DominoAllFives.Contracts.Domain;
using LobbyModel = DominoAllFives.Contracts.Domain.Lobby;
using DominoAllFives.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Server.Infrastructure.Lobby
{
    internal sealed class LobbyEventPublisher
    {
        private readonly LobbyCallbackRegistry _callbacks;
        private readonly ILogger<LobbyEventPublisher> _logger;
        public LobbyEventPublisher(
            LobbyCallbackRegistry callbacks,
            ILogger<LobbyEventPublisher> logger)
        {
            _callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void PublishLobbyChanged(
            LobbyModel lobby) =>
            Publish(
            lobby,
            callback => callback.OnLobbyChanged(
            lobby));
        public void PublishLobbyClosed(
            LobbyModel lobby) =>
            Publish(
            lobby,
            callback => callback.OnLobbyClosed());
        public void PublishMatchStarted(
            LobbyModel lobby,
            int matchId) =>
            Publish(
            lobby,
            callback => callback.OnMatchStarted(
            matchId));
        private void Publish(LobbyModel lobby, Action<ILobbyCallback> notification)
        {
            if (lobby == null)
            {
                throw new ArgumentNullException(nameof(lobby));
            }

            foreach (Player player in lobby.Participants)
            {
                ILobbyCallback callback;
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
                        "Lobby callback failed for player {PlayerId}.",
                        player.PlayerId);
                }
                catch (TimeoutException ex)
                {
                    _callbacks.RemoveCallback(player.PlayerId, callback);
                    _logger.LogWarning(
                        ex,
                        "Lobby callback failed for player {PlayerId}.",
                        player.PlayerId);
                }
                catch (ObjectDisposedException ex)
                {
                    _callbacks.RemoveCallback(player.PlayerId, callback);
                    _logger.LogWarning(
                        ex,
                        "Lobby callback failed for player {PlayerId}.",
                        player.PlayerId);
                }
            }
        }
    }
}
