using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Gameplay;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Gameplay;
using DominoAllFives.Contracts.Services;
using NUnit.Framework;

namespace DominoAllFives.Server.AdditionalTest
{
    [TestFixture]
    [NonParallelizable]
    public sealed class MatchPublicationWcfTests
    {
        private static int nextPlayerId = 87000;

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void LeaveMatch_DeferredPublication_DeliversOrderedFinishAndReleasesLobby(
            int scenario)
        {
            // Arrange
            int firstPlayerId = Interlocked.Add(ref nextPlayerId, 2);
            int secondPlayerId = firstPlayerId + 1;
            Lobby lobby = CreateStartedLobby(firstPlayerId, secondPlayerId);
            string[] keys =
            {
                SessionManager.CreateSession(firstPlayerId),
                SessionManager.CreateSession(secondPlayerId)
            };
            Receiver[] receivers =
            {
                new Receiver(lobby.LobbyCode),
                new Receiver(lobby.LobbyCode)
            };
            DuplexChannelFactory<IMatchService>[] factories =
                new DuplexChannelFactory<IMatchService>[2];
            IMatchService[] clients = new IMatchService[2];
            using (ServiceHost host = CreateHost())
            {
                try
                {
                    host.Open();
                    for (int index = 0; index < clients.Length; index++)
                    {
                        factories[index] = new DuplexChannelFactory<IMatchService>(
                            new InstanceContext(receivers[index]),
                            CreateBinding(),
                            host.Description.Endpoints[0].Address);
                        clients[index] = factories[index].CreateChannel();
                    }

                    MatchView initial = clients[0].OpenMatchSession(keys[0], lobby.LobbyId);
                    clients[1].OpenMatchSession(keys[1], lobby.LobbyId);
                    if (scenario == 1)
                    {
                        ((IClientChannel)clients[1]).Abort();
                        factories[1].Abort();
                    }

                    Move move = new Move
                    {
                        TileId = initial.OpeningTileId,
                        End = BoardEndPosition.Left,
                        ExpectedVersion = initial.Version
                    };
                    MatchManager.Instance.PlayTile(
                        lobby.LobbyId,
                        initial.CurrentPlayerId.Value,
                        move);
                    if (scenario == 2)
                    {
                        MatchManager.Instance.LeaveMatch(lobby.LobbyId, firstPlayerId);
                    }
                    // Act
                    clients[0].LeaveMatch(lobby.LobbyId);
                    bool received = receivers[0].Finished.Wait(5000);
                    string[] expectedEvents =
                    {
                        "view:1", "hand:1", "view:2", "hand:2", "finished:2"
                    };
                    List<string[]> events = new List<string[]>
                    {
                        receivers[0].Events.ToArray()
                    };
                    List<string[]> expected = new List<string[]> { expectedEvents };
                    bool released = receivers[0].LobbyWasReleased;
                    bool privateHands = HandsBelongTo(receivers[0], firstPlayerId);
                    if (scenario != 1)
                    {
                        received = receivers[1].Finished.Wait(5000) && received;
                        events.Add(receivers[1].Events.ToArray());
                        expected.Add(expectedEvents);
                        released = released && receivers[1].LobbyWasReleased;
                        privateHands = privateHands && HandsBelongTo(receivers[1], secondPlayerId);
                    }

                    MatchView after = MatchManager.Instance.GetMatchView(
                        lobby.LobbyId,
                        firstPlayerId);
                    // Assert
                    Assert.That(
                        new object[] { received, events, released, privateHands, after.Version },
                        Is.EqualTo(new object[] { true, expected, true, true, 2L }));
                }
                finally
                {
                    AbortConnections(clients, factories);
                    ReleaseMatch(keys, lobby.LobbyId);
                    foreach (Receiver receiver in receivers)
                    {
                        receiver.Finished.Dispose();
                    }
                }
            }
        }

