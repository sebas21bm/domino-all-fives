using System;
using System.Collections.Generic;
using System.Linq;
using DominoAllFives.BusinessLogic.Gameplay;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class PlayScoreTests
    {
        [TestCase(SpecialPower.None, 1, 10)]
        [TestCase(SpecialPower.DoublePoints, 2, 20)]
        [TestCase(SpecialPower.Block, 1, 10)]
        public void Play_OpeningScore_AccreditsExactlyExplainedPoints(
            SpecialPower power,
            int multiplier,
            int awardedPoints)
        {
            // Arrange
            List<Tile> deck = MatchTestDeck.Ordered(55);
            Tile opening = deck[0];
            deck[0] = new Tile(opening.TileId, 5, 5, power);
            Match match = new Match(1, new[] { 1, 2 }, 250, deck);
            int playerId = match.CurrentPlayerId.Value;
            // Act
            match.Play(playerId, match.OpeningTileId, BoardEndPosition.Left, match.Version);
            // Assert
            Assert.That(
                new object[]
                {
                    match.GetPlayer(playerId).Points,
                    match.GetPlayer(2).Points,
                    match.LastPlay.PlayerId,
                    match.LastPlay.RoundNumber,
                    match.LastPlay.BasePoints,
                    match.LastPlay.Multiplier,
                    match.LastPlay.Points,
                    match.LastPlay.RoundPoints,
                    match.LastPlay.Tile.TileId,
                    match.LastPlay.PlayedEnd
                },
                Is.EqualTo(new object[]
                {
                    awardedPoints, 0, playerId, 1, 10, multiplier, awardedPoints, 0,
                    opening.TileId, BoardEndPosition.Left
                }));
        }

        [Test]
        public void Play_RedLastTile_DoublesPlayWithoutDoublingRoundAward()
        {
            // Arrange
            List<Tile> deck = MatchTestDeck.Ordered(
                5, 34, 4, 23, 14, 24, 56, 0, 33, 44, 45, 11, 26, 3,
                2, 22, 12, 25, 46, 55, 36, 35, 13, 6, 66, 1, 16, 15);
            Tile lastTile = deck[6];
            deck[6] = new Tile(lastTile.TileId, 5, 6, SpecialPower.DoublePoints);
            Match match = new Match(1, new[] { 1, 2 }, 250, deck);
            for (int action = 0; action < 14; action++)
            {
                GameplayTests.PlayNextLegalAction(match);
            }
            // Act
            GameplayTests.PlayNextLegalAction(match);
            // Assert
            Assert.That(
                new object[]
                {
                    match.LastPlay.BasePoints,
                    match.LastPlay.Multiplier,
                    match.LastPlay.Points,
                    match.LastPlay.RoundPoints,
                    match.LastRound.AwardedPoints,
                    match.LastRound.Scores[1],
                    match.GetPlayer(1).Points,
                    match.RoundNumber
                },
                Is.EqualTo(new object[] { 15, 2, 30, 5, 5, 50, 50, 2 }));
        }

        [Test]
        public void Play_RedWithOpeningDouble_ReportsExactAwardAndSurvivesContractMapping()
        {
            // Arrange
            Match match = CreateMatch();
            PlayTile(match, 6, 6, BoardEndPosition.Left);
            PlayTile(match, 0, 6, BoardEndPosition.Top);
            PlayTile(match, 1, 6, BoardEndPosition.Right);
            PlayTile(match, 0, 3, BoardEndPosition.Top);
            PlayTile(match, 1, 3, BoardEndPosition.Right);
            PlayTile(match, 3, 4, BoardEndPosition.Top);
            int previousPoints = match.GetPlayer(1).Points;
            // Act
            PlayTile(match, 4, 5, BoardEndPosition.Top);
            DominoAllFives.Contracts.Gameplay.MatchView view = MatchContractMapper.CreateMatchView(match);
            // Assert
            bool endValuesMatch = view.LastPlay.EndValues
                .OrderBy(value => value)
                .SequenceEqual(new[] { 3, 5, 12 });
            Assert.That(
                new object[]
                {
                    endValuesMatch,
                    view.LastPlay.Total,
                    view.LastPlay.IncludesOpeningDouble,
                    view.LastPlay.Multiplier,
                    view.LastPlay.Points,
                    match.GetPlayer(1).Points - previousPoints,
                    view.LastPlay.BlockedEnds.Count
                },
                Is.EqualTo(new object[] { true, 20, true, 2, 40, 40, 0 }));
        }

        [Test]
        public void Play_PurpleBlock_RecordsScoringTurnInsteadOfNextTurn()
        {
            // Arrange
            Match match = CreateMatch();
            PlayTile(match, 6, 6, BoardEndPosition.Left);
            PlayTile(match, 0, 6, BoardEndPosition.Top);
            PlayTile(match, 1, 6, BoardEndPosition.Right);
            // Act
            PlayTile(match, 0, 3, BoardEndPosition.Top);
            DominoAllFives.Contracts.Gameplay.MatchView view = MatchContractMapper.CreateMatchView(match);
            // Assert
            bool endValuesMatch = view.LastPlay.EndValues
                .OrderBy(value => value)
                .SequenceEqual(new[] { 3, 12 });
            Assert.That(
                new object[]
                {
                    view.LastPlay.BlockedEnds.Single(),
                    endValuesMatch,
                    view.LastPlay.Points,
                    view.Board.Ends.Single(end =>
                        end.End == Contracts.Gameplay.BoardEnd.Right).IsBlocked
                },
                Is.EqualTo(new object[]
                {
                    Contracts.Gameplay.BoardEnd.Right,
                    true,
                    15,
                    false
                }));
        }

        [Test]
        public void CreateView_ChangedBreakdown_DoesNotMutateAuthoritativeValues()
        {
            // Arrange
            Match match = CreateMatch();
            PlayTile(match, 6, 6, BoardEndPosition.Left);
            DominoAllFives.Contracts.Gameplay.MatchView view = MatchContractMapper.CreateMatchView(match);
            // Act
            view.LastPlay.EndValues.Clear();
            view.LastPlay.Tile.FirstValue = 99;
            // Assert
            Assert.That(
                new object[]
                {
                    match.LastPlay.EndValues.Single(),
                    match.LastPlay.Tile.FirstValue,
                    match.LastPlay.Points
                },
                Is.EqualTo(new object[] { 12, 6, 0 }));
        }

        [Test]
        public void Draw_AfterPlacement_PreservesLastPlayExplanation()
        {
            // Arrange
            List<Tile> initialDeck = CreateDrawDeck();
            Match match = new Match(1, new[] { 1, 2 }, 100, initialDeck);
            PlayTile(match, 6, 6, BoardEndPosition.Left);
            PlayScore score = match.LastPlay;
            // Act
            match.Draw(match.CurrentPlayerId.Value, match.Version);
            // Assert
            Assert.That(match.LastPlay, Is.SameAs(score));
        }

        private static Match CreateMatch()
        {
            List<Tile> initialDeck = CreateDeck();
            return new Match(1, new[] { 1, 2 }, 100, initialDeck);
        }

        private static List<Tile> CreateDrawDeck()
        {
            List<Tile> openingDoubles = new List<Tile>();
            List<Tile> tilesWithoutSix = new List<Tile>();
            List<Tile> remainingTilesWithSix = new List<Tile>();
            foreach (Tile tile in new DeckGenerator().Generate())
            {
                bool isOpeningDouble = tile.IsDouble
                    && tile.FirstValue == 6;
                if (isOpeningDouble)
                {
                    openingDoubles.Add(tile);
                    continue;
                }

                if (tile.Matches(6))
                {
                    remainingTilesWithSix.Add(tile);
                    continue;
                }

                tilesWithoutSix.Add(tile);
            }

            List<Tile> orderedTiles = new List<Tile>();
            orderedTiles.AddRange(openingDoubles);
            orderedTiles.AddRange(tilesWithoutSix);
            orderedTiles.AddRange(remainingTilesWithSix);
            return orderedTiles;
        }

        private static List<Tile> CreateDeck()
        {
            int[] values =
            {
                66,
                16,
                13,
                45,
                0,
                1,
                2,
                6,
                3,
                34,
                11,
                12,
                14,
                15
            };
            List<Tile> allTiles = new List<Tile>();
            foreach (Tile tile in new DeckGenerator().Generate())
            {
                allTiles.Add(CreateTile(tile));
            }

            List<Tile> orderedTiles = new List<Tile>();
            foreach (int value in values)
            {
                int firstValue = value / 10;
                int secondValue = value % 10;
                orderedTiles.Add(FindTile(allTiles, firstValue, secondValue));
            }

            foreach (Tile tile in allTiles)
            {
                if (!orderedTiles.Contains(tile))
                {
                    orderedTiles.Add(tile);
                }
            }

            return orderedTiles;
        }

        private static Tile CreateTile(Tile tile)
        {
            SpecialPower power = SpecialPower.None;
            if (tile.FirstValue == 1 && tile.SecondValue == 6)
            {
                power = SpecialPower.Block;
            }
            else if (tile.FirstValue == 4 && tile.SecondValue == 5)
            {
                power = SpecialPower.DoublePoints;
            }

            return new Tile(tile.TileId, tile.FirstValue, tile.SecondValue, power);
        }

        private static void PlayTile(Match match, int first, int second, BoardEndPosition end)
        {
            int playerId = match.CurrentPlayerId.Value;
            Tile tile = FindTile(match.GetPlayer(playerId).Tiles, first, second);
            match.Play(playerId, tile.TileId, end, match.Version);
        }

        private static Tile FindTile(
            IEnumerable<Tile> tiles,
            int firstValue,
            int secondValue)
        {
            foreach (Tile tile in tiles)
            {
                bool firstValueMatches = tile.FirstValue == firstValue;
                bool secondValueMatches = tile.SecondValue == secondValue;
                if (firstValueMatches && secondValueMatches)
                {
                    return tile;
                }
            }

            throw new InvalidOperationException("The requested tile is not available.");
        }
    }
}
