using System;
using System.ServiceModel;
using DominoAllFives.Client.WPF.Services.Wcf.Callbacks;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using MatchService;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.Services.Wcf.Services
{
    public sealed class MatchClient : IMatchClient
    {
        private const string EndpointName = "MatchEndpoint";

        private readonly MatchCallbackHandler _callback;
        private MatchServiceClient _service;
        private EventHandler _connectionEnded;
        private bool _disposed;

        public MatchClient()
        {
            _callback = new MatchCallbackHandler(OnMatchChanged, OnPrivateHandChanged);
        }

        public event Action<MatchView> MatchChanged;
        public event Action<PlayerHand> HandChanged;
        public event Action ConnectionLost;

        public MatchView Open(string sessionKey, int matchId)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MatchClient));
            }

            ResetConnection();
            try
            {
                MatchServiceClient service = new MatchServiceClient(
                    new InstanceContext(_callback),
                    EndpointName);
                ConnectionNotification notification = new ConnectionNotification(this, service);
                _service = service;
                ICommunicationObject channel = service;
                _connectionEnded = notification.OnConnectionEnded;
                channel.Faulted += _connectionEnded;
                channel.Closed += _connectionEnded;
                return service.OpenMatchSession(sessionKey, matchId);
            }
            catch (CommunicationException)
            {
                ResetConnection();
                throw;
            }
            catch (TimeoutException)
            {
                ResetConnection();
                throw;
            }
        }

        public MatchView GetMatch(int matchId)
        {
            return GetOpenService().GetMatch(matchId);
        }

        public PlayerHand GetHand(int matchId)
        {
            return GetOpenService().GetHand(matchId);
        }

        public void Play(int matchId, DominoTile tile, BoardEnd end, long version)
        {
            GetOpenService().PlayTile(matchId, tile, end, version);
        }

        public void Draw(int matchId, long version)
        {
            GetOpenService().DrawTiles(matchId, version);
        }

        public void Pass(int matchId, long version)
        {
            GetOpenService().PassTurn(matchId, version);
        }

        public void Leave(int matchId)
        {
            GetOpenService().LeaveMatch(matchId);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            ResetConnection();
        }

        private MatchServiceClient GetOpenService()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MatchClient));
            }

            return _service ?? throw new InvalidOperationException("No active match session.");
        }

        private void OnMatchChanged(MatchView match)
        {
            MatchChanged?.Invoke(match);
        }

        private void OnPrivateHandChanged(PlayerHand hand)
        {
            HandChanged?.Invoke(hand);
        }

        private void ResetConnection()
        {
            ICommunicationObject channel = _service;
            _service = null;
            if (channel == null)
            {
                return;
            }

            channel.Faulted -= _connectionEnded;
            channel.Closed -= _connectionEnded;
            _connectionEnded = null;
            Close(channel);
        }

        private void NotifyConnectionLost(MatchServiceClient service)
        {
            if (ReferenceEquals(service, _service))
            {
                ConnectionLost?.Invoke();
            }
        }

        private static void Close(ICommunicationObject channel)
        {
            try
            {
                if (channel.State == CommunicationState.Faulted)
                {
                    channel.Abort();
                }
                else
                {
                    channel.Close();
                }
            }
            catch (CommunicationException ex)
            {
                LoggerFactoryProvider.CreateLogger<MatchClient>().LogWarning(
                    ex,
                    "The WCF channel could not be closed cleanly.");
                channel.Abort();
            }
            catch (TimeoutException ex)
            {
                LoggerFactoryProvider.CreateLogger<MatchClient>().LogWarning(
                    ex,
                    "The WCF channel could not be closed cleanly.");
                channel.Abort();
            }
        }

        private sealed class ConnectionNotification
        {
            private readonly MatchClient _owner;
            private readonly MatchServiceClient _service;

            public ConnectionNotification(MatchClient owner, MatchServiceClient service)
            {
                _owner = owner;
                _service = service;
            }

            public void OnConnectionEnded(object sender, EventArgs eventArgs)
            {
                _owner.NotifyConnectionLost(_service);
            }
        }
    }
}
