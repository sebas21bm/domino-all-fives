using System;
using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Gameplay;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class RoundResolutionTests
    {
        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(2, 0)]
        [TestCase(3, 5)]
        [TestCase(4, 5)]
        [TestCase(5, 5)]
        [TestCase(6, 5)]
        [TestCase(7, 5)]
        [TestCase(8, 10)]
        [TestCase(9, 10)]
        [TestCase(10, 10)]
        [TestCase(53, 55)]
        public void ResolvePlayedOut_RemainingPips_ProjectsRoundedAwardWithoutMutation(
            int remainingPips,
            int awardedPoints)
        {
            // Arrange
            List<MatchPlayer> players = CreatePlayedOutPlayers(remainingPips);
            List<int> before = CapturePlayers(players);
            List<Tile> expectedHand = new List<Tile>(players[1].Tiles);
            // Act
            RoundSummary summary = RoundResolution.ResolvePlayedOut(players, 3, 1);
            // Assert
            Assert.That(
                new object[]
                {
                    summary.RoundNumber,
                    summary.WinnerId,
                    summary.AwardedPoints,
                    summary.Scores[1],
                    summary.Scores[2],
                    summary.RemainingHands[1].Count,
                    summary.RemainingHands[2],
                    CapturePlayers(players)
                },
                Is.EqualTo(new object[]
                {
                    3, 1, awardedPoints, 20 + awardedPoints, 40, 0, expectedHand, before
                }));
        }

        [TestCase(0, 2, 20)]
        [TestCase(1, 3, 15)]
        [TestCase(2, null, 0)]
        [TestCase(3, null, 0)]
        [TestCase(4, 3, 20)]
        [TestCase(5, null, 0)]
        public void ResolveBlocked_RemainingHands_SelectsWinnerOrTieWithoutMutation(
            int scenario,
            int? winnerId,
            int awardedPoints)
        {
            // Arrange
            List<MatchPlayer> players = CreateBlockedPlayers(scenario);
            List<int> before = CapturePlayers(players);
            int[] expectedScores = GetBlockedScores(scenario);
            // Act
            RoundSummary summary = RoundResolution.ResolveBlocked(players, 4);
            // Assert
            List<int> scores = new List<int>();
            foreach (MatchPlayer player in players)
            {
                scores.Add(summary.Scores[player.PlayerId]);
            }

            Assert.That(
                new object[]
                {
                    summary.RoundNumber,
                    summary.WinnerId,
                    summary.AwardedPoints,
                    scores,
                    CapturePlayers(players)
                },
                Is.EqualTo(new object[] { 4, winnerId, awardedPoints, expectedScores, before }));
        }

        [TestCase(SpecialPower.None)]
        [TestCase(SpecialPower.DoublePoints)]
        [TestCase(SpecialPower.Block)]
        public void ResolveBlocked_SpecialPower_DoesNotChangeRemainingPips(SpecialPower power)
        {
            // Arrange
            List<MatchPlayer> players = CreatePlayers(2);
            players[0].Hand.Clear();
            players[0].Hand.Add(new Tile(1, 0, 6, power));
            players[1].Hand.Clear();
            players[1].Hand.Add(new Tile(2, 0, 5, SpecialPower.None));
            // Act
            RoundSummary summary = RoundResolution.ResolveBlocked(players, 1);
            // Assert
            Assert.That(
                new object[]
                {
                    summary.WinnerId,
                    summary.AwardedPoints,
                    summary.Scores[1],
                    summary.Scores[2],
                    summary.RemainingHands[1][0].SpecialPower,
                    players[0].Points,
                    players[1].Points
                },
                Is.EqualTo(new object[] { 2, 10, 10, 30, power, 10, 20 }));
        }

        [Test]
        public void ResolvePlayedOut_SourceChanges_PreservesRoundSnapshot()
        {
            // Arrange
            List<MatchPlayer> players = CreatePlayedOutPlayers(7);
            RoundSummary summary = RoundResolution.ResolvePlayedOut(players, 5, 1);
            // Act
            players[0].Points = 500;
            players[1].Points = 800;
            players[1].Hand.Clear();
            players.Clear();
            // Assert
            Assert.That(
                new object[]
                {
                    summary.RoundNumber,
                    summary.WinnerId,
                    summary.AwardedPoints,
                    summary.Scores[1],
                    summary.Scores[2],
                    summary.RemainingHands[2].Count,
                    summary.RemainingHands[2][0].TotalPips
                },
                Is.EqualTo(new object[] { 5, 1, 5, 25, 40, 1, 7 }));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void ResolvePlayedOut_CollectionMutation_IsRejected(int collection)
        {
            // Arrange
            List<MatchPlayer> players = CreatePlayedOutPlayers(7);
            RoundSummary summary = RoundResolution.ResolvePlayedOut(players, 1, 1);
            NotSupportedException failure = null;
            // Act
            try
            {
                switch (collection)
                {
                    case 0:
                        ((IDictionary<int, int>)summary.Scores).Clear();
                        break;
                    case 1:
                        ((IDictionary<int, IReadOnlyList<Tile>>)summary.RemainingHands).Clear();
                        break;
                    case 2:
                        ((IList<Tile>)summary.RemainingHands[2]).Clear();
                        break;
                }
            }
            catch (NotSupportedException ex)
            {
                failure = ex;
            }
            // Assert
            Assert.That(
                new object[]
                {
                    failure?.GetType(),
                    summary.Scores.Count,
                    summary.RemainingHands.Count,
                    summary.RemainingHands[2].Count
                },
                Is.EqualTo(new object[] { typeof(NotSupportedException), 2, 2, 1 }));
        }

        [TestCase("PlayedOut", 0, typeof(ArgumentNullException), "players")]
        [TestCase("Blocked", 0, typeof(ArgumentNullException), "players")]
        [TestCase("PlayedOut", 1, typeof(ArgumentException), "players")]
        [TestCase("Blocked", 1, typeof(ArgumentException), "players")]
        [TestCase("PlayedOut", 2, typeof(ArgumentException), "players")]
        [TestCase("Blocked", 2, typeof(ArgumentException), "players")]
        [TestCase("PlayedOut", 3, typeof(ArgumentException), "players")]
        [TestCase("Blocked", 3, typeof(ArgumentException), "players")]
        [TestCase("PlayedOut", 4, typeof(ArgumentOutOfRangeException), "roundNumber")]
        [TestCase("Blocked", 4, typeof(ArgumentOutOfRangeException), "roundNumber")]
        [TestCase("PlayedOut", 5, typeof(ArgumentOutOfRangeException), "roundNumber")]
        [TestCase("Blocked", 5, typeof(ArgumentOutOfRangeException), "roundNumber")]
        public void Resolve_InvalidRound_ReportsArgument(
            string operation,
            int scenario,
            Type exceptionType,
            string parameterName)
        {
            // Arrange
            List<MatchPlayer> players = CreatePlayedOutPlayers(7);
            int roundNumber = 1;
            switch (scenario)
            {
                case 0:
                    players = null;
                    break;
                case 1:
                    players = CreatePlayers(0);
                    break;
                case 2:
                    players = CreatePlayers(1);
                    break;
                case 3:
                    players = CreatePlayers(5);
                    break;
                case 4:
                    roundNumber = 0;
                    break;
                case 5:
                    roundNumber = -1;
                    break;
            }

            ArgumentException failure = null;
            // Act
            try
            {
                if (operation == "PlayedOut")
                {
                    RoundResolution.ResolvePlayedOut(players, roundNumber, 1);
                }
                else
                {
                    RoundResolution.ResolveBlocked(players, roundNumber);
                }
            }
            catch (ArgumentException ex)
            {
                failure = ex;
            }
            // Assert
            Assert.That(
                new object[] { failure?.GetType(), failure?.ParamName },
                Is.EqualTo(new object[] { exceptionType, parameterName }));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(99)]
        public void ResolvePlayedOut_UnknownWinner_RejectsWithoutMutation(int winnerId)
        {
            // Arrange
            List<MatchPlayer> players = CreatePlayedOutPlayers(7);
            List<int> before = CapturePlayers(players);
            MatchRuleException failure = null;
            // Act
            try
            {
                RoundResolution.ResolvePlayedOut(players, 1, winnerId);
            }
            catch (MatchRuleException ex)
            {
                failure = ex;
            }
            // Assert
            Assert.That(
                new object[] { failure?.Issue, CapturePlayers(players) },
                Is.EqualTo(new object[] { MatchIssue.PlayerNotInMatch, before }));
        }

        [Test]
        public void ResolvePlayedOut_WinnerStillHasTiles_RejectsWithoutMutation()
        {
            // Arrange
            List<MatchPlayer> players = CreatePlayers(2);
            List<int> before = CapturePlayers(players);
            InvalidOperationException failure = null;
            // Act
            try
            {
                RoundResolution.ResolvePlayedOut(players, 1, 1);
            }
            catch (InvalidOperationException ex)
            {
                failure = ex;
            }
            // Assert
            Assert.That(
                new object[] { failure?.GetType(), CapturePlayers(players) },
                Is.EqualTo(new object[] { typeof(InvalidOperationException), before }));
        }

        private static List<MatchPlayer> CreatePlayedOutPlayers(int remainingPips)
        {
            List<MatchPlayer> players = CreatePlayers(2);
            players[0].Points = 20;
            players[0].Hand.Clear();
            players[1].Points = 40;
            players[1].Hand.Clear();
            if (remainingPips == 53)
            {
                List<Tile> deck = MatchTestDeck.Ordered(66, 56, 46, 36, 26, 3);
                players[1].Hand.AddRange(deck.GetRange(0, 6));
            }
            else if (remainingPips <= 6)
            {
                players[1].Hand.Add(new Tile(1, 0, remainingPips, SpecialPower.None));
            }
            else
            {
                players[1].Hand.Add(new Tile(1, remainingPips - 6, 6, SpecialPower.None));
            }

            return players;
        }

        private static List<MatchPlayer> CreateBlockedPlayers(int scenario)
        {
            int[][] hands =
            {
                new[] { 66, 33 },
                new[] { 6, 15, 2 },
                new[] { 6, 15, 26 },
                new[] { 4, 6, 13, 26 },
                new[] { 6, 15, 3, 4 },
                new[] { 6, 15, 24, 33 }
            };
            int[] values = hands[scenario];
            List<MatchPlayer> players = CreatePlayers(values.Length);
            for (int index = 0; index < values.Length; index++)
            {
                players[index].Hand.Clear();
                players[index].Hand.Add(new Tile(
                    index + 1,
                    values[index] / 10,
                    values[index] % 10,
                    SpecialPower.None));
            }

            return players;
        }

        private static int[] GetBlockedScores(int scenario)
        {
            int[][] scores =
            {
                new[] { 10, 40 },
                new[] { 10, 20, 45 },
                new[] { 10, 20, 30 },
                new[] { 10, 20, 30, 40 },
                new[] { 10, 20, 50, 40 },
                new[] { 10, 20, 30, 40 }
            };
            return scores[scenario];
        }

        private static List<MatchPlayer> CreatePlayers(int count)
        {
            List<MatchPlayer> players = new List<MatchPlayer>();
            for (int id = 1; id <= count; id++)
            {
                MatchPlayer player = new MatchPlayer
                {
                    PlayerId = id,
                    Points = id * 10
                };
                player.Hand.Add(new Tile(id, 0, id, SpecialPower.None));
                players.Add(player);
            }

            return players;
        }

        private static List<int> CapturePlayers(List<MatchPlayer> players)
        {
            List<int> snapshot = new List<int>();
            foreach (MatchPlayer player in players)
            {
                snapshot.Add(player.PlayerId);
                snapshot.Add(player.Points);
                snapshot.Add(player.Tiles.Count);
                foreach (Tile tile in player.Tiles)
                {
                    snapshot.Add(tile.TileId);
                }
            }

            return snapshot;
        }
    }
}
