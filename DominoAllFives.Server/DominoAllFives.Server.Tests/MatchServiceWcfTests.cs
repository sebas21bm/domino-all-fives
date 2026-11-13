using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Threading;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Gameplay;
using DominoAllFives.Contracts.Services;
using DominoAllFives.Server.Infrastructure;
using NUnit.Framework;
using Proxy = global::MatchService;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class MatchServiceWcfTests
    {
        private static int nextMatchId = 20000;
        [Test]
        public void PlayTile_GeneratedProxy_PreservesWireContractsAndPrivateCallbacks()
        {
            // Arrange
            const int FirstPlayerId = 83001;
            const int SecondPlayerId = 83002;
            const int CallbackTimeoutMilliseconds = 5000;
            int matchId = Interlocked.Increment(ref nextMatchId);
            GameRuntime.Controller.Create(matchId, new[] { FirstPlayerId, SecondPlayerId }, 100);
            string[] keys =
            {
                SessionManager.CreateSession(FirstPlayerId),
                SessionManager.CreateSession(SecondPlayerId)
            };
            MatchProxyCallback[] callbacks =
            {
                new MatchProxyCallback(),
                new MatchProxyCallback()
            };
            using (ServiceHost host = CreateHost())
            {
                host.Open();
                Proxy.MatchServiceClient[] clients = callbacks.Select(callback
                    => new Proxy.MatchServiceClient(new InstanceContext(callback),
                    new NetTcpBinding(SecurityMode.None), host.Description.Endpoints[0].Address))
                    .ToArray();
                try
                {
                    Proxy.MatchView view = clients[0].OpenMatchSession(keys[0], matchId);
                    clients[1].OpenMatchSession(keys[1], matchId);
                    int activeIndex = view.CurrentPlayerId == FirstPlayerId ? 0 : 1;
                    Proxy.DominoTile tile = clients[activeIndex].GetHand(matchId).Tiles.Single(
                        candidate => candidate.TileId == view.OpeningTileId);
                    int originalValue = tile.FirstValue;
                    tile.FirstValue = 99;
                    // Act
                    Proxy.MatchView after = clients[activeIndex].PlayTile(matchId, tile,
                        Proxy.BoardEnd.Left, view.Version);
                    bool received = SpinWait.SpinUntil(() => callbacks.All(callback
                        => callback.Hands.Count > 0), CallbackTimeoutMilliseconds);
                    // Assert
                    Assert.That(typeof(Proxy.DominoTile).Assembly, Is.Not.EqualTo(typeof(
                        DominoTile).Assembly));
                    Assert.That(after.Board.Tiles[0].Tile.FirstValue, Is.EqualTo(originalValue));
                    Assert.That(after.Version, Is.EqualTo(view.Version + 1));
                    Assert.That(received, Is.True);
                    Assert.That(callbacks[0].Hands.All(hand => hand.PlayerId == FirstPlayerId),
                        Is.True);
                    Assert.That(callbacks[1].Hands.All(hand => hand.PlayerId == SecondPlayerId),
                        Is.True);
                    FaultException<Proxy.MatchIssue> stale =
                        Assert.Throws<FaultException<Proxy.MatchIssue>>(()
                        => clients[activeIndex].PlayTile(matchId, tile, Proxy.BoardEnd.Left,
                        view.Version));
                    Assert.That(stale.Detail, Is.EqualTo(Proxy.MatchIssue.StaleVersion));
                    SessionManager.RemoveSession(keys[activeIndex]);
                    Assert.Throws<FaultException<Proxy.AccessIssue>>(()
                        => clients[activeIndex].GetMatch(matchId));
                }
                finally
                {
                    foreach (Proxy.MatchServiceClient client in clients)
                    {
                        client.Abort();
                    }

                    foreach (string key in keys)
                    {
                        SessionManager.RemoveSession(key);
                    }
                }
            }
        }

        [Test]
        public void PlayTile_NetTcp_IgnoresForgedValuesAndPublishesOnlyOwnHand()
        {
            // Arrange
            int matchId = Interlocked.Increment(ref nextMatchId);
            GameRuntime.Controller.Create(matchId, new[] { 81001, 81002 }, 100);
            string[] keys =
            {
                SessionManager.CreateSession(81001),
                SessionManager.CreateSession(81002)
            };
            Callback[] callbacks =
            {
                new Callback(),
                new Callback()
            };
            using (ServiceHost host = CreateHost())
            {
                host.Open();
                DuplexChannelFactory<IMatchService>[] factories = callbacks.Select(callback
                    => new DuplexChannelFactory<IMatchService>(new InstanceContext(callback),
                    new NetTcpBinding(SecurityMode.None), host.Description.Endpoints[0].Address))
                    .ToArray();
                IMatchService[] channels = factories.Select(factory => factory.CreateChannel())
                    .ToArray();
                try
                {
                    MatchView view = channels[0].OpenMatchSession(keys[0], matchId);
                    channels[1].OpenMatchSession(keys[1], matchId);
                    int activeIndex = view.CurrentPlayerId == 81001 ? 0 : 1;
                    DominoTile owned = channels[activeIndex].GetHand(matchId).Tiles.Single(tile
                        => tile.TileId == view.OpeningTileId);
                    DominoTile forged = new DominoTile
                    {
                        TileId = owned.TileId,
                        FirstValue = 99,
                        SecondValue = 99,
                        SpecialPower = SpecialPower.DoublePoints
                    };
                    // Act
                    MatchView after = channels[activeIndex].PlayTile(matchId, forged,
                        BoardEnd.Left, view.Version);
                    bool received = SpinWait.SpinUntil(() => callbacks.All(callback
                        => callback.Hands.Count > 0), 5000);
                    // Assert
                    Assert.That(after.Board.Tiles[0].Tile.FirstValue, Is.EqualTo(owned.FirstValue));
                    Assert.That(after.Board.Tiles[0].Tile.SpecialPower, Is.EqualTo(
                        owned.SpecialPower));
                    Assert.That(received, Is.True);
                    Assert.That(callbacks[0].Hands.All(hand => hand.PlayerId == 81001), Is.True);
                    Assert.That(callbacks[1].Hands.All(hand => hand.PlayerId == 81002), Is.True);
                    FaultException<MatchIssue> stale =
                        Assert.Throws<FaultException<MatchIssue>>(()
                        => channels[activeIndex].PlayTile(matchId, forged, BoardEnd.Left,
                        view.Version));
                    Assert.That(stale.Detail, Is.EqualTo(MatchIssue.StaleVersion));
                    SessionManager.RemoveSession(keys[activeIndex]);
                    Assert.Throws<FaultException<AccessIssue>>(()
                        => channels[activeIndex].GetMatch(matchId));
                }
                finally
                {
                    foreach (IMatchService channel in channels)
                    {
                        ((ICommunicationObject)channel).Abort();
                    }

                    foreach (DuplexChannelFactory<IMatchService> factory in factories)
                    {
                        factory.Abort();
                    }

                    foreach (string key in keys)
                    {
                        SessionManager.RemoveSession(key);
                    }
                }
            }
        }

        [Test]
        public void OpenMatchSession_NonParticipant_CannotReadMatch()
        {
            // Arrange
            int matchId = Interlocked.Increment(ref nextMatchId);
            GameRuntime.Controller.Create(matchId, new[] { 82001, 82002 }, 100);
            string key = SessionManager.CreateSession(82003);
            using (ServiceHost host = CreateHost())
            {
                host.Open();
                var factory = new DuplexChannelFactory<IMatchService>(new InstanceContext(
                    new Callback()), new NetTcpBinding(SecurityMode.None),
                    host.Description.Endpoints[0].Address);
                IMatchService channel = factory.CreateChannel();
                try
                {
                    // Act
                    FaultException<MatchIssue> ex = Assert.Throws<FaultException<MatchIssue>>((
                        ) => channel.OpenMatchSession(key, matchId));
                    // Assert
                    Assert.That(ex.Detail, Is.EqualTo(MatchIssue.PlayerNotInMatch));
                }
                finally
                {
                    ((ICommunicationObject)channel).Abort();
                    factory.Abort();
                    SessionManager.RemoveSession(key);
                }
            }
        }

        private static ServiceHost CreateHost()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            ServiceHost host = new ServiceHost(typeof(MatchService));
            host.AddServiceEndpoint(typeof(IMatchService), new NetTcpBinding(SecurityMode.None),
                "net.tcp://localhost:" + port + "/MatchTest");
            return host;
        }

        [CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode =
            ConcurrencyMode.Multiple)]
        private sealed class Callback : IMatchCallback
        {
            public ConcurrentQueue<PlayerHand> Hands { get; } = new ConcurrentQueue<PlayerHand>();

            public void OnMatchChanged(MatchView match)
            {
            }

            public void OnPrivateHandChanged(PlayerHand hand)
            {
                Hands.Enqueue(hand);
            }

            public void OnRoundFinished(RoundResult result)
            {
            }

            public void OnMatchFinished(MatchResult result)
            {
            }
        }
    }
}
