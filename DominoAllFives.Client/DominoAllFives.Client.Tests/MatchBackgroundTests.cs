using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Windows.Media;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.ViewModels;
using MatchService;
using NUnit.Framework;

namespace DominoAllFives.Client.Tests
{
    [TestFixture, NonParallelizable]
    public sealed class MatchBackgroundTests
    {
        private const int CompletionTimeoutMilliseconds = 5000;
        [OneTimeSetUp]
        public void StartDispatcher()
        {
            WpfTestHost.EnsureStarted();
        }

        [Test]
        public void Initialize_PendingCall_KeepsUiResponsiveAndPreventsOverlappingRequests()
        {
            // Arrange
            MatchWorkerClient client = new MatchWorkerClient
            {
                BlockedOperation = MatchWorkerOperation.Open
            };
            MatchViewModel model = CreateModel(client);
            int uiThreadId = WpfTestHost.Invoke(() => Thread.CurrentThread.ManagedThreadId);
            try
            {
                // Act
                WpfTestHost.Invoke(model.Initialize);
                WaitFor(client.WorkEntered);
                bool isStarting = WpfTestHost.Invoke(() => model.IsStarting);
                WpfTestHost.Invoke(model.Initialize);
                WpfTestHost.Invoke(() => model.ReconnectCommand.Execute(null));
                bool canReconnect = WpfTestHost.Invoke(() => model.ReconnectCommand.CanExecute(null));
                client.ReleaseWork.Set();
                WaitUntilReady(model);
                // Assert
                Assert.That(canReconnect, Is.False);
                Assert.That(isStarting, Is.True);
                Assert.That(client.OpenCalls, Is.EqualTo(1));
                Assert.That(client.WorkerThreadId, Is.Not.EqualTo(uiThreadId));
                Assert.That(WpfTestHost.Invoke(() => model.Hand.Count), Is.EqualTo(1));
                Assert.That(WpfTestHost.Invoke(() => model.IsStarting), Is.False);
            }
            finally
            {
                client.ReleaseWork.Set();
                DisposeModel(model, client);
            }
        }

        [TestCase(typeof(TimeoutException))]
        [TestCase(typeof(CommunicationException))]
        [TestCase(typeof(InvalidOperationException))]
        public void Initialize_FailedCall_ReportsErrorAndAllowsRetry(Type errorType)
        {
            // Arrange
            MatchWorkerClient client = new MatchWorkerClient
            {
                OpenError = (Exception)Activator.CreateInstance(errorType)
            };
            MatchViewModel model = CreateModel(client);
            try
            {
                // Act
                WpfTestHost.Invoke(model.Initialize);
                WaitUntilReady(model);
                bool showAlert = errorType != typeof(InvalidOperationException);
                string failure = WpfTestHost.Invoke(() => showAlert ? model.ConnectionMessage : model.Status);
                Assert.That(WpfTestHost.Invoke(() => model.ShowConnectionAlert), Is.EqualTo(showAlert));
                client.OpenError = null;
                if (showAlert)
                {
                    WpfTestHost.Invoke(() => model.ReconnectCommand.Execute(null));
                }
                else
                {
                    WpfTestHost.Invoke(model.Initialize);
                }
                WaitUntilReady(model);
                // Assert
                Assert.That(failure, Is.Not.Null.And.Not.Empty);
                Assert.That(client.OpenCalls, Is.EqualTo(2));
                Assert.That(WpfTestHost.Invoke(() => model.Hand.Count), Is.EqualTo(1));
                Assert.That(WpfTestHost.Invoke(() => model.Status), Is.Empty);
            }
            finally
            {
                DisposeModel(model, client);
            }
        }

