using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services.Wcf.Callbacks;
using DominoAllFives.Client.WPF.Services.Wcf.Services;
using LobbyService;
using NUnit.Framework;

namespace DominoAllFives.Client.Tests
{
    [TestFixture]
    public class WcfLobbyServiceTests
    {
        [Test]
        public void Create_OpenFailure_ReleasesClientAndAllowsAnotherAttempt()
        {
            // Arrange
            TestLobbyService remoteService = new TestLobbyService
            {
                OpenFailuresRemaining = 1
            };
            using (TestLobbyHost host = new TestLobbyHost(remoteService))
            {
                using (WcfLobbyService service = host.CreateClient())
                {
                    // Act
                    Assert.Throws<FaultException>(() => service.CreateLobby(new LobbySettings()));
                    // Assert
                    Assert.That(service.State, Is.EqualTo(CommunicationState.Closed));
                    Assert.That(service.CurrentLobby, Is.Null);
                    // Act
                    service.CreateLobby(new LobbySettings());
                    // Assert
                    Assert.That(service.IsActive, Is.True);
                    Assert.That(remoteService.OpenCount, Is.EqualTo(2));
                }
            }
        }

        [Test]
        public void Join_BusinessRejection_LeavesNoActiveSession()
        {
            // Arrange
            TestLobbyService remoteService = new TestLobbyService
            {
                RejectJoin = true
            };
            using (TestLobbyHost host = new TestLobbyHost(remoteService))
            {
                using (WcfLobbyService service = host.CreateClient())
                {
                    // Act
                    Assert.Throws<FaultException>(() => service.JoinLobby("MISSING"));
                    // Assert
                    Assert.That(service.IsActive, Is.False);
                    Assert.That(service.CurrentLobby, Is.Null);
                }
            }
        }

        [Test]
        public void Create_CallbackBeforeResponse_PreservesLatestSnapshot()
        {
            // Arrange
            TestLobbyService remoteService = new TestLobbyService
            {
                ChangeBeforeResponse = true
            };
            using (TestLobbyHost host = new TestLobbyHost(remoteService))
            {
                using (WcfLobbyService service = host.CreateClient())
                {
                    // Act
                    Lobby lobby = service.CreateLobby(new LobbySettings());
                    // Assert
                    Assert.That(lobby.Participants.Count, Is.EqualTo(2));
                    Assert.That(service.WasCreatedByThisClient, Is.True);
                }
            }
        }

        [Test]
        public void Join_StartAndCloseBeforeViewSubscribes_CachesNotifications()
        {
            // Arrange
            TestLobbyService remoteService = new TestLobbyService();
            using (TestLobbyHost host = new TestLobbyHost(remoteService))
            {
                using (WcfLobbyService service = host.CreateClient())
                {
                    // Act
                    service.JoinLobby("ROOM");
                    remoteService.EmitMatchStarted(19);
                    remoteService.EmitLobbyClosed();
                    WaitUntil(() => service.WasClosed);
                    // Assert
                    Assert.That(service.ProvisionalMatchId, Is.EqualTo(19));
                    Assert.That(service.WasCreatedByThisClient, Is.False);
                }
            }
        }

        [Test]
        public void Leave_RemoteFailure_StillClearsConnection()
        {
            // Arrange
            TestLobbyService remoteService = new TestLobbyService
            {
                RejectLeave = true
            };
            using (TestLobbyHost host = new TestLobbyHost(remoteService))
            {
                using (WcfLobbyService service = host.CreateClient())
                {
                    service.JoinLobby("ROOM");
                    // Act
                    Assert.Throws<FaultException>(service.LeaveLobby);
                    // Assert
                    Assert.That(service.IsActive, Is.False);
                    Assert.That(service.CurrentLobby, Is.Null);
                    Assert.That(service.State, Is.EqualTo(CommunicationState.Closed));
                }
            }
        }

        [Test]
        public void Reset_PreviousConnectionCallbacks_IgnoresNotifications()
        {
            // Arrange
            TestLobbyService remoteService = new TestLobbyService();
            using (TestLobbyHost host = new TestLobbyHost(remoteService))
            {
                using (WcfLobbyService service = host.CreateClient())
                {
                    service.JoinLobby("ROOM");
                    LobbyCallbackHandler previousCallback = host.Callbacks[0];
                    // Act
                    service.LeaveLobby();
                    service.JoinLobby("ROOM");
                    previousCallback.OnMatchStarted(88);
                    previousCallback.OnLobbyClosed();
                    // Assert
                    Assert.That(service.ProvisionalMatchId, Is.Zero);
                    Assert.That(service.WasClosed, Is.False);
                }
            }
        }

