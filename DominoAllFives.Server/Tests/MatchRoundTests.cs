using System;
using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Gameplay;
using NUnit.Framework;
using GameplayContracts = DominoAllFives.Contracts.Gameplay;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class MatchRoundTests
    {
        [Test]
        public void Play_NextRound_PreservesPlayersAndPreviousBoard()
        {
            // Arrange
            Match match = new Match(1, new[] { 1, 2 }, 250, CreateDeck(2));
            Advance(match, 13);
            MatchPlayer first = match.GetPlayer(1);
            MatchPlayer second = match.GetPlayer(2);
            Board previousBoard = match.Board;
            int placedCount = previousBoard.Tiles.Count;
            long previousVersion = match.Version;
            // Act
            GameplayTests.PlayNextLegalAction(match);
            // Assert
            GameplayContracts.MatchView view = MatchContractMapper.CreateMatchView(match);
            Assert.That(view.Board.Tiles, Is.Empty);
            Assert.That(view.LastRound.ClosedBoard.Tiles.Count, Is.EqualTo(placedCount + 1));
            Assert.That(view.LastRound.Version, Is.EqualTo(match.Version));
            Assert.That(view.Result, Is.Null);
            Assert.That(
                new object[]
                {
                    ReferenceEquals(first, match.GetPlayer(1)),
                    ReferenceEquals(second, match.GetPlayer(2)), previousBoard.Tiles.Count,
                    first.Tiles.Count, second.Tiles.Count, match.RoundNumber, match.TurnNumber,
                    match.Version
                },
                Is.EqualTo(new object[]
                {
                    true, true, placedCount, 7, 7, 2, 0, previousVersion + 1
                }));
        }

        [TestCase(109)]
        [TestCase(3951)]
        public void Pass_NextRound_PreservesPlayersAndLastPlay(int scenario)
        {
            // Arrange
            Match match = new Match(1, new[] { 1, 2 }, 250, CreateDeck(scenario));
            AdvanceToBlockedRound(match);
            match.Pass(match.CurrentPlayerId.Value, match.Version);
            MatchPlayer first = match.GetPlayer(1);
            MatchPlayer second = match.GetPlayer(2);
            PlayScore previousPlay = match.LastPlay;
            long previousVersion = match.Version;
            // Act
            match.Pass(match.CurrentPlayerId.Value, match.Version);
            // Assert
            Assert.That(
                new object[]
                {
                    ReferenceEquals(first, match.GetPlayer(1)),
                    ReferenceEquals(second, match.GetPlayer(2)),
                    ReferenceEquals(previousPlay, match.LastPlay),
                    first.Tiles.Count, second.Tiles.Count,
                    match.RoundNumber, match.TurnNumber, match.Version
                },
                Is.EqualTo(new object[]
                {
                    true, true, true, 7, 7, 2, 0, previousVersion + 1
                }));
        }

        [Test]
        public void Pass_RoundAwardReachesTarget_CompletesWithoutReplacingHands()
        {
            // Arrange
            Match match = new Match(1, new[] { 1, 2 }, 100, CreateDeck(109));
            AdvanceToBlockedRound(match);
            match.Pass(match.CurrentPlayerId.Value, match.Version);
            match.GetPlayer(2).Points = 80;
            IReadOnlyList<Tile> previousHand = match.GetPlayer(2).Tiles;
            long previousVersion = match.Version;
            Board previousBoard = match.Board;
            int passingPlayer = match.CurrentPlayerId.Value;
            // Act
            match.Pass(match.CurrentPlayerId.Value, match.Version);
            // Assert
            GameplayContracts.MatchView view = MatchContractMapper.CreateMatchView(match);
            Assert.That(view.LastRound.ClosedBoard.Tiles.Count, Is.EqualTo(previousBoard.Tiles.Count));
            Assert.That(view.LastRound.Version, Is.EqualTo(view.Result.Version));
            Assert.That(view.Result.WinnerIds, Is.EqualTo(new[] { 2 }));
            Assert.That(view.LastTurnAction.PlayerId, Is.EqualTo(passingPlayer));
            Assert.That(view.LastTurnAction.IsPass, Is.True);
            Assert.That(view.LastTurnAction.Version, Is.EqualTo(view.Version));
            Assert.That(
                new object[]
                {
                    match.GetPlayer(2).Points, match.Phase,
                    match.RoundNumber, match.StockCount, match.CurrentPlayerId,
                    ReferenceEquals(previousBoard, match.Board), match.GetPlayer(2).Tiles,
                    match.Version
                },
                Is.EqualTo(new object[]
                {
                    100, MatchState.Completed, 1, 0, null, true, previousHand, previousVersion + 1
                }));
        }

        [Test]
        public void CreateView_ChangedRoundContract_PreservesAuthoritativeRound()
        {
            // Arrange
            Match match = new Match(1, new[] { 1, 2 }, 250, CreateDeck(2));
            Advance(match, 14);
            DominoAllFives.Contracts.Gameplay.MatchView view =
                MatchContractMapper.CreateMatchView(match);
            DominoAllFives.Contracts.Gameplay.PlayerHand revealedHand = null;
            foreach (DominoAllFives.Contracts.Gameplay.PlayerHand hand
                in view.LastRound.RevealedHands)
            {
                if (hand.PlayerId == 2)
                {
                    revealedHand = hand;
                }
            }

            int previousCount = match.LastRound.RemainingHands[2].Count;
            int previousValue = match.LastRound.RemainingHands[2][0].FirstValue;
            // Act
            revealedHand.Tiles[0].FirstValue = 99;
            revealedHand.Tiles.Clear();
            view.LastRound.Scores.Clear();
            view.LastRound.ClosedBoard.Tiles.Clear();
            // Assert
            Assert.That(MatchContractMapper.CreateMatchView(match).LastRound.ClosedBoard.Tiles,
                Is.Not.Empty);
            Assert.That(
                new object[]
                {
                    match.LastRound.Scores.Count,
                    match.LastRound.Scores[1],
                    match.LastRound.RemainingHands[2].Count,
                    match.LastRound.RemainingHands[2][0].FirstValue
                },
                Is.EqualTo(new object[] { 2, 100, previousCount, previousValue }));
        }

        [TestCase(0, 12, 1, 0)]
        [TestCase(1, 19, 20, 20)]
        [TestCase(2, 13, 53, 55)]
        [TestCase(6, 12, 7, 5)]
        [TestCase(10, 18, 4, 5)]
        public void Play_LastTile_RoundsRemainingPipsAndStartsNextRound(
            int scenario,
            int precedingActions,
            int remainingPips,
            int awardedPoints)
        {
            // Arrange
            List<Tile> deck = CreateDeck(scenario);
            Match match = new Match(1, new[] { 1, 2 }, 250, deck);
            Advance(match, precedingActions);
            int winnerId = match.CurrentPlayerId.Value;
            int previousPoints = match.GetPlayer(winnerId).Points;
            long previousVersion = match.Version;
            // Act
            GameplayTests.PlayNextLegalAction(match);
            // Assert
            Assert.That(
                new object[]
                {
                    match.LastRound.WinnerId,
                    CountRemainingPips(match.LastRound),
                    match.LastRound.AwardedPoints,
                    match.LastRound.Scores[winnerId],
                    match.LastRound.RemainingHands[winnerId].Count,
                    match.LastPlay.RoundNumber,
                    match.LastPlay.RoundPoints,
                    match.RoundNumber,
                    match.TurnNumber,
                    match.Version,
                    match.Board.Tiles.Count,
                    match.StockCount
                },
                Is.EqualTo(new object[]
                {
                    winnerId, remainingPips, awardedPoints, previousPoints + awardedPoints,
                    0, 1, awardedPoints, 2, 0, previousVersion + 1, 0, 14
                }));
        }

        [TestCase(109, 2, 20)]
        [TestCase(3951, null, 0)]
        public void Pass_BlockedRound_ResolvesWinnerOrTie(
            int scenario,
            int? winnerId,
            int awardedPoints)
        {
            // Arrange
            List<Tile> deck = CreateDeck(scenario);
            Match match = new Match(1, new[] { 1, 2 }, 250, deck);
            AdvanceToBlockedRound(match);
            match.Pass(match.CurrentPlayerId.Value, match.Version);
            long previousVersion = match.Version;
            int secondScore = 15;
            if (winnerId.HasValue)
            {
                secondScore = 50;
            }
            // Act
            match.Pass(match.CurrentPlayerId.Value, match.Version);
            // Assert
            Assert.That(
                new object[]
                {
                    match.LastRound.WinnerId,
                    match.LastRound.AwardedPoints,
                    match.LastRound.Scores[1],
                    match.LastRound.Scores[2],
                    match.LastRound.RemainingHands[1].Count > 0,
                    match.LastRound.RemainingHands[2].Count > 0,
                    match.RoundNumber,
                    match.Version
                },
                Is.EqualTo(new object[]
                {
                    winnerId, awardedPoints, 30, secondScore,
                    true, true, 2, previousVersion + 1
                }));
        }

        [Test]
        public void Play_RoundAwardReachesTarget_CompletesWithoutDealingAgain()
        {
            // Arrange
            List<Tile> deck = CreateDeck(2);
            Match match = new Match(1, new[] { 1, 2 }, 100, deck);
            Advance(match, 13);
            // Act
            GameplayTests.PlayNextLegalAction(match);
            // Assert
            Assert.That(
                new object[]
                {
                    match.GetPlayer(1).Points,
                    match.LastRound.AwardedPoints,
                    match.LastPlay.RoundPoints,
                    match.Phase,
                    match.CurrentPlayerId,
                    match.RoundNumber,
                    match.Version
                },
                Is.EqualTo(new object[] { 100, 55, 55, MatchState.Completed, null, 1, 14L }));
        }

        [TestCase("Draw", 21, MatchIssue.DrawNotAllowed)]
        [TestCase("Pass", 22, MatchIssue.PassNotAllowed)]
        public void Operation_EmptyStockWithIncompatibleAction_PreservesTurn(
            string operation,
            int precedingActions,
            MatchIssue issue)
        {
            // Arrange
            List<Tile> deck = CreateDeck(109);
            Match match = new Match(1, new[] { 1, 2 }, 250, deck);
            Advance(match, precedingActions);
            int playerId = match.CurrentPlayerId.Value;
            int handCount = match.GetPlayer(playerId).Tiles.Count;
            long version = match.Version;
            MatchRuleException failure = null;
            // Act
            try
            {
                if (operation == "Draw")
                {
                    match.Draw(playerId, version);
                }
                else
                {
                    match.Pass(playerId, version);
                }
            }
            catch (MatchRuleException ex)
            {
                failure = ex;
            }
            // Assert
            Assert.That(
                new object[]
                {
                    failure?.Issue,
                    match.StockCount,
                    match.CurrentPlayerId,
                    match.Version,
                    match.GetPlayer(playerId).Tiles.Count
                },
                Is.EqualTo(new object[] { issue, 0, playerId, version, handCount }));
        }

        [Test]
        public void Play_TargetReachedOnLastTile_CompletesBeforeRoundResolution()
        {
            // Arrange
            List<Tile> deck = CreateDeck(9);
            Match match = new Match(1, new[] { 1, 2 }, 100, deck);
            Advance(match, 14);
            // Represents points carried from earlier rounds without replaying those rounds.
            match.GetPlayer(1).Points = 85;
            // Act
            GameplayTests.PlayNextLegalAction(match);
            // Assert
            GameplayContracts.MatchView view = MatchContractMapper.CreateMatchView(match);
            Assert.That(view.Result.WinnerIds, Is.EqualTo(new[] { 1 }));
            Assert.That(view.Result.Version, Is.EqualTo(view.Version));
            Assert.That(view.LastRound, Is.Null);
            Assert.That(
                new object[]
                {
                    match.GetPlayer(1).Points,
                    match.GetPlayer(1).Tiles.Count,
                    match.LastPlay.Points,
                    match.LastPlay.RoundPoints,
                    match.LastRound,
                    match.Phase,
                    match.RoundNumber,
                    match.Version
                },
                Is.EqualTo(new object[] { 100, 0, 15, 0, null, MatchState.Completed, 1, 15L }));
        }

        [Test]
        public void Draw_StockAction_ProjectsActorAndVersion()
        {
            Match match = new Match(1, new[] { 1, 2 }, 250, CreateDeck(109));
            MatchPlayer player = match.GetPlayer(match.CurrentPlayerId.Value);
            while (match.Board.HasMove(player.Tiles, match.TurnNumber))
            {
                Advance(match, 1);
                player = match.GetPlayer(match.CurrentPlayerId.Value);
            }

            long previousVersion = match.Version;
            match.Draw(player.PlayerId, previousVersion);

            GameplayContracts.MatchView view = MatchContractMapper.CreateMatchView(match);
            Assert.That(view.LastTurnAction.PlayerId, Is.EqualTo(player.PlayerId));
            Assert.That(view.LastTurnAction.IsPass, Is.False);
            Assert.That(view.LastTurnAction.Version, Is.EqualTo(previousVersion + 1));
            Assert.That(view.CurrentPlayerId, Is.EqualTo(player.PlayerId));
        }

        private static void Advance(Match match, int actionCount)
        {
            for (int action = 0; action < actionCount; action++)
            {
                if (match.LastRound != null || match.Phase != MatchState.InProgress)
                {
                    throw new InvalidOperationException("The scenario ended before its boundary.");
                }

                GameplayTests.PlayNextLegalAction(match);
            }
        }

        private static void AdvanceToBlockedRound(Match match)
        {
            const int MaximumActions = 100;
            for (int action = 0; action < MaximumActions; action++)
            {
                bool anyMove = false;
                foreach (MatchPlayer player in match.Players)
                {
                    if (match.Board.HasMove(player.Tiles, match.TurnNumber))
                    {
                        anyMove = true;
                    }
                }

                if (match.StockCount == 0 && !anyMove)
                {
                    return;
                }

                Advance(match, 1);
            }

            throw new InvalidOperationException("The scenario did not reach a blocked round.");
        }

        private static int CountRemainingPips(RoundSummary round)
        {
            int total = 0;
            foreach (IReadOnlyList<Tile> hand in round.RemainingHands.Values)
            {
                foreach (Tile tile in hand)
                {
                    total += tile.TotalPips;
                }
            }

            return total;
        }

        private static List<Tile> CreateDeck(int scenario)
        {
            int[] values;
            switch (scenario)
            {
                case 0:
                    values = new[]
                    {
                        45, 3, 1, 12, 56, 23, 26, 66, 2, 6, 15, 24, 0, 46,
                        25, 14, 11, 33, 5, 36, 13, 55, 16, 4, 22, 34, 44, 35
                    };
                    break;
                case 1:
                    values = new[]
                    {
                        35, 25, 45, 15, 1, 33, 26, 22, 14, 36, 66, 3, 12, 44,
                        23, 5, 4, 0, 55, 56, 46, 11, 13, 24, 34, 16, 2, 6
                    };
                    break;
                case 2:
                    values = new[]
                    {
                        35, 66, 6, 34, 16, 1, 56, 15, 24, 23, 45, 12, 3, 5,
                        44, 25, 33, 22, 0, 55, 13, 26, 11, 2, 46, 4, 14, 36
                    };
                    break;
                case 6:
                    values = new[]
                    {
                        6, 56, 3, 1, 44, 0, 45, 15, 5, 34, 11, 4, 14, 25,
                        12, 26, 2, 16, 66, 23, 13, 22, 33, 36, 35, 55, 24, 46
                    };
                    break;
                case 9:
                    values = new[]
                    {
                        5, 34, 4, 23, 14, 24, 56, 0, 33, 44, 45, 11, 26, 3,
                        2, 22, 12, 25, 46, 55, 36, 35, 13, 6, 66, 1, 16, 15
                    };
                    break;
                case 10:
                    values = new[]
                    {
                        45, 11, 16, 5, 44, 25, 23, 24, 1, 66, 2, 0, 3, 22,
                        14, 36, 55, 15, 33, 4, 13, 12, 6, 46, 26, 34, 35, 56
                    };
                    break;
                case 109:
                    values = new[]
                    {
                        24, 5, 22, 16, 0, 11, 15, 23, 4, 26, 6, 44, 1, 3,
                        55, 56, 34, 45, 12, 13, 2, 25, 66, 14, 35, 46, 36, 33
                    };
                    break;
                case 3951:
                    values = new[]
                    {
                        66, 5, 26, 33, 22, 4, 15, 13, 25, 14, 34, 12, 6, 44,
                        23, 35, 1, 2, 46, 24, 55, 56, 11, 0, 36, 16, 3, 45
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scenario));
            }

            return MatchTestDeck.Ordered(values);
        }
    }
}