        [Test]
        public void Reconnect_ChannelLoss_ConfirmsSameVersionAndPreventsDoubleClick()
        {
            MatchWorkerClient client = new MatchWorkerClient();
            MatchViewModel model = CreateModel(client);
            try
            {
                InitializeModel(model);
                WpfTestHost.Invoke(() => model.SelectedTile = model.Hand[0]);
                Assert.That(WpfTestHost.Invoke(() => model.PlayCommand.CanExecute(null)), Is.True);
                Assert.That(WpfTestHost.Invoke(() => model.ReconnectCommand.CanExecute(null)), Is.False);

                client.Disconnect();
                WaitUntil(() => WpfTestHost.Invoke(() => model.ShowConnectionAlert));
                Assert.That(WpfTestHost.Invoke(() => model.Hand.Count), Is.EqualTo(1));
                Assert.That(WpfTestHost.Invoke(() => model.PlayCommand.CanExecute(null)), Is.False);
                Assert.That(WpfTestHost.Invoke(() => model.DrawCommand.CanExecute(null)), Is.False);
                Assert.That(WpfTestHost.Invoke(() => model.PassCommand.CanExecute(null)), Is.False);
                string alert = WpfTestHost.Invoke(() => model.ConnectionMessage);
                client.BlockedOperation = MatchWorkerOperation.GetMatch;
                WpfTestHost.Invoke(() => model.ReconnectCommand.Execute(null));
                WaitFor(client.WorkEntered);
                Assert.That(WpfTestHost.Invoke(() => model.ConnectionMessage), Is.Not.EqualTo(alert));
                Assert.That(WpfTestHost.Invoke(() => model.ReconnectCommand.CanExecute(null)), Is.False);
                WpfTestHost.Invoke(() => model.ReconnectCommand.Execute(null));

                client.ReleaseWork.Set();
                WaitUntilReady(model);
                Assert.That(client.OpenCalls, Is.EqualTo(2));
                Assert.That(WpfTestHost.Invoke(() => model.ShowConnectionAlert), Is.False);
                Assert.That(WpfTestHost.Invoke(() => model.PlayCommand.CanExecute(null)), Is.True);
            }
            finally
            {
                client.ReleaseWork.Set();
                DisposeModel(model, client);
            }
        }

        [Test]
        public void Reconnect_MismatchedVersions_KeepsAlertUntilFreshMatchingPair()
        {
            MatchWorkerClient client = new MatchWorkerClient
            {
                View = MatchWorkerClient.CreateView(2)
            };
            MatchViewModel model = CreateModel(client);
            try
            {
                InitializeModel(model);
                Assert.That(client.GetMatchCalls, Is.EqualTo(8));

                WpfTestHost.Invoke(() => model.ReconnectCommand.Execute(null));
                WaitUntilReady(model);
                Assert.That(WpfTestHost.Invoke(() => model.ShowConnectionAlert), Is.True);
                Assert.That(WpfTestHost.Invoke(() => model.Hand), Is.Empty);
                Assert.That(WpfTestHost.Invoke(() => model.ReconnectCommand.CanExecute(null)), Is.True);

                client.Hand = MatchWorkerClient.CreateHand(2);
                WpfTestHost.Invoke(() => model.ReconnectCommand.Execute(null));
                WaitUntilReady(model);
                Assert.That(WpfTestHost.Invoke(() => model.ShowConnectionAlert), Is.False);
                Assert.That(WpfTestHost.Invoke(() => model.Hand.Count), Is.EqualTo(1));
            }
            finally
            {
                DisposeModel(model, client);
            }
        }

        [Test]
        public void Reconnect_AfterLostPlayReply_RecoversFinalResultWithoutReplaying()
        {
            MatchWorkerClient client = new MatchWorkerClient();
            MatchViewModel model = CreateModel(client);
            try
            {
                InitializeModel(model);
                client.PlayError = new TimeoutException("The play response was lost.");
                WpfTestHost.Invoke(() => model.SelectedTile = model.Hand[0]);
                WpfTestHost.Invoke(() => model.PlayCommand.Execute(null));
                WaitUntilReady(model);
                Assert.That(WpfTestHost.Invoke(() => model.ShowConnectionAlert), Is.True);

                MatchView terminal = MatchWorkerClient.CreateView(2);
                terminal.Phase = MatchStatus.Completed;
                terminal.CurrentPlayerId = null;
                terminal.Result = new MatchResult
                {
                    Phase = terminal.Phase,
                    Version = terminal.Version,
                    Scores = terminal.Scores,
                    WinnerIds = new List<int> { 1 }
                };
                client.View = terminal;
                client.Hand = MatchWorkerClient.CreateHand(2);
                client.Hand.Tiles.Clear();
                WpfTestHost.Invoke(() => model.ReconnectCommand.Execute(null));
                WaitUntilReady(model);

                Assert.That(client.PlayCalls, Is.EqualTo(1));
                Assert.That(WpfTestHost.Invoke(() => model.IsMatchResultVisible), Is.True);
                Assert.That(WpfTestHost.Invoke(() => model.ShowConnectionAlert), Is.False);
                Assert.That(WpfTestHost.Invoke(() => model.Hand), Is.Empty);
            }
            finally
            {
                DisposeModel(model, client);
            }
        }

