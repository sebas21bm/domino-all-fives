using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Gameplay;
using DominoAllFives.Contracts.Services;
using NUnit.Framework;
using Proxy = global::MatchService;

namespace DominoAllFives.Server.AdditionalTest
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
            CreateMatch(
                matchId,
                new[] { FirstPlayerId, SecondPlayerId },
                100);
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
                Proxy.MatchServiceClient[] clients =
                    new Proxy.MatchServiceClient[callbacks.Length];
                for (int index = 0; index < callbacks.Length; index++)
                {
                    clients[index] = new Proxy.MatchServiceClient(
                        new InstanceContext(callbacks[index]),
                        CreateBinding(),
                        host.Description.Endpoints[0].Address);
                }
                try
                {
                    Proxy.MatchView view = clients[0].OpenMatchSession(keys[0], matchId);
                    clients[1].OpenMatchSession(keys[1], matchId);
                    int activeIndex = 1;
                    if (view.CurrentPlayerId == FirstPlayerId)
                    {
                        activeIndex = 0;
                    }
                    Proxy.DominoTile tile = clients[activeIndex].GetHand(matchId).Tiles.Single(
                        candidate => candidate.TileId == view.OpeningTileId);
                    int originalValue = tile.FirstValue;
                    tile.FirstValue = 99;
                    // Act
                    Proxy.MatchView after = clients[activeIndex].PlayTile(matchId, tile,
                        Proxy.BoardEnd.Left, view.Version);
                    bool received = WaitForProxyHands(
                        callbacks,
                        CallbackTimeoutMilliseconds);
                    // Assert
                    Assert.That(typeof(Proxy.DominoTile).Assembly, Is.Not.EqualTo(typeof(
                        DominoTile).Assembly));
                    Assert.That(after.Board.Tiles[0].Tile.FirstValue, Is.EqualTo(originalValue));
                    Assert.That(after.Version, Is.EqualTo(view.Version + 1));
                    Assert.That(after.LastPlay.PlayerId, Is.EqualTo(view.CurrentPlayerId));
                    Assert.That(after.LastPlay.Total, Is.EqualTo(after.LastPlay.EndValues.Sum()));
                    Assert.That(after.LastPlay.Points, Is.EqualTo(
                        after.LastPlay.BasePoints * after.LastPlay.Multiplier));
                    Assert.That(received, Is.True);
                    Assert.That(callbacks[0].Hands.All(hand => hand.PlayerId == FirstPlayerId),
                        Is.True);
                    Assert.That(callbacks[1].Hands.All(hand => hand.PlayerId == SecondPlayerId),
                        Is.True);
                    FaultException<Proxy.MatchIssue> stale = null;
                    try
                    {
                        clients[activeIndex].PlayTile(
                            matchId,
                            tile,
                            Proxy.BoardEnd.Left,
                            view.Version);
                    }
                    catch (FaultException<Proxy.MatchIssue> ex)
                    {
                        stale = ex;
                    }

                    Assert.That(stale.Detail, Is.EqualTo(Proxy.MatchIssue.StaleVersion));
                    SessionManager.RemoveSession(keys[activeIndex]);
                    FaultException<Proxy.AccessIssue> accessFault = null;
                    try
                    {
                        clients[activeIndex].GetMatch(matchId);
                    }
                    catch (FaultException<Proxy.AccessIssue> ex)
                    {
                        accessFault = ex;
                    }

                    Assert.That(accessFault, Is.Not.Null);
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
        public void PlayTile_NamedPipe_IgnoresForgedValuesAndPublishesOnlyOwnHand()
        {
            // Arrange
            int matchId = Interlocked.Increment(ref nextMatchId);
            CreateMatch(matchId, new[] { 81001, 81002 }, 100);
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
                DuplexChannelFactory<IMatchService>[] factories =
                    new DuplexChannelFactory<IMatchService>[callbacks.Length];
                IMatchService[] channels = new IMatchService[callbacks.Length];
                for (int index = 0; index < callbacks.Length; index++)
                {
                    factories[index] = new DuplexChannelFactory<IMatchService>(
                        new InstanceContext(callbacks[index]),
                        CreateBinding(),
                        host.Description.Endpoints[0].Address);
                    channels[index] = factories[index].CreateChannel();
                }
                try
                {
                    MatchView view = channels[0].OpenMatchSession(keys[0], matchId);
                    channels[1].OpenMatchSession(keys[1], matchId);
                    int activeIndex = 1;
                    if (view.CurrentPlayerId == 81001)
                    {
                        activeIndex = 0;
                    }
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
                    bool received = WaitForHands(callbacks, 5000);
                    // Assert
                    Assert.That(after.Board.Tiles[0].Tile.FirstValue, Is.EqualTo(owned.FirstValue));
                    Assert.That(after.Board.Tiles[0].Tile.SpecialPower, Is.EqualTo(
                        owned.SpecialPower));
                    Assert.That(received, Is.True);
                    Assert.That(callbacks[0].Hands.All(hand => hand.PlayerId == 81001), Is.True);
                    Assert.That(callbacks[1].Hands.All(hand => hand.PlayerId == 81002), Is.True);
                    FaultException<MatchIssue> stale = null;
                    try
                    {
                        channels[activeIndex].PlayTile(
                            matchId,
                            forged,
                            BoardEnd.Left,
                            view.Version);
                    }
                    catch (FaultException<MatchIssue> ex)
                    {
                        stale = ex;
                    }

                    Assert.That(stale.Detail, Is.EqualTo(MatchIssue.StaleVersion));
                    SessionManager.RemoveSession(keys[activeIndex]);
                    FaultException<AccessIssue> accessFault = null;
                    try
                    {
                        channels[activeIndex].GetMatch(matchId);
                    }
                    catch (FaultException<AccessIssue> ex)
                    {
                        accessFault = ex;
                    }

                    Assert.That(accessFault, Is.Not.Null);
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
            CreateMatch(matchId, new[] { 82001, 82002 }, 100);
            string key = SessionManager.CreateSession(82003);
            using (ServiceHost host = CreateHost())
            {
                host.Open();
                DuplexChannelFactory<IMatchService> factory =
                    new DuplexChannelFactory<IMatchService>(
                    new InstanceContext(new Callback()),
                    CreateBinding(),
                    host.Description.Endpoints[0].Address);
                IMatchService channel = factory.CreateChannel();
                try
                {
                    // Act
                    FaultException<MatchIssue> ex = null;
                    try
                    {
                        channel.OpenMatchSession(key, matchId);
                    }
                    catch (FaultException<MatchIssue> fault)
                    {
                        ex = fault;
                    }

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
            ServiceHost host = new ServiceHost(typeof(MatchService));
            string address = "net.pipe://localhost/DominoAllFives/MatchTest/"
                + Guid.NewGuid().ToString("N");
            host.AddServiceEndpoint(typeof(IMatchService), CreateBinding(), address);
            return host;
        }

        private static void CreateMatch(
            int matchId,
            IEnumerable<int> playerIds,
            int targetPoints)
        {
            MatchController matchController = new MatchController(MatchManager.Instance);
            matchController.CreateMatch(
                matchId,
                playerIds,
                targetPoints);
        }

        private static NetNamedPipeBinding CreateBinding()
        {
            return new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
        }

        private static bool WaitForProxyHands(
            IEnumerable<MatchProxyCallback> callbacks,
            int timeoutMilliseconds)
        {
            DateTime timeout = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            while (DateTime.UtcNow < timeout)
            {
                bool allCallbacksReceivedHand = true;
                foreach (MatchProxyCallback callback in callbacks)
                {
                    if (callback.Hands.Count == 0)
                    {
                        allCallbacksReceivedHand = false;
                        break;
                    }
                }

                if (allCallbacksReceivedHand)
                {
                    return true;
                }

                Thread.Sleep(10);
            }

            return false;
        }

        private static bool WaitForHands(
            IEnumerable<Callback> callbacks,
            int timeoutMilliseconds)
        {
            DateTime timeout = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            while (DateTime.UtcNow < timeout)
            {
                bool allCallbacksReceivedHand = true;
                foreach (Callback callback in callbacks)
                {
                    if (callback.Hands.Count == 0)
                    {
                        allCallbacksReceivedHand = false;
                        break;
                    }
                }

                if (allCallbacksReceivedHand)
                {
                    return true;
                }

                Thread.Sleep(10);
            }

            return false;
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
