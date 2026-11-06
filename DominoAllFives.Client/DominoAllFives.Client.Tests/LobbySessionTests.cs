using System;
using System.Collections.Generic;
using System.ServiceModel;
using DominoAllFives.Client.WPF.Services.Wcf;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFivesService;
using NUnit.Framework;

namespace DominoAllFives.Client.Tests
{
    [TestFixture]
    public class LobbySessionTests
    {
        [Test]
        public void Create_OpenFailure_ReleasesClientAndAllowsAnotherAttempt()
        {
            FakeLobbyClient failed = new FakeLobbyClient { OpenError = new CommunicationException() };
            FakeLobbyClient next = new FakeLobbyClient();
            int count = 0;
            using (LobbySession session = new LobbySession(() => count++ == 0 ? failed : next))
            {
                Assert.Throws<CommunicationException>(() => session.Create(new LobbySettings()));
                Assert.That(failed.Disposed, Is.True);
                Assert.That(session.Client, Is.Null);
                session.Create(new LobbySettings());
                Assert.That(session.Client, Is.SameAs(next));
            }
        }

        [Test]
        public void Join_BusinessRejection_ReleasesProxyAndLeavesNoActiveSession()
        {
            FakeLobbyClient client = new FakeLobbyClient { JoinError = new FaultException("Lobby not found.") };
            using (LobbySession session = new LobbySession(() => client))
            {
                Assert.Throws<FaultException>(() => session.Join("MISSING"));
                Assert.That(client.Disposed, Is.True);
                Assert.That(session.IsActive, Is.False);
            }
        }

        [Test]
        public void Create_CallbackBeforeResponse_PreservesLatestSnapshotAcrossNavigation()
        {
            FakeLobbyClient client = new FakeLobbyClient { ChangeBeforeResponse = true };
            using (LobbySession session = new LobbySession(() => client))
            {
                Lobby lobby = session.Create(new LobbySettings());
                Assert.That(lobby.Participants.Count, Is.EqualTo(2));
                Assert.That(session.WasCreatedByThisClient, Is.True);
                Assert.That(client.OpenCount, Is.EqualTo(1));
                Assert.That(session.Client, Is.SameAs(client));
            }
        }

        [Test]
        public void Join_StartAndCloseBeforeViewSubscribes_CachesNotifications()
        {
            FakeLobbyClient client = new FakeLobbyClient();
            using (LobbySession session = new LobbySession(() => client))
            {
                session.Join("ROOM");
                client.EmitStart(19);
                client.EmitClose();
                Assert.That(session.ProvisionalMatchId, Is.EqualTo(19));
                Assert.That(session.WasClosed, Is.True);
                Assert.That(session.WasCreatedByThisClient, Is.False);
            }
        }

        [Test]
        public void Leave_RemoteFailure_StillClearsSessionAndDisposesProxy()
        {
            FakeLobbyClient client = new FakeLobbyClient { LeaveError = new TimeoutException() };
            using (LobbySession session = new LobbySession(() => client))
            {
                session.Join("ROOM");
                Assert.Throws<TimeoutException>(() => session.Leave());
                Assert.That(session.Client, Is.Null);
                Assert.That(session.CurrentLobby, Is.Null);
                Assert.That(client.Disposed, Is.True);
            }
        }

        [Test]
        public void Reset_UnsubscribesCallbacksFromPreviousParticipation()
        {
            FakeLobbyClient old = new FakeLobbyClient();
            FakeLobbyClient current = new FakeLobbyClient();
            int count = 0;
            using (LobbySession session = new LobbySession(() => count++ == 0 ? old : current))
            {
                session.Join("ROOM");
                Action<int> inFlight = old.CaptureStartHandler();
                session.Leave();
                session.Join("ROOM");
                inFlight(88);
                old.EmitClose();
                old.EmitStart(99);
                Assert.That(session.WasClosed, Is.False);
                Assert.That(session.ProvisionalMatchId, Is.Zero);
                Assert.That(session.Client, Is.SameAs(current));
            }
        }

        [Test]
        public void Dispose_LeavesActiveParticipationAndCannotReopen()
        {
            FakeLobbyClient client = new FakeLobbyClient();
            LobbySession session = new LobbySession(() => client);
            session.Join("ROOM");
            session.Dispose();
            session.Dispose();
            Assert.That(client.LeaveCount, Is.EqualTo(1));
            Assert.That(client.Disposed, Is.True);
            Assert.Throws<ObjectDisposedException>(() => session.Join("ROOM"));
        }

        private sealed class FakeLobbyClient : ILobbyClient
        {
            public event Action<Lobby> LobbyChanged;
            public event Action LobbyClosed;
            public event Action<int> MatchStarted;
            public CommunicationState State { get; private set; }
            internal Exception OpenError, JoinError, LeaveError;
            internal bool ChangeBeforeResponse, Disposed;
            internal int OpenCount, LeaveCount;
            public void Open()
            {
                OpenCount++;
                if (OpenError != null) throw OpenError;
                State = CommunicationState.Opened;
            }
            public Lobby CreateLobby(LobbySettings settings)
            {
                Lobby response = Snapshot(1);
                if (ChangeBeforeResponse) LobbyChanged?.Invoke(Snapshot(2));
                return response;
            }
            public Lobby JoinLobby(string code)
            {
                if (JoinError != null) throw JoinError;
                return Snapshot(2);
            }
            private static Lobby Snapshot(int count)
            {
                Lobby result = new Lobby { LobbyCode = "ROOM", MaximumPlayers = 2,
                    Participants = new List<LobbyParticipant>() };
                for (int i = 0; i < count; i++)
                    result.Participants.Add(new LobbyParticipant { DisplayName = "Player", IsHost = i == 0 });
                return result;
            }
            public Lobby GetLobby(string code) => Snapshot(2);
            public void LeaveLobby() { LeaveCount++; if (LeaveError != null) throw LeaveError; }
            public void CloseLobby() => LobbyClosed?.Invoke();
            public void StartMatch() => EmitStart(1);
            public void CloseConnection() { State = CommunicationState.Closed; }
            public void Dispose() { Disposed = true; CloseConnection(); }
            internal Action<int> CaptureStartHandler() => MatchStarted;
            internal void EmitStart(int id) => MatchStarted?.Invoke(id);
            internal void EmitClose() => LobbyClosed?.Invoke();
        }
    }
}
