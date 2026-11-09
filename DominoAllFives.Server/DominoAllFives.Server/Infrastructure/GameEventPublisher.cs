using System;
using System.Collections.Generic;
using System.ServiceModel;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Services;
using DominoAllFives.Server.Logging;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Server.Infrastructure
{
    internal sealed class GameEventPublisher
    {
        private readonly ILogger<GameEventPublisher> _logger;
        private readonly CallbackRegistry<ILobbyCallback> _lobbyCallbacks;

        internal GameEventPublisher(CallbackRegistry<ILobbyCallback> lobbyCallbacks, ILogger<GameEventPublisher> logger)
        {
            _lobbyCallbacks = lobbyCallbacks ??
                throw new ArgumentNullException(nameof(lobbyCallbacks));
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
        }

        internal void LobbyChanged(IEnumerable<string> recipients, Lobby lobby)
        {
            Publish(recipients, callback => callback.OnLobbyChanged(lobby));
        }

        internal void LobbyClosed(IEnumerable<string> recipients)
        {
            Publish(recipients, callback => callback.OnLobbyClosed());
        }

        internal void MatchStarted(IEnumerable<string> recipients, int matchId)
        {
            Publish(recipients, callback => callback.OnMatchStarted(matchId));
        }

        private void Publish(IEnumerable<string> recipients, Action<ILobbyCallback> send)
        {
            foreach (KeyValuePair<string, ILobbyCallback> item in
                _lobbyCallbacks.Snapshot(recipients))
            {
                try
                {
                    send(item.Value);
                }
                catch (CommunicationException ex)
                {
                    _lobbyCallbacks.Unregister(item.Key);
                    _logger.LogWarning(ex, "Lobby callback delivery failed; channel unregistered.");
                }
                catch (TimeoutException ex)
                {
                    _lobbyCallbacks.Unregister(item.Key);
                    _logger.LogWarning(ex, "Lobby callback delivery failed; channel unregistered.");
                }
                catch (ObjectDisposedException ex)
                {
                    _lobbyCallbacks.Unregister(item.Key);
                    _logger.LogWarning(ex, "Lobby callback delivery failed; channel unregistered.");
                }
            }
        }
    }
}

