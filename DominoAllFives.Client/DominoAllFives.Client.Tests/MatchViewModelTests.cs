using System;
using System.Collections.Generic;
using System.Threading;
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

        [TestCase(1, MatchPhase.InProgress, true)]
        [TestCase(2, MatchPhase.InProgress, false)]
        [TestCase(1, MatchPhase.Completed, false)]
        [TestCase(1, MatchPhase.Cancelled, false)]
        public void PlayCommand_TurnAndPhase_EnforcesLocalAvailability(int currentPlayer,
            MatchPhase phase, bool expected)
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

        private static MatchViewModel CreateModel(Client client)
        {
            PlayerSession session = new PlayerSession();
            session.Start(1, "Alice", "es-MX", "test-session");
            return new MatchViewModel(new Navigation(), new MatchNavigation { MatchId = 1,
                PlayerSession = session, TargetPoints = 150, PlayerNames = new Dictionary<int,
                string> { { 1, "Alice" }, { 2, "Bob" } } }, client);
        }

        private static MatchView View(long version)
        {
            return new MatchView
            {
                MatchId = 1,
                Version = version,
                CurrentPlayerId = 1,
                OpeningTileId = 1,
                Board = new PublicBoard
                {
                    Tiles = new List<PlacedTile>(),
                    Ends = new List<PublicEnd>()
                },
                Scores = new List<PlayerScore>(),
                Phase = MatchPhase.InProgress
            };
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
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
            }));
        }

        private sealed class Client : IMatchClient
        {
            public event Action<MatchView> MatchChanged;
            public event Action<PlayerHand> HandChanged;
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

            public MatchView Play(int matchId, DominoTile tile, BoardEnd end, long version)
            {
                throw new NotSupportedException();
            }

            public MatchView Draw(int matchId, long version)
            {
                throw new NotSupportedException();
            }

            public MatchView Pass(int matchId, long version)
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
    }
}
