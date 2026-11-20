using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.ViewModels;
using DominoAllFives.Client.WPF.ViewModels.Base;
using MatchService;
using NUnit.Framework;

namespace DominoAllFives.Client.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public sealed class MatchViewModelTests
    {
        [OneTimeSetUp]
        public void StartDispatcher()
        {
            WpfTestHost.EnsureStarted();
        }

        [TestCase(20, 1, "+20", "")]
        [TestCase(40, 2, "+40", "×2")]
        [TestCase(0, 2, "", "")]
        public void Callbacks_PlayScore_ShowsOnlyAwardAndApplicableMultiplier(int points,
            int multiplier, string pointsText, string multiplierText)
        {
            // Arrange
            Client client = new Client();
            using (MatchViewModel model = CreateModel(client))
            {
                MatchView view = View(1);
                view.LastPlay = new PlayScore
                {
                    PlayerId = 2,
                    RoundNumber = 1,
                    Tile = new DominoTile
                    {
                        FirstValue = 4,
                        SecondValue = 5,
                        SpecialPower = SpecialPower.DoublePoints
                    },
                    EndValues = new List<int>
                    {
                        12,
                        5,
                        3
                    },
                    BlockedEnds = new List<BoardEnd>(),
                    Total = 20,
                    BasePoints = 20,
                    Multiplier = multiplier,
                    Points = points
                };
                // Act
                client.Emit(view, Hand(1));
                Pump();
                // Assert
                Assert.That(model.AwardedPointsText, Is.EqualTo(pointsText));
                Assert.That(model.ScoreMultiplierText, Is.EqualTo(multiplierText));
                Assert.That(model.HasScoredPlay, Is.EqualTo(points > 0));
            }
        }

        [Test]
        public void Callbacks_OtherMatchView_DoesNotPoisonSnapshotVersions()
        {
            // Arrange
            Client client = new Client();
            using (MatchViewModel model = CreateModel(client))
            {
                MatchView foreign = View(99);
                foreign.MatchId = 2;
                // Act
                client.Emit(foreign, Hand(1));
                client.Emit(View(1), Hand(1));
                Pump();
                // Assert
                Assert.That(model.Hand.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void Callbacks_DisposedModel_DiscardsQueuedSnapshot()
        {
            // Arrange
            Client client = new Client();
            MatchViewModel model = CreateModel(client);
            // Act
            client.Emit(View(1), Hand(1));
            model.Dispose();
            Pump();
            // Assert
            Assert.That(model.Hand, Is.Empty);
        }

        [Test]
        public void Callbacks_PlayerSummaries_ShowNamesGoalAndOpponentCounts()
        {
            // Arrange
            Client client = new Client();
            using (MatchViewModel model = CreateModel(client))
            {
                MatchView view = View(1);
                view.Scores.Add(new PlayerScore { PlayerId = 1, Points = 55, TileCount = 1 });
                view.Scores.Add(new PlayerScore { PlayerId = 2, Points = 35, TileCount = 5 });
                // Act
                client.Emit(view, Hand(1));
                Pump();
                // Assert
                Assert.That(model.TargetPoints, Is.EqualTo(150));
                Assert.That(model.Players[0].Username, Is.EqualTo("Alice"));
                Assert.That(model.Opponents.Count, Is.EqualTo(1));
                Assert.That(model.Opponents[0].Username, Is.EqualTo("Bob"));
                Assert.That(model.Opponents[0].TileCount, Is.EqualTo(5));
            }
        }

        [Test]
        public void Callbacks_DifferentVersions_WaitsForCoherentSnapshot()
        {
            // Arrange
            Client client = new Client();
            using (MatchViewModel model = CreateModel(client))
            {
                // Act
                client.Emit(View(2), Hand(1));
                Pump();
                // Assert
                Assert.That(model.Hand, Is.Empty);
                client.Emit(View(2), Hand(2));
                Pump();
                Assert.That(model.Hand.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void Callbacks_OlderRevision_DoesNotReplaceNewerHand()
        {
            // Arrange
            Client client = new Client();
            using (MatchViewModel model = CreateModel(client))
            {
                client.Emit(View(3), Hand(3));
                Pump();
                PlayerHand older = Hand(2);
                older.Tiles.Clear();
                // Act
                client.Emit(View(2), older);
                Pump();
                // Assert
                Assert.That(model.Hand.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void Callbacks_OtherPlayersHand_IsIgnored()
        {
            // Arrange
            Client client = new Client();
            using (MatchViewModel model = CreateModel(client))
            {
                PlayerHand foreign = Hand(1);
                foreign.PlayerId = 2;
                // Act
                client.Emit(View(1), foreign);
                Pump();
                // Assert
                Assert.That(model.Hand, Is.Empty);
            }
        }

        [TestCase(1, MatchStatus.InProgress, true)]
        [TestCase(2, MatchStatus.InProgress, false)]
        [TestCase(1, MatchStatus.Completed, false)]
        [TestCase(1, MatchStatus.Cancelled, false)]
        public void PlayCommand_TurnAndPhase_EnforcesLocalAvailability(int currentPlayer,
            MatchStatus phase, bool expected)
        {
            // Arrange
            Client client = new Client();
            using (MatchViewModel model = CreateModel(client))
            {
                MatchView view = View(1);
                view.CurrentPlayerId = currentPlayer;
                view.Phase = phase;
                client.Emit(view, Hand(1));
                Pump();
                // Act
                model.SelectedTile = model.Hand[0];
                // Assert
                Assert.That(model.PlayCommand.CanExecute(null), Is.EqualTo(expected));
                Assert.That(model.DrawCommand.CanExecute(null), Is.False);
                Assert.That(model.PassCommand.CanExecute(null), Is.False);
            }
        }

        [Test]
        public void Callbacks_EmptyBoard_AnnouncesOpeningPlayer()
        {
            Client client = new Client();
            using (MatchViewModel model = CreateModel(client))
            {
                MatchView view = View(1);
                view.CurrentPlayerId = 2;
                client.Emit(view, Hand(1));
                Pump();

                string template = DominoAllFives.Client.WPF.Localization.Resources.ResourceManager
                    .GetString("Match_lblOpener");
                Assert.That(model.TurnText, Is.EqualTo(string.Format(template, "Bob")));
            }
        }

        [Test]
        public void SelectTile_OneLegalEnd_AutoSelectsAndIgnoresBlockedAndIncompatibleEnds()
        {
            Client client = new Client();
            using (MatchViewModel model = CreateModel(client))
            {
                client.Emit(StartedView(
                    new PublicEnd { End = BoardEnd.Left, Value = 2, IsBlocked = true },
                    new PublicEnd { End = BoardEnd.Right, Value = 3 },
                    new PublicEnd { End = BoardEnd.Top, Value = 6 }), Hand(1));
                Pump();
                model.SelectedTile = model.Hand[0];

                Assert.That(model.Ends.Select(end => end.End), Is.EqualTo(new[] { BoardEnd.Right }));
                Assert.That(model.SelectedEnd, Is.EqualTo(BoardEnd.Right));
                Assert.That(model.PlayCommand.CanExecute(null), Is.True);
            }
        }

        [Test]
        public void SelectTile_MultipleLegalEnds_RequiresChoiceAndClearsItForUnplayableTile()
        {
            Client client = new Client();
            using (MatchViewModel model = CreateModel(client))
            {
                MatchView view = StartedView(
                    new PublicEnd { End = BoardEnd.Left, Value = 2 },
                    new PublicEnd { End = BoardEnd.Right, Value = 3 });
                view.StockCount = 5;
                PlayerHand hand = Hand(1);
                hand.Tiles.Add(new DominoTile { TileId = 2, FirstValue = 4, SecondValue = 5 });
                client.Emit(view, hand);
                Pump();
                model.SelectedTile = model.Hand[0];

                Assert.That(model.Ends.Select(end => end.End),
                    Is.EqualTo(new[] { BoardEnd.Left, BoardEnd.Right }));
                Assert.That(model.SelectedEnd, Is.Null);
                Assert.That(model.PlayCommand.CanExecute(null), Is.False);
                model.SelectedEnd = BoardEnd.Right;
                Assert.That(model.PlayCommand.CanExecute(null), Is.True);

                model.SelectedTile = model.Hand[1];
                Assert.That(model.Ends, Is.Empty);
                Assert.That(model.SelectedEnd, Is.Null);
                Assert.That(model.PlayCommand.CanExecute(null), Is.False);
                Assert.That(model.DrawCommand.CanExecute(null), Is.False);
                Assert.That(model.PassCommand.CanExecute(null), Is.False);
            }
        }

        [TestCase(5, true)]
        [TestCase(0, false)]
        public void SelectTile_NoLegalEnds_AllowsDrawOrPassBasedOnStock(int stockCount, bool canDraw)
        {
            Client client = new Client();
            using (MatchViewModel model = CreateModel(client))
            {
                MatchView view = StartedView(
                    new PublicEnd { End = BoardEnd.Left, Value = 2, IsBlocked = true },
                    new PublicEnd { End = BoardEnd.Right, Value = 6 });
                view.StockCount = stockCount;
                client.Emit(view, Hand(1));
                Pump();
                model.SelectedTile = model.Hand[0];

                Assert.That(model.Ends, Is.Empty);
                Assert.That(model.SelectedEnd, Is.Null);
                Assert.That(model.PlayCommand.CanExecute(null), Is.False);
                Assert.That(model.DrawCommand.CanExecute(null), Is.EqualTo(canDraw));
                Assert.That(model.PassCommand.CanExecute(null), Is.EqualTo(!canDraw));
            }
        }

        [TestCase(false, "Match_msgDrew")]
        [TestCase(true, "Match_msgPassed")]
        public void Callbacks_TurnAction_ShowsActorOnlyForTheMatchingVersion(bool isPass, string key)
        {
            Client client = new Client();
            using (MatchViewModel model = CreateModel(client))
            {
                TurnActionNotice action = new TurnActionNotice
                {
                    PlayerId = 2, IsPass = isPass, Version = 1
                };
                MatchView view = View(1);
                view.LastTurnAction = action;
                client.Emit(view, Hand(1));
                Pump();
                Assert.That(model.TurnActionText, Is.EqualTo(string.Format(ResourceText(key), "Bob")));

                MatchView later = View(2);
                later.LastTurnAction = action;
                client.Emit(later, Hand(2));
                Pump();
                Assert.That(model.TurnActionText, Is.Empty);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Callbacks_ClosedRound_PreservesBoardThenShowsNewestState(bool winsMatch)
        {
            Client client = new Client();
            using (MatchViewModel model = CreateModel(client))
            {
                MatchView initial = View(1);
                initial.RoundNumber = 1;
                client.Emit(initial, Hand(1));
                Pump();

                MatchView closed = View(2);
                closed.RoundNumber = winsMatch ? 1 : 2;
                closed.LastRound = ClosedRound(2, winsMatch ? (int?)2 : null);
                if (winsMatch)
                {
                    closed.Phase = MatchStatus.Completed;
                    closed.Result = FinalResult(2, MatchStatus.Completed);
                }

                client.Emit(closed, Hand(2));
                Pump();
                Assert.That(model.IsRoundSummaryVisible, Is.True);
                Assert.That(model.BoardTiles.Count, Is.EqualTo(1));
                Assert.That(model.RoundPlayers[0].Tiles[0].TileId, Is.EqualTo(1));
                Assert.That(model.RoundSummaryText, Is.EqualTo(winsMatch
                    ? string.Format(ResourceText("Match_msgRoundAward"), "Bob", 10)
                    : ResourceText("Match_msgRoundTied")));
                Assert.That(model.IsMatchResultVisible, Is.False);
                Assert.That(model.PlayCommand.CanExecute(null), Is.False);
                Assert.That(model.DrawCommand.CanExecute(null), Is.False);
                Assert.That(model.PassCommand.CanExecute(null), Is.False);

                if (!winsMatch)
                {
                    MatchView newer = View(3);
                    newer.RoundNumber = 2;
                    newer.LastRound = closed.LastRound;
                    newer.LastPlay = new PlayScore { RoundNumber = 1, Points = 20 };
                    PlayerHand newerHand = Hand(3);
                    newerHand.Tiles[0].TileId = 8;
                    client.Emit(newer, newerHand);
                    Pump();
                    Assert.That(model.Hand[0].TileId, Is.EqualTo(1));
                }

                Assert.That(SpinWait.SpinUntil(() =>
                {
                    Pump();
                    return !model.IsRoundSummaryVisible;
                }, TimeSpan.FromSeconds(8)), Is.True);
                Assert.That(model.IsMatchResultVisible, Is.EqualTo(winsMatch));
                Assert.That(model.BoardTiles, Is.Empty);
                if (winsMatch)
                {
                    Assert.That(model.WinnerNames, Is.EqualTo("Bob"));
                }
                else
                {
                    Assert.That(model.Hand[0].TileId, Is.EqualTo(8));
                    Assert.That(model.HasScoredPlay, Is.False);
                    client.Emit(closed, Hand(2));
                    Pump();
                    Assert.That(model.IsRoundSummaryVisible, Is.False);
                }
            }
        }

        [TestCase(MatchStatus.Completed, true)]
        [TestCase(MatchStatus.Cancelled, false)]
        public void Callbacks_TerminalSnapshot_ShowsResultWithoutReplayingEarlierRound(
            MatchStatus phase, bool hasWinner)
        {
            Client client = new Client();
            using (MatchViewModel model = CreateModel(client))
            {
                MatchView view = View(3);
                view.RoundNumber = 2;
                view.Phase = phase;
                view.LastRound = ClosedRound(2, 2);
                view.Result = FinalResult(3, phase);
                client.Emit(view, Hand(3));
                Pump();

                Assert.That(model.IsRoundSummaryVisible, Is.False);
                Assert.That(model.IsMatchResultVisible, Is.True);
                Assert.That(model.HasWinner, Is.EqualTo(hasWinner));
                Assert.That(model.IsMatchCancelled, Is.EqualTo(!hasWinner));
                Assert.That(model.Players[0].Points, Is.EqualTo(150));
            }
        }

        private static RoundResult ClosedRound(long version, int? winnerId)
        {
            return new RoundResult
            {
                RoundNumber = 1,
                Version = version,
                WinnerId = winnerId,
                AwardedPoints = winnerId.HasValue ? 10 : 0,
                ClosedBoard = StartedView().Board,
                Scores = new List<PlayerScore> { new PlayerScore { PlayerId = 1, Points = 35 } },
                RevealedHands = new List<PlayerHand> { Hand(version) }
            };
        }

        private static MatchResult FinalResult(long version, MatchStatus phase)
        {
            return new MatchResult
            {
                Version = version,
                Phase = phase,
                WinnerIds = phase == MatchStatus.Completed ? new List<int> { 2 } : new List<int>(),
                Scores = new List<PlayerScore> { new PlayerScore { PlayerId = 2, Points = 150 } }
            };
        }

        private static string ResourceText(string key)
        {
            return DominoAllFives.Client.WPF.Localization.Resources.ResourceManager.GetString(key);
        }

        private static MatchViewModel CreateModel(Client client)
        {
            PlayerSession session = new PlayerSession();
            session.Start(1, "Alice", "es-MX", "test-session");
            Dictionary<int, string> playerNames = new Dictionary<int, string>
            {
                { 1, "Alice" },
                { 2, "Bob" }
            };
            MatchPlayerDirectory players = new MatchPlayerDirectory(
                playerNames,
                new Dictionary<int, ImageSource>());
            MatchNavigation context = new MatchNavigation(1, 150, session, players);
            return new MatchViewModel(new Navigation(), context, client);
        }

        private static MatchView View(long version)
        {
            return new MatchView
            {
                MatchId = 1,
                Version = version,
                RoundNumber = 1,
                CurrentPlayerId = 1,
                OpeningTileId = 1,
                Board = new PublicBoard
                {
                    Tiles = new List<PlacedTile>(),
                    Ends = new List<PublicEnd>()
                },
                Scores = new List<PlayerScore>(),
                Phase = MatchStatus.InProgress
            };
        }

        private static MatchView StartedView(params PublicEnd[] ends)
        {
            MatchView view = View(1);
            view.Board.Tiles.Add(new PlacedTile { Tile = new DominoTile
                { TileId = 3, FirstValue = 2, SecondValue = 6 } });
            view.Board.Ends.AddRange(ends);
            return view;
        }

        private static PlayerHand Hand(long version)
        {
            return new PlayerHand
            {
                MatchId = 1,
                PlayerId = 1,
                Version = version,
                Tiles = new List<DominoTile>
                {
                    new DominoTile
                    {
                        TileId = 1,
                        FirstValue = 2,
                        SecondValue = 3
                    }
                }
            };
        }

        private static void Pump()
        {
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(
                NoOperation));
        }

        private sealed class Client : IMatchClient
        {
            public event Action<MatchView> MatchChanged;
            public event Action<PlayerHand> HandChanged;
            public event Action ConnectionLost
            {
                add { }
                remove { }
            }
            public void Emit(MatchView view, PlayerHand hand)
            {
                MatchChanged?.Invoke(view);
                HandChanged?.Invoke(hand);
            }

            public MatchView Open(string key, int matchId)
            {
                throw new NotSupportedException();
            }

            public MatchView GetMatch(int matchId)
            {
                throw new NotSupportedException();
            }

            public PlayerHand GetHand(int matchId)
            {
                throw new NotSupportedException();
            }

            public void Play(int matchId, DominoTile tile, BoardEnd end, long version)
            {
                throw new NotSupportedException();
            }

            public void Draw(int matchId, long version)
            {
                throw new NotSupportedException();
            }

            public void Pass(int matchId, long version)
            {
                throw new NotSupportedException();
            }

            public void Leave(int matchId)
            {
                throw new NotSupportedException();
            }

            public void Dispose()
            {
            }
        }

        private sealed class Navigation : IFrameNavigationService
        {
            public bool CanGoBack
            {
                get
                {
                    return false;
                }
            }

            public void NavigateTo<T>()
                where T : ViewModelBase
            {
            }

            public void NavigateTo<T>(object parameter)
                where T : ViewModelBase
            {
            }

            public void NavigateAsRoot<T>()
                where T : ViewModelBase
            {
            }

            public void GoBackToRefresh<T>()
                where T : ViewModelBase
            {
            }

            public void GoBack()
            {
            }
        }

        private static void NoOperation()
        {
        }
    }
}