        [Test]
        public void Reconnect_DuringRoundSummary_DoesNotReplayTheSameClosure()
        {
            MatchWorkerClient client = new MatchWorkerClient();
            MatchViewModel model = CreateModel(client);
            try
            {
                InitializeModel(model);
                MatchView nextRound = MatchWorkerClient.CreateView(2);
                nextRound.RoundNumber = 2;
                nextRound.LastRound = new RoundResult
                {
                    Version = 2,
                    RoundNumber = 1,
                    Scores = new List<PlayerScore>(),
                    RevealedHands = new List<PlayerHand>(),
                    ClosedBoard = client.View.Board
                };
                client.View = nextRound;
                client.Hand = MatchWorkerClient.CreateHand(2);
                client.Hand.Tiles[0].TileId = 8;
                client.Emit(nextRound, client.Hand);
                WaitUntil(() => WpfTestHost.Invoke(() => model.IsRoundSummaryVisible));

                client.Disconnect();
                WaitUntil(() => WpfTestHost.Invoke(() => model.ShowConnectionAlert));
                WpfTestHost.Invoke(() => model.ReconnectCommand.Execute(null));
                WaitUntilReady(model);
                WaitUntil(() => WpfTestHost.Invoke(() => model.Hand[0].TileId == 8));

                Assert.That(WpfTestHost.Invoke(() => model.ShowConnectionAlert), Is.False);
                Assert.That(WpfTestHost.Invoke(() => model.IsRoundSummaryVisible), Is.False);
                client.Emit(nextRound, client.Hand);
                Assert.That(WpfTestHost.Invoke(() => model.IsRoundSummaryVisible), Is.False);
            }
            finally
            {
                DisposeModel(model, client);
            }
        }