        private static void AbortConnections(
            IMatchService[] clients,
            DuplexChannelFactory<IMatchService>[] factories)
        {
            foreach (IMatchService client in clients)
            {
                if (client != null)
                {
                    ((IClientChannel)client).Abort();
                }
            }

            foreach (DuplexChannelFactory<IMatchService> factory in factories)
            {
                if (factory != null)
                {
                    factory.Abort();
                }
            }
        }

        private static void ReleaseMatch(string[] keys, int matchId)
        {
            foreach (string key in keys)
            {
                SessionManager.RemoveSession(key);
            }

            LobbyManager.Instance.ReleaseMatch(matchId);
            MatchManager.Instance.RemoveMatch(matchId);
        }

        private static Lobby CreateStartedLobby(int firstPlayerId, int secondPlayerId)
        {
            LobbySettings settings = new LobbySettings
            {
                MaxPlayers = 2,
                TargetPoints = 100
            };
            Lobby lobby = LobbyManager.Instance.CreateLobby(CreatePlayer(firstPlayerId), settings);
            LobbyManager.Instance.JoinPlayer(CreatePlayer(secondPlayerId), lobby.LobbyCode);
            LobbyManager.Instance.PrepareLobbyForMatch(firstPlayerId);
            MatchManager.Instance.CreateMatch(
                lobby.LobbyId,
                new[] { firstPlayerId, secondPlayerId },
                100);
            return LobbyManager.Instance.ConfirmMatchStarted(firstPlayerId);
        }

        private static Player CreatePlayer(int playerId)
        {
            return new Player
            {
                PlayerId = playerId,
                Username = "Player" + playerId,
                Picture = ProfilePicture.FromStoredContent(new byte[0]),
                Presence = PlayerPresence.Online
            };
        }

        private static ServiceHost CreateHost()
        {
            ServiceHost host = new ServiceHost(typeof(MatchService));
            string address = "net.pipe://localhost/DominoAllFives/PublicationTest/"
                + Guid.NewGuid().ToString("N");
            host.AddServiceEndpoint(typeof(IMatchService), CreateBinding(), address);
            return host;
        }

        private static NetNamedPipeBinding CreateBinding()
        {
            return new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
            {
                SendTimeout = TimeSpan.FromSeconds(2),
                OpenTimeout = TimeSpan.FromSeconds(2)
            };
        }

        private static bool HandsBelongTo(Receiver receiver, int playerId)
        {
            if (receiver.Hands.Count != 2)
            {
                return false;
            }

            foreach (PlayerHand hand in receiver.Hands)
            {
                if (hand.PlayerId != playerId)
                {
                    return false;
                }
            }

            return true;
        }

        [CallbackBehavior(
            UseSynchronizationContext = false,
            ConcurrencyMode = ConcurrencyMode.Single)]
        private sealed class Receiver : IMatchCallback
        {
            private readonly string _lobbyCode;

            public ConcurrentQueue<string> Events { get; } = new ConcurrentQueue<string>();
            public ConcurrentQueue<PlayerHand> Hands { get; } = new ConcurrentQueue<PlayerHand>();
            public ManualResetEventSlim Finished { get; } = new ManualResetEventSlim();
            public bool LobbyWasReleased { get; private set; }

            public Receiver(string lobbyCode)
            {
                _lobbyCode = lobbyCode;
            }

            public void OnMatchChanged(MatchView match)
            {
                Events.Enqueue("view:" + match.Version);
            }

            public void OnPrivateHandChanged(PlayerHand hand)
            {
                Hands.Enqueue(hand);
                Events.Enqueue("hand:" + hand.Version);
            }

            public void OnRoundFinished(RoundResult result)
            {
                Events.Enqueue("round:" + result.RoundNumber);
            }

            public void OnMatchFinished(MatchResult result)
            {
                Events.Enqueue("finished:" + result.Version);
                try
                {
                    LobbyManager.Instance.GetLobby(_lobbyCode);
                }
                catch (LobbyRuleException ex)
                {
                    LobbyWasReleased = ex.Issue == LobbyIssue.NotFound;
                }

                Finished.Set();
            }
        }
    }
}
