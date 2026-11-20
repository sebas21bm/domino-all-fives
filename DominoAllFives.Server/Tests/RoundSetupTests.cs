using System;
using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Gameplay;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class RoundSetupTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(1)]
        public void Prepare_InvalidPlayerId_RejectsWithoutChangingInputs(int secondId)
        {
            // Arrange
            RoundSetup setup = new RoundSetup();
            List<int> ids = new List<int> { 1, secondId };
            List<Tile> deck = MatchTestDeck.Ordered();
            Tile[] previousDeck = deck.ToArray();
            ArgumentException failure = null;
            // Act
            try
            {
                setup.Prepare(ids, deck);
            }
            catch (ArgumentException ex)
            {
                failure = ex;
            }
            // Assert
            Assert.That(
                new object[]
                {
                    failure?.ParamName, ids, deck
                },
                Is.EqualTo(new object[] { "playerIds", new[] { 1, secondId }, previousDeck }));
        }

        [Test]
        public void Prepare_SourceIdsChange_PreservesPreparedPlayersAndBoard()
        {
            // Arrange
            RoundSetup setup = new RoundSetup();
            List<int> ids = new List<int> { 1, 2 };
            PreparedRound round = setup.Prepare(ids, MatchTestDeck.Ordered(66, 55));
            // Act
            ids.Clear();
            // Assert
            Assert.That(
                new object[]
                {
                    round.Players.Count, round.Players[0].PlayerId,
                    round.Players[1].PlayerId, round.Players[0].Tiles.Count,
                    round.Stock.Count, round.Board.Tiles.Count, round.Board.Ends.Count
                },
                Is.EqualTo(new object[] { 2, 1, 2, 7, 14, 0, 0 }));
        }

        [Test]
        public void Prepare_FirstResultChanges_PreservesSecondResult()
        {
            // Arrange
            RoundSetup setup = new RoundSetup();
            int[] ids = { 1, 2 };
            List<Tile> deck = MatchTestDeck.Ordered();
            PreparedRound first = setup.Prepare(ids, deck);
            PreparedRound second = setup.Prepare(ids, deck);
            List<int> before = CapturePlayers(new List<MatchPlayer>(second.Players));
            Tile[] stock = second.Stock.ToArray();
            // Act
            first.Players[0].Hand.Clear();
            first.Stock.Dequeue();
            first.Board.Place(deck[0], BoardEndPosition.Left, 0);
            // Assert
            Assert.That(
                new object[]
                {
                    CapturePlayers(new List<MatchPlayer>(second.Players)),
                    second.Stock.ToArray(), second.Board.Tiles.Count
                },
                Is.EqualTo(new object[] { before, stock, 0 }));
        }

        [Test]
        public void Prepare_SourceDeckChanges_PreservesPreparedStockAndHands()
        {
            // Arrange
            RoundSetup setup = new RoundSetup();
            List<MatchPlayer> players = CreatePlayers(2);
            List<Tile> deck = MatchTestDeck.Ordered();
            PreparedRound round = setup.Prepare(GetIds(players), deck);
            List<int> before = CapturePlayers(new List<MatchPlayer>(round.Players));
            Tile[] previousStock = round.Stock.ToArray();
            // Act
            deck.Clear();
            // Assert
            Assert.That(
                new object[]
                {
                    round.Stock.ToArray(), CapturePlayers(new List<MatchPlayer>(round.Players))
                },
                Is.EqualTo(new object[] { previousStock, before }));
        }

        [Test]
        public void Prepare_NullPlayerIds_ReportsPlayerIdsArgument()
        {
            // Arrange
            List<Tile> deck = MatchTestDeck.Ordered();
            RoundSetup setup = new RoundSetup();
            ArgumentNullException failure = null;
            // Act
            try
            {
                setup.Prepare(null, deck);
            }
            catch (ArgumentNullException ex)
            {
                failure = ex;
            }
            // Assert
            Assert.That(failure?.ParamName, Is.EqualTo("playerIds"));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        public void Prepare_InvalidPlayerCount_ReportsPlayerIdsArgument(int playerCount)
        {
            // Arrange
            List<Tile> deck = MatchTestDeck.Ordered();
            RoundSetup setup = new RoundSetup();
            List<MatchPlayer> players = CreatePlayers(playerCount);
            ArgumentException failure = null;
            // Act
            try
            {
                setup.Prepare(GetIds(players), deck);
            }
            catch (ArgumentException ex)
            {
                failure = ex;
            }
            // Assert
            Assert.That(failure?.ParamName, Is.EqualTo("playerIds"));
        }

        [TestCase(2, 7, 14)]
        [TestCase(3, 5, 13)]
        [TestCase(4, 5, 8)]
        public void Prepare_ExistingHands_ReturnsNewHandsWithoutChangingPlayers(
            int playerCount,
            int handSize,
            int stockCount)
        {
            // Arrange
            List<Tile> tiles = MatchTestDeck.Ordered(66, 55);
            RoundSetup setup = new RoundSetup();
            List<MatchPlayer> players = CreatePlayers(playerCount);
            List<int> originalState = CapturePlayers(players);
            // Act
            PreparedRound round = setup.Prepare(GetIds(players), tiles);
            // Assert
            List<Tile> dealt = new List<Tile>();
            List<int> counts = new List<int>();
            List<int> expectedCounts = new List<int>();
            foreach (MatchPlayer originalPlayer in players)
            {
                foreach (MatchPlayer preparedPlayer in round.Players)
                {
                    if (preparedPlayer.PlayerId == originalPlayer.PlayerId)
                    {
                        dealt.AddRange(preparedPlayer.Tiles);
                        counts.Add(preparedPlayer.Tiles.Count);
                    }
                }

                expectedCounts.Add(handSize);
            }

            Assert.That(
                new object[]
                {
                    dealt,
                    round.Stock.ToArray(),
                    counts,
                    CapturePlayers(players),
                    round.OpeningTileId
                },
                Is.EqualTo(new object[]
                {
                    tiles.GetRange(0, playerCount * handSize),
                    tiles.GetRange(playerCount * handSize, stockCount),
                    expectedCounts,
                    originalState,
                    tiles[0].TileId
                }));
        }

        [TestCase(0, 1, 0)]
        [TestCase(1, 2, 66)]
        [TestCase(2, 2, 36)]
        public void Prepare_OpeningPriority_OrdersPlayersAndSelectsRequiredTile(
            int scenario,
            int openingPlayerId,
            int openingValue)
        {
            // Arrange
            int[] values = GetOpeningValues(scenario);
            List<Tile> tiles = MatchTestDeck.Ordered(values);
            RoundSetup setup = new RoundSetup();
            List<MatchPlayer> players = CreatePlayers(2);
            List<int> playerIds = GetIds(players);
            // Act
            PreparedRound round = setup.Prepare(playerIds, tiles);
            // Assert
            MatchPlayer openingPlayer = round.Players[0];
            int actualValue = -1;
            foreach (Tile tile in openingPlayer.Tiles)
            {
                if (tile.TileId == round.OpeningTileId)
                {
                    actualValue = tile.FirstValue * 10 + tile.SecondValue;
                }
            }

            Assert.That(
                new object[] { openingPlayer.PlayerId, actualValue, playerIds },
                Is.EqualTo(new object[] { openingPlayerId, openingValue, new[] { 1, 2 } }));
        }

        [TestCase(0, typeof(ArgumentNullException))]
        [TestCase(1, typeof(ArgumentException))]
        [TestCase(2, typeof(ArgumentException))]
        [TestCase(3, typeof(ArgumentException))]
        [TestCase(4, typeof(ArgumentException))]
        [TestCase(5, typeof(ArgumentException))]
        public void Prepare_InvalidDeck_PreservesInputs(int scenario, Type expectedExceptionType)
        {
            // Arrange
            List<Tile> tiles = CreateInvalidDeck(scenario);
            RoundSetup setup = new RoundSetup();
            List<int> playerIds = new List<int> { 1, 2 };
            Tile[] previousDeck = null;
            if (tiles != null)
            {
                previousDeck = tiles.ToArray();
            }

            ArgumentException failure = null;
            // Act
            try
            {
                setup.Prepare(playerIds, tiles);
            }
            catch (ArgumentException ex)
            {
                failure = ex;
            }
            // Assert
            Assert.That(
                new object[] { failure?.GetType(), playerIds, tiles },
                Is.EqualTo(new object[]
                {
                    expectedExceptionType, new[] { 1, 2 }, previousDeck
                }));
        }

        [Test]
        public void Prepare_InvalidNextDeck_PreservesPreviousPreparation()
        {
            // Arrange
            List<Tile> deck = MatchTestDeck.Ordered();
            RoundSetup setup = new RoundSetup();
            List<MatchPlayer> players = CreatePlayers(2);
            PreparedRound previousRound = setup.Prepare(GetIds(players), deck);
            List<Tile> previousStock = new List<Tile>(previousRound.Stock);
            List<Tile> invalidDeck = MatchTestDeck.Ordered();
            invalidDeck.RemoveAt(0);
            List<int> before = CapturePlayers(new List<MatchPlayer>(previousRound.Players));
            ArgumentException failure = null;
            // Act
            try
            {
                setup.Prepare(GetIds(players), invalidDeck);
            }
            catch (ArgumentException ex)
            {
                failure = ex;
            }
            // Assert
            Assert.That(
                new object[]
                {
                    failure?.GetType(),
                    CapturePlayers(new List<MatchPlayer>(previousRound.Players)),
                    previousRound.Stock.ToArray()
                },
                Is.EqualTo(new object[] { typeof(ArgumentException), before, previousStock }));
        }

        private static List<int> GetIds(List<MatchPlayer> players)
        {
            List<int> ids = new List<int>();
            foreach (MatchPlayer player in players)
            {
                ids.Add(player.PlayerId);
            }

            return ids;
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
                player.Hand.Add(new Tile(100 + id, 1, 1, SpecialPower.None));
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

        private static int[] GetOpeningValues(int scenario)
        {
            switch (scenario)
            {
                case 0:
                    return new[] { 0, 1, 2, 3, 4, 5, 6, 56, 12, 13, 14, 15, 16, 23 };
                case 1:
                    return new[] { 55, 1, 2, 3, 4, 5, 6, 66, 12, 13, 14, 15, 16, 23 };
                case 2:
                    return new[] { 45, 1, 2, 3, 4, 5, 6, 36, 12, 13, 14, 15, 16, 23 };
                default:
                    throw new ArgumentOutOfRangeException(nameof(scenario));
            }
        }

        private static List<Tile> CreateInvalidDeck(int scenario)
        {
            List<Tile> tiles = MatchTestDeck.Ordered();
            switch (scenario)
            {
                case 0:
                    return null;
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

            return tiles;
        }
    }
}
