using System;
using System.Collections.Generic;
using System.ServiceModel;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Services;
using Microsoft.Extensions.Logging;
using LobbyModel = DominoAllFives.Contracts.Domain.Lobby;

namespace DominoAllFives.Server.Infrastructure.Lobby
{
    internal sealed class LobbyEventPublisher
    {
        private enum LobbyCallbackEvent
        {
            LobbyChanged,
            LobbyClosed,
            MatchStarted
        }

        private readonly LobbyCallbackRegistry _callbacks;
        private readonly ILogger<LobbyEventPublisher> _logger;

        public LobbyEventPublisher(
            LobbyCallbackRegistry callbacks,
            ILogger<LobbyEventPublisher> logger)
        {
            _callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void PublishLobbyChanged(LobbyModel lobby)
        {
            Publish(lobby, LobbyCallbackEvent.LobbyChanged, 0);
        }

        public void PublishLobbyClosed(LobbyModel lobby)
        {
            Publish(lobby, LobbyCallbackEvent.LobbyClosed, 0);
        }

        public void PublishMatchStarted(LobbyModel lobby, int matchId)
        {
            Publish(lobby, LobbyCallbackEvent.MatchStarted, matchId);
        }

        private void Publish(LobbyModel lobby, LobbyCallbackEvent callbackEvent, int matchId)
        {
            if (lobby == null)
            {
                throw new ArgumentNullException(nameof(lobby));
            }

            foreach (Player player in lobby.Participants)
            {
                ILobbyCallback callback;
                if (!_callbacks.TryGetCallback(player.PlayerId, out callback))
                {
                    continue;
                }

                try
                {
                    NotifyCallback(callback, callbackEvent, lobby, matchId);
                }
                catch (CommunicationException ex)
                {
                    RemoveFailedCallback(player.PlayerId, callback, ex);
                }
                catch (TimeoutException ex)
                {
                    RemoveFailedCallback(player.PlayerId, callback, ex);
                }
                catch (ObjectDisposedException ex)
                {
                    RemoveFailedCallback(player.PlayerId, callback, ex);
                }
            }
        }

        private static void NotifyCallback(
            ILobbyCallback callback,
            LobbyCallbackEvent callbackEvent,
            LobbyModel lobby,
            int matchId)
        {
            switch (callbackEvent)
            {
                case LobbyCallbackEvent.LobbyChanged:
                    callback.OnLobbyChanged(lobby);
                    break;
                case LobbyCallbackEvent.LobbyClosed:
                    callback.OnLobbyClosed();
                    break;
                case LobbyCallbackEvent.MatchStarted:
                    callback.OnMatchStarted(matchId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(callbackEvent));
            }
        }

        private void RemoveFailedCallback(
            int playerId,
            ILobbyCallback callback,
            Exception ex)
        {
            _callbacks.RemoveCallback(playerId, callback);
            _logger.LogDebug(
                ex,
                "Lobby callback failed for player {PlayerId}.",
                playerId);
        }
    }
}
