using System.Collections.Generic;
using System.Linq;
using DominoAllFives.BusinessLogic.Gameplay;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class PlayScoreTests
    {
        [Test]
        public void Play_RedWithOpeningDouble_ReportsExactAwardAndSurvivesProjection()
        {
            // Arrange
            Match match = CreateMatch();
            PlayTile(match, 6, 6, End.Left);
            PlayTile(match, 0, 6, End.Top);
            PlayTile(match, 1, 6, End.Right);
            PlayTile(match, 0, 3, End.Top);
            PlayTile(match, 1, 3, End.Right);
            PlayTile(match, 3, 4, End.Top);
            int previousPoints = match.GetPlayer(1).Points;
            // Act
            PlayTile(match, 4, 5, End.Top);
            DominoAllFives.Contracts.Gameplay.MatchView view = MatchProjection.CreateView(match);
            // Assert
            Assert.That(view.LastPlay.EndValues, Is.EquivalentTo(new[] { 12, 5, 3 }));
            Assert.That(view.LastPlay.Total, Is.EqualTo(20));
            Assert.That(view.LastPlay.IncludesOpeningDouble, Is.True);
            Assert.That(view.LastPlay.Multiplier, Is.EqualTo(2));
            Assert.That(view.LastPlay.Points, Is.EqualTo(40));
            Assert.That(match.GetPlayer(1).Points - previousPoints, Is.EqualTo(40));
            Assert.That(view.LastPlay.BlockedEnds, Is.Empty);
        }

        [Test]
        public void Play_PurpleBlock_RecordsScoringTurnInsteadOfNextTurn()
        {
            // Arrange
            Match match = CreateMatch();
            PlayTile(match, 6, 6, End.Left);
            PlayTile(match, 0, 6, End.Top);
            PlayTile(match, 1, 6, End.Right);
            // Act
            PlayTile(match, 0, 3, End.Top);
            DominoAllFives.Contracts.Gameplay.MatchView view = MatchProjection.CreateView(match);
            // Assert
            Assert.That(view.LastPlay.BlockedEnds, Is.EqualTo(
                new[] { Contracts.Gameplay.BoardEnd.Right }));
            Assert.That(view.LastPlay.EndValues, Is.EquivalentTo(new[] { 12, 3 }));
            Assert.That(view.LastPlay.Points, Is.EqualTo(15));
            Assert.That(view.Board.Ends.Single(end
                => end.End == Contracts.Gameplay.BoardEnd.Right).IsBlocked, Is.False);
        }

        [Test]
        public void CreateView_ChangedBreakdown_DoesNotMutateAuthoritativeValues()
        {
            // Arrange
            Match match = CreateMatch();
            PlayTile(match, 6, 6, End.Left);
            DominoAllFives.Contracts.Gameplay.MatchView view = MatchProjection.CreateView(match);
            // Act
            view.LastPlay.EndValues.Clear();
            view.LastPlay.Tile.FirstValue = 99;
            // Assert
            Assert.That(match.LastPlay.EndValues, Is.EqualTo(new[] { 12 }));
            Assert.That(match.LastPlay.Tile.FirstValue, Is.EqualTo(6));
            Assert.That(match.LastPlay.Points, Is.Zero);
        }

        [Test]
        public void Draw_AfterPlacement_PreservesLastPlayExplanation()
        {
            // Arrange
            Match match = new Match(1, new[] { 1, 2 }, 100, CreateDrawDeck);
            PlayTile(match, 6, 6, End.Left);
            PlayScore score = match.LastPlay;
            // Act
            match.Draw(match.CurrentPlayerId.Value, match.Version);
            // Assert
            Assert.That(match.LastPlay, Is.SameAs(score));
        }

        private static Match CreateMatch()
        {
            return new Match(1, new[] { 1, 2 }, 100, CreateDeck);
        }

        private static List<Tile> CreateDrawDeck()
        {
            return Deck.Create()
                .OrderByDescending(tile => tile.IsDouble && tile.FirstValue == 6).ThenBy(tile
                    => tile.Matches(6))
                .ToList();
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
            List<Tile> all = Deck.Create()
                .Select(CreateTile)
                .ToList();
            List<Tile> ordered = values.Select(value => all.Single(tile => tile.FirstValue * 10
                + tile.SecondValue == value))
                .ToList();
            return ordered.Concat(all.Where(tile => !ordered.Contains(tile)))
                .ToList();
        }

        private static Tile CreateTile(Tile tile)
        {
            SpecialPower power = SpecialPower.None;
            if (tile.FirstValue == 1 && tile.SecondValue == 6)
            {
                power = SpecialPower.Block;
            }

            if (tile.FirstValue == 4 && tile.SecondValue == 5)
            {
                power = SpecialPower.DoublePoints;
            }

            return new Tile(tile.TileId, tile.FirstValue, tile.SecondValue, power);
        }

        private static void PlayTile(Match match, int first, int second, End end)
        {
            int playerId = match.CurrentPlayerId.Value;
            Tile tile = match.GetPlayer(playerId).Tiles.Single(candidate
                => candidate.FirstValue == first && candidate.SecondValue == second);
            match.Play(playerId, tile.TileId, end, match.Version);
        }
    }
}
