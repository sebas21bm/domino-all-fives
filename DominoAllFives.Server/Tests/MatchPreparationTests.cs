using System;
using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Gameplay;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class MatchPreparationTests
    {
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Play_PreparedPlacement_PreservesPlayerIdentityAndPreviousSnapshots(int count)
        {
            // Arrange
            List<int> ids = new List<int>();
            for (int id = 1; id <= count; id++)
            {
                ids.Add(id);
            }

            Match match = new Match(1, ids, 250, MatchTestDeck.Ordered());
            MatchPlayer player = match.GetPlayer(match.CurrentPlayerId.Value);
            Board previousBoard = match.Board;
            IReadOnlyList<Tile> previousHand = player.Tiles;
            int handSize = previousHand.Count;
            int stockCount = match.StockCount;
            // Act
            match.Play(player.PlayerId, match.OpeningTileId, BoardEndPosition.Left, 0);
            // Assert
            Assert.That(
                new object[]
                {
                    ReferenceEquals(player, match.GetPlayer(player.PlayerId)),
                    previousBoard.Tiles.Count, previousHand.Count, player.Tiles.Count,
                    match.Board.Tiles.Count, match.Version, match.TurnNumber, match.StockCount
                },
                Is.EqualTo(new object[] { true, 0, handSize, handSize - 1, 1, 1L, 1, stockCount }));
        }

        [TestCase(2, 7, 14)]
        [TestCase(3, 5, 13)]
        [TestCase(4, 5, 8)]
        public void Constructor_OrderedDeck_PreservesExactHandsAndInitialState(
            int playerCount,
            int handSize,
            int stockCount)
        {
            // Arrange
            List<int> ids = new List<int>();
            for (int id = 1; id <= playerCount; id++)
            {
                ids.Add(id);
            }

            List<Tile> tiles = MatchTestDeck.Ordered(66, 55);
            // Act
            Match match = new Match(1, ids, 250, tiles);
            // Assert
            List<Tile> dealt = new List<Tile>();
            foreach (int id in ids)
            {
                dealt.AddRange(match.GetPlayer(id).Tiles);
            }

            List<Tile> expected = tiles.GetRange(0, playerCount * handSize);
            Assert.That(
                new object[]
                {
                    dealt,
                    match.StockCount,
                    match.RoundNumber,
                    match.TurnNumber,
                    match.Version,
                    match.CurrentPlayerId,
                    match.OpeningTileId,
                    match.Board.Tiles.Count,
                    match.Phase
                },
                Is.EqualTo(new object[]
                {
                    expected, stockCount, 1, 0, 0L, 1, tiles[0].TileId, 0,
                    MatchState.InProgress
                }));
        }

        [TestCase(100)]
        [TestCase(150)]
        [TestCase(200)]
        [TestCase(250)]
        public void Constructor_AllowedTarget_PreservesTarget(int target)
        {
            // Arrange
            List<Tile> tiles = MatchTestDeck.Ordered();
            // Act
            Match match = new Match(1, new[] { 1, 2 }, target, tiles);
            // Assert
            Assert.That(match.TargetPoints, Is.EqualTo(target));
        }

        [Test]
        public void Constructor_NoDealtDoubles_SelectsHighestSumThenHighestValue()
        {
            // Arrange
            List<Tile> tiles = MatchTestDeck.Ordered(
                45, 1, 2, 3, 4, 5, 6,
                36, 12, 13, 14, 15, 16, 23);
            // Act
            Match match = new Match(1, new[] { 1, 2 }, 250, tiles);
            // Assert
            Assert.That(
                new object[] { match.CurrentPlayerId, match.OpeningTileId },
                Is.EqualTo(new object[] { 2, tiles[7].TileId }));
        }

        [TestCase(0, typeof(ArgumentNullException))]
        [TestCase(1, typeof(ArgumentException))]
        [TestCase(2, typeof(ArgumentException))]
        [TestCase(3, typeof(ArgumentException))]
        [TestCase(4, typeof(ArgumentException))]
        [TestCase(5, typeof(ArgumentException))]
        public void Constructor_InvalidDeck_RejectsBeforeDealing(
            int scenario,
            Type expectedExceptionType)
        {
            // Arrange
            List<Tile> tiles = MatchTestDeck.Ordered();
            switch (scenario)
            {
                case 0:
                    tiles = null;
                    break;
                case 1:
                    tiles.RemoveAt(0);
                    break;
                case 2:
                    tiles[0] = null;
                    break;
                case 3:
                    tiles[1] = new Tile(tiles[0].TileId, 0, 1, SpecialPower.None);
                    break;
                case 4:
                    tiles[2] = new Tile(tiles[2].TileId, 1, 0, SpecialPower.None);
                    break;
                case 5:
                    for (int index = 0; index < 6; index++)
                    {
                        Tile tile = tiles[index];
                        tiles[index] = new Tile(
                            tile.TileId,
                            tile.FirstValue,
                            tile.SecondValue,
                            SpecialPower.Block);
                    }
                    break;
            }
            ArgumentException failure = null;
            // Act
            try
            {
                new Match(1, new[] { 1, 2 }, 100, tiles);
            }
            catch (ArgumentException ex)
            {
                failure = ex;
            }
            // Assert
            Assert.That(failure, Is.TypeOf(expectedExceptionType));
        }

        [TestCase(new int[] { 1 })]
        [TestCase(new int[] { 1, 2, 3, 4, 5 })]
        [TestCase(new int[] { 1, 1 })]
        [TestCase(new int[] { 0, 2 })]
        [TestCase(new int[] { -1, 2 })]
        public void Constructor_InvalidPlayers_ReportsPlayerIds(int[] ids)
        {
            // Arrange
            List<Tile> tiles = MatchTestDeck.Ordered();
            ArgumentException failure = null;
            // Act
            try
            {
                new Match(1, ids, 100, tiles);
            }
            catch (ArgumentException ex)
            {
                failure = ex;
            }
            // Assert
            Assert.That(
                new object[] { failure?.GetType(), failure?.ParamName },
                Is.EqualTo(new object[] { typeof(ArgumentException), "playerIds" }));
        }

        [TestCase(0, "matchId", typeof(ArgumentOutOfRangeException))]
        [TestCase(1, "matchId", typeof(ArgumentOutOfRangeException))]
        [TestCase(2, "playerIds", typeof(ArgumentNullException))]
        public void Constructor_InvalidIdentity_ReportsArgument(
            int scenario,
            string parameterName,
            Type exceptionType)
        {
            // Arrange
            List<Tile> tiles = MatchTestDeck.Ordered();
            int matchId = 1;
            int[] ids = new[] { 1, 2 };
            switch (scenario)
            {
                case 0:
                    matchId = 0;
                    break;
                case 1:
                    matchId = -1;
                    break;
                case 2:
                    ids = null;
                    break;
            }

            ArgumentException failure = null;
            // Act
            try
            {
                new Match(matchId, ids, 100, tiles);
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
    }
}