        [Test]
        public void Play_CallbackDuringPendingCall_PreservesCapturedTileEndAndVersion()
        {
            // Arrange
            MatchWorkerClient client = new MatchWorkerClient
            {
                BlockedOperation = MatchWorkerOperation.Play
            };
            MatchViewModel model = CreateModel(client);
            try
            {
                InitializeModel(model);
                DominoTile original = WpfTestHost.Invoke(() => model.Hand[0]);
                WpfTestHost.Invoke(() => model.SelectedTile = original);
                // Act
                WpfTestHost.Invoke(() => model.PlayCommand.Execute(null));
                WaitFor(client.WorkEntered);
                original.TileId = 99;
                PlayerHand newer = MatchWorkerClient.CreateHand(2);
                newer.Tiles.Clear();
                client.Emit(MatchWorkerClient.CreateView(2), newer);
                WaitUntil(() => WpfTestHost.Invoke(() => model.Hand.Count == 0));
                WpfTestHost.Invoke(() => model.SelectedEnd = BoardEnd.Right);
                WpfTestHost.Invoke(() => model.PlayCommand.Execute(null));
                client.ReleaseWork.Set();
                WaitUntilReady(model);
                // Assert
                Assert.That(client.PlayCalls, Is.EqualTo(1));
                Assert.That(client.PlayedTile.TileId, Is.EqualTo(1));
                Assert.That(client.PlayedTile, Is.Not.SameAs(original));
                Assert.That(client.PlayedEnd, Is.EqualTo(BoardEnd.Left));
                Assert.That(client.PlayedVersion, Is.EqualTo(1));
            }
            finally
            {
                client.ReleaseWork.Set();
                DisposeModel(model, client);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Play_StaleVersion_QueriesWithoutReplayingOrRepeatingFailedRecovery(
            bool recoveryFails)
        {
            // Arrange
            MatchWorkerClient client = new MatchWorkerClient();
            MatchViewModel model = CreateModel(client);
            try
            {
                InitializeModel(model);
                int previousQueries = client.GetMatchCalls;
                client.PlayError = new FaultException<MatchIssue>(MatchIssue.StaleVersion);
                if (recoveryFails)
                {
                    client.QueryError = new FaultException<MatchIssue>(MatchIssue.StaleVersion);
                }

                client.View = MatchWorkerClient.CreateView(2);
                client.Hand = MatchWorkerClient.CreateHand(2);
                client.Hand.Tiles.Clear();
                WpfTestHost.Invoke(() => model.SelectedTile = model.Hand[0]);
                // Act
                WpfTestHost.Invoke(() => model.PlayCommand.Execute(null));
                WaitUntilReady(model);
                // Assert
                Assert.That(client.PlayCalls, Is.EqualTo(1));
                Assert.That(client.GetMatchCalls, Is.EqualTo(previousQueries + 1));
                Assert.That(WpfTestHost.Invoke(() => model.Hand.Count), Is.EqualTo(
                    recoveryFails ? 1 : 0));
            }
            finally
            {
                DisposeModel(model, client);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Dispose_PendingSuccessOrFailure_ClosesOnceAfterWorkWithoutLeaving(bool fails)
        {
            // Arrange
            MatchWorkerClient client = new MatchWorkerClient
            {
                BlockedOperation = MatchWorkerOperation.Open,
                OpenError = fails ? new TimeoutException("Test timeout.") : null
            };
            MatchViewModel model = CreateModel(client);
            int uiThreadId = WpfTestHost.Invoke(() => Thread.CurrentThread.ManagedThreadId);
            try
            {
                WpfTestHost.Invoke(model.Initialize);
                WaitFor(client.WorkEntered);
                // Act
                WpfTestHost.Invoke(model.Dispose);
                WpfTestHost.Invoke(model.Dispose);
                bool closedTooEarly = client.DisposeEntered.IsSet;
                client.ReleaseWork.Set();
                WaitFor(client.DisposeEntered);
                // Assert
                Assert.That(closedTooEarly, Is.False);
                Assert.That(client.DisposeCalls, Is.EqualTo(1));
                Assert.That(client.LeaveCalls, Is.Zero);
                Assert.That(client.DisposeThreadId, Is.Not.EqualTo(uiThreadId));
                Assert.That(WpfTestHost.Invoke(() => model.Hand.Count), Is.Zero);
                Assert.That(WpfTestHost.Invoke(() => model.ReconnectCommand.CanExecute(null)),
                    Is.False);
            }
            finally
            {
                client.ReleaseWork.Set();
                DisposeModel(model, client);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Leave_RemoteCompletion_NavigatesOnlyAfterSuccess(bool fails)
        {
            // Arrange
            MatchWorkerClient client = new MatchWorkerClient
            {
                BlockedOperation = MatchWorkerOperation.Leave,
                LeaveError = fails ? new TimeoutException("Test timeout.") : null
            };
            MatchWorkerNavigation navigation = new MatchWorkerNavigation();
            MatchViewModel model = CreateModel(client, navigation);
            try
            {
                InitializeModel(model);
                // Act
                WpfTestHost.Invoke(() => model.LeaveCommand.Execute(null));
                WaitFor(client.WorkEntered);
                int earlyNavigations = WpfTestHost.Invoke(() => navigation.RootNavigations);
                client.ReleaseWork.Set();
                WaitUntilReady(model);
                // Assert
                Assert.That(earlyNavigations, Is.Zero);
                Assert.That(client.LeaveCalls, Is.EqualTo(1));
                Assert.That(WpfTestHost.Invoke(() => navigation.RootNavigations), Is.EqualTo(
                    fails ? 0 : 1));
            }
            finally
            {
                client.ReleaseWork.Set();
                DisposeModel(model, client);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DrawOrPass_AllowedAction_UsesBackgroundOperation(bool draw)
        {
            // Arrange
            MatchWorkerClient client = new MatchWorkerClient();
            client.View.StockCount = draw ? 1 : 0;
            client.Hand.Tiles.Clear();
            MatchViewModel model = CreateModel(client);
            try
            {
                InitializeModel(model);
                DominoAllFives.Client.WPF.Commands.RelayCommand command =
                    draw ? model.DrawCommand : model.PassCommand;
                // Act
                WpfTestHost.Invoke(() => command.Execute(null));
                WaitUntilReady(model);
                // Assert
                Assert.That(client.DrawCalls, Is.EqualTo(draw ? 1 : 0));
                Assert.That(client.PassCalls, Is.EqualTo(draw ? 0 : 1));
            }
            finally
            {
                DisposeModel(model, client);
            }
        }

        private static MatchViewModel CreateModel(MatchWorkerClient client,
            MatchWorkerNavigation navigation = null)
        {
            PlayerSession session = new PlayerSession();
            session.Start(1, "Alice", "es-MX", "test-session");
            MatchPlayerDirectory players = new MatchPlayerDirectory(
                new Dictionary<int, string>(),
                new Dictionary<int, ImageSource>());
            MatchNavigation context = new MatchNavigation(1, 0, session, players);
            return WpfTestHost.Invoke(() => new MatchViewModel(
                navigation ?? new MatchWorkerNavigation(), context, client));
        }

        private static void InitializeModel(MatchViewModel model)
        {
            WpfTestHost.Invoke(model.Initialize);
            WaitUntilReady(model);
        }

        private static void DisposeModel(MatchViewModel model, MatchWorkerClient client)
        {
            WpfTestHost.Invoke(model.Dispose);
            WaitFor(client.DisposeEntered);
        }

        private static void WaitUntilReady(MatchViewModel model)
        {
            WaitUntil(() => WpfTestHost.Invoke(() => model.LeaveCommand.CanExecute(null)));
        }

        private static void WaitUntil(Func<bool> condition)
        {
            Assert.That(SpinWait.SpinUntil(condition, CompletionTimeoutMilliseconds), Is.True,
                "The expected match continuation did not complete.");
        }

        private static void WaitFor(ManualResetEventSlim signal)
        {
            Assert.That(signal.Wait(CompletionTimeoutMilliseconds), Is.True,
                "The expected worker signal did not arrive.");
        }
    }
}