        [Test]
        public void Dispose_ActiveParticipation_LeavesAndCannotReopen()
        {
            // Arrange
            TestLobbyService remoteService = new TestLobbyService();
            using (TestLobbyHost host = new TestLobbyHost(remoteService))
            {
                WcfLobbyService service = host.CreateClient();
                service.JoinLobby("ROOM");
                // Act
                service.Dispose();
                service.Dispose();
                // Assert
                Assert.That(remoteService.LeaveCount, Is.EqualTo(1));
                Assert.Throws<ObjectDisposedException>(() => service.JoinLobby("ROOM"));
            }
        }

        private static void WaitUntil(Func<bool> condition)
        {
            bool completed = SpinWait.SpinUntil(condition, TimeSpan.FromSeconds(2));
            Assert.That(completed, Is.True);
        }

        private sealed class TestLobbyHost : IDisposable
        {
            private readonly NetNamedPipeBinding _binding;
            private readonly EndpointAddress _endpointAddress;
            private readonly ServiceHost _host;
            public List<LobbyCallbackHandler> Callbacks { get; } = new List<LobbyCallbackHandler>();

            public TestLobbyHost(TestLobbyService service)
            {
                string address =
                    "net.pipe://localhost/DominoAllFives.Tests/" + Guid.NewGuid().ToString(
                    "N");
                _binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                _endpointAddress = new EndpointAddress(address);
                _host = new ServiceHost(service, new Uri(address));
                _host.AddServiceEndpoint(typeof(ILobbyService), _binding, string.Empty);
                _host.Open();
            }

            public WcfLobbyService CreateClient()
            {
                PlayerSession playerSession = new PlayerSession();
                playerSession.Start(1, "Player", "en-US", "session-key");
                return new WcfLobbyService(playerSession, CreateProxy);
            }

            public void Dispose()
            {
                if (_host.State == CommunicationState.Faulted)
                {
                    _host.Abort();
                }
                else
                {
                    _host.Close();
                }
            }

            private LobbyServiceClient CreateProxy(LobbyCallbackHandler callback)
            {
                Callbacks.Add(callback);
                return new LobbyServiceClient(
                    new InstanceContext(
                    callback),
                    _binding,
                    _endpointAddress);
            }
        }

        [ServiceBehavior(
            InstanceContextMode = InstanceContextMode.Single,
            ConcurrencyMode = ConcurrencyMode.Reentrant)]
        private sealed class TestLobbyService : ILobbyService
        {
            private ILobbyServiceCallback _callback;
            public int OpenFailuresRemaining { get; set; }
            public bool RejectJoin { get; set; }
            public bool RejectLeave { get; set; }
            public bool ChangeBeforeResponse { get; set; }
            public int OpenCount { get; private set; }
            public int LeaveCount { get; private set; }

            public void OpenLobbySession(string sessionKey)
            {
                OpenCount++;
                if (OpenFailuresRemaining > 0)
                {
                    OpenFailuresRemaining--;
                    throw new FaultException("Session rejected.");
                }

                _callback = OperationContext.Current.GetCallbackChannel<ILobbyServiceCallback>();
            }

            public Lobby CreateLobby(LobbySettings settings)
            {
                if (ChangeBeforeResponse)
                {
                    _callback.OnLobbyChanged(CreateSnapshot(2));
                }

                return CreateSnapshot(1);
            }

            public Lobby JoinLobby(string lobbyCode)
            {
                if (RejectJoin)
                {
                    throw new FaultException("Lobby not found.");
                }

                return CreateSnapshot(2);
            }

            public void LeaveLobby()
            {
                LeaveCount++;
                if (RejectLeave)
                {
                    throw new FaultException("Leave failed.");
                }
            }

            public Lobby GetLobby(string lobbyCode)
            {
                return CreateSnapshot(2);
            }

            public void StartMatch()
            {
                EmitMatchStarted(1);
            }

            public void CloseLobby()
            {
                EmitLobbyClosed();
            }

            public void EmitMatchStarted(int matchId)
            {
                _callback.OnMatchStarted(matchId);
            }

            public void EmitLobbyClosed()
            {
                _callback.OnLobbyClosed();
            }

            private static Lobby CreateSnapshot(int participantCount)
            {
                Lobby lobby = new Lobby
                {
                    LobbyCode = "ROOM",
                    Settings = new LobbySettings
                    {
                        MaxPlayers = 2,
                        TargetPoints = 100
                    },
                    Participants = new List<Player>()
                };
                for (int index = 0; index < participantCount; index++)
                {
                    Player player = new Player
                    {
                        PlayerId = index + 1,
                        Username = "Player"
                    };
                    lobby.Participants.Add(player);
                    if (index == 0)
                    {
                        lobby.Host = player;
                    }
                }

                return lobby;
            }
        }
    }
}
