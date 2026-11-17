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

        [TestCase(false)]
        [TestCase(true)]
        public void LeaveMatch_GeneratedProxy_PublishesSingleTerminalSnapshot(bool repeat)
        {
            // Arrange
            using (CommandSession session = new CommandSession(false))
            {
                // Act
                session.Client.LeaveMatch(session.MatchId);
                if (repeat)
                {
                    session.Client.LeaveMatch(session.MatchId);
                }

                bool received = SpinWait.SpinUntil(session.HasResult, 5000);
                Proxy.MatchView after = session.Client.GetMatch(session.MatchId);
                Proxy.MatchResult result;
                session.Callback.Results.TryPeek(out result);
                // Assert
                Assert.That(new object[]
                {
                    received, after.Version, after.Phase, result?.Phase,
                    session.Callback.Views.Count, session.Callback.Hands.Count,
                    session.Callback.Results.Count
                }, Is.EqualTo(new object[]
                {
                    true, 1L, Proxy.MatchStatus.Cancelled, Proxy.MatchStatus.Cancelled, 1, 1, 1
                }));
            }
        }

        [TestCase("PlayTile", "stale", Proxy.MatchIssue.StaleVersion)]
        [TestCase("DrawTiles", "stale", Proxy.MatchIssue.StaleVersion)]
        [TestCase("PassTurn", "stale", Proxy.MatchIssue.StaleVersion)]
        [TestCase("PlayTile", "turn", Proxy.MatchIssue.NotPlayersTurn)]
        [TestCase("DrawTiles", "turn", Proxy.MatchIssue.NotPlayersTurn)]
        [TestCase("PassTurn", "turn", Proxy.MatchIssue.NotPlayersTurn)]
        [TestCase("DrawTiles", "legal", Proxy.MatchIssue.DrawNotAllowed)]
        [TestCase("PassTurn", "legal", Proxy.MatchIssue.PassNotAllowed)]
        [TestCase("PlayTile", "missing", Proxy.MatchIssue.TileNotOwned)]
        public void Command_GeneratedProxy_ReturnsTypedRuleFaultWithoutMutation(
            string operation,
            string scenario,
            Proxy.MatchIssue expectedIssue)
        {
            // Arrange
            using (CommandSession session = new CommandSession(scenario == "turn"))
            {
                Proxy.MatchView request = session.Client.GetMatch(session.MatchId);
                if (scenario == "stale")
                {
                    request.Version = -1;
                }

                if (scenario == "missing")
                {
                    request.OpeningTileId = -1;
                }

                Proxy.MatchIssue? issue = null;
                // Act
                try
                {
                    InvokeCommand(session.Client, operation, request);
                }
                catch (FaultException<Proxy.MatchIssue> ex)
                {
                    issue = ex.Detail;
                }

                Proxy.MatchView after = session.Client.GetMatch(session.MatchId);
                // Assert
                Assert.That(new object[] { issue, after.Version, after.Phase }, Is.EqualTo(
                    new object[] { expectedIssue, 0L, Proxy.MatchStatus.InProgress }));
            }
        }

        [TestCase("PlayTile", false)]
        [TestCase("DrawTiles", false)]
        [TestCase("PassTurn", false)]
        [TestCase("LeaveMatch", false)]
        [TestCase("PlayTile", true)]
        [TestCase("DrawTiles", true)]
        [TestCase("PassTurn", true)]
        [TestCase("LeaveMatch", true)]
        public void Command_GeneratedProxy_ReturnsTypedAccessFault(
            string operation,
            bool revoked)
        {
            // Arrange
            using (CommandSession session = new CommandSession(false))
            {
                Proxy.MatchView request = session.Client.GetMatch(session.MatchId);
                Proxy.AccessIssue expectedIssue = Proxy.AccessIssue.PermissionDenied;
                if (revoked)
                {
                    SessionManager.RemoveSession(session.Key);
                    expectedIssue = Proxy.AccessIssue.SessionRequired;
                }
                else
                {
                    request.MatchId++;
                }

                Proxy.AccessIssue? issue = null;
                // Act
                try
                {
                    InvokeCommand(session.Client, operation, request);
                }
                catch (FaultException<Proxy.AccessIssue> ex)
                {
                    issue = ex.Detail;
                }

                MatchView after = MatchManager.Instance.GetMatchView(
                    session.MatchId,
                    session.PlayerId);
                // Assert
                Assert.That(new object[] { issue, after.Version, after.Phase }, Is.EqualTo(
                    new object[] { expectedIssue, 0L, MatchStatus.InProgress }));
            }
        }

        private static void InvokeCommand(
            Proxy.MatchServiceClient client,
            string operation,
            Proxy.MatchView request)
        {
            switch (operation)
            {
                case "PlayTile":
                    Proxy.DominoTile tile = null;
                    if (request.OpeningTileId >= 0)
                    {
                        tile = new Proxy.DominoTile { TileId = request.OpeningTileId };
                    }

                    client.PlayTile(request.MatchId, tile, Proxy.BoardEnd.Left, request.Version);
                    break;
                case "DrawTiles":
                    client.DrawTiles(request.MatchId, request.Version);
                    break;
                case "PassTurn":
                    client.PassTurn(request.MatchId, request.Version);
                    break;
                case "LeaveMatch":
                    client.LeaveMatch(request.MatchId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private sealed class CommandSession : IDisposable
        {
            private readonly ServiceHost _host;
            public int MatchId { get; }
            public int PlayerId { get; }
            public string Key { get; }
            public Proxy.MatchServiceClient Client { get; }
            public MatchProxyCallback Callback { get; } = new MatchProxyCallback();

            public CommandSession(bool otherPlayer)
            {
                MatchId = Interlocked.Increment(ref nextMatchId);
                int first = MatchId * 2;
                int second = first + 1;
                CreateMatch(MatchId, new[] { first, second }, 100);
                MatchView initial = MatchManager.Instance.GetMatchView(MatchId, first);
                PlayerId = initial.CurrentPlayerId.Value;
                if (otherPlayer)
                {
                    PlayerId = first;
                    if (initial.CurrentPlayerId == first)
                    {
                        PlayerId = second;
                    }
                }

                Key = SessionManager.CreateSession(PlayerId);
                _host = CreateHost();
                _host.Open();
                Client = new Proxy.MatchServiceClient(
                    new InstanceContext(Callback),
                    CreateBinding(),
                    _host.Description.Endpoints[0].Address);
                Client.OpenMatchSession(Key, MatchId);
            }

            public bool HasResult()
            {
                return !Callback.Results.IsEmpty
                    && !Callback.Views.IsEmpty
                    && !Callback.Hands.IsEmpty;
            }

            public void Dispose()
            {
                Client.Abort();
                _host.Abort();
                SessionManager.RemoveSession(Key);
                MatchManager.Instance.RemoveMatch(MatchId);
            }
        }

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
                    clients[activeIndex].PlayTile(matchId, tile,
                        Proxy.BoardEnd.Left, view.Version);
                    Proxy.MatchView after = clients[activeIndex].GetMatch(matchId);
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
                    Assert.That(callbacks[activeIndex].Views.Single().Version,
                        Is.EqualTo(after.Version));
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
                    channels[activeIndex].PlayTile(matchId, forged,
                        BoardEnd.Left, view.Version);
                    MatchView after = channels[activeIndex].GetMatch(matchId);
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
