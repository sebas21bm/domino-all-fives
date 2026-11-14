using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Gameplay;
using DominoAllFives.BusinessLogic.Managers;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class GameplayTests
    {
        [TestCase(0, 1, 2)]
        [TestCase(1, -1, 2)]
        [TestCase(1, 7, 2)]
        [TestCase(1, 1, -1)]
        [TestCase(1, 1, 7)]
        public void Tile_InvalidIdentityOrPips_RejectsInput(int tileId, int first, int second)
        {
            // Arrange
            SpecialPower power = SpecialPower.None;
            // Act
            TestDelegate create = () => new Tile(tileId, first, second, power);
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(create);
        }

        [TestCase(0)]
        [TestCase(99)]
        [TestCase(125)]
        [TestCase(300)]
        public void Match_UnsupportedTarget_RejectsInput(int targetPoints)
        {
            // Arrange
            int[] players =
            {
                1,
                2
            };
            // Act
            TestDelegate create = () => new Match(1, players, targetPoints, FixedDeck);
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(create);
        }

        [TestCase(-1)]
        [TestCase(4)]
        public void Place_UndefinedEnd_PreservesBoard(int invalidEnd)
        {
            // Arrange
            Board board = new Board();
            Tile tile = new Tile(1, 6, 6, SpecialPower.None);
            // Act
            MatchRuleException ex = Assert.Throws<MatchRuleException>(() => board.Place(tile, (
                End)invalidEnd, 0));
            // Assert
            Assert.That(ex.Issue, Is.EqualTo(MatchIssue.InvalidPlacement));
            Assert.That(board.Tiles, Is.Empty);
            Assert.That(board.Ends, Is.Empty);
        }

        [Test]
        public void Draw_PlayableHand_RejectsWithoutConsumingStock()
        {
            // Arrange
            Match match = NewMatch();
            int playerId = match.CurrentPlayerId.Value;
            int stockCount = match.StockCount;
            // Act
            MatchRuleException ex = Assert.Throws<MatchRuleException>(() => match.Draw(playerId,
                0));
            // Assert
            Assert.That(ex.Issue, Is.EqualTo(MatchIssue.DrawNotAllowed));
            Assert.That(match.StockCount, Is.EqualTo(stockCount));
            Assert.That(match.Version, Is.Zero);
        }

        [Test]
        public void Cancel_RepeatedRequest_ProducesOnlyOneVersionChange()
        {
            // Arrange
            Match match = NewMatch();
            int playerId = match.CurrentPlayerId.Value;
            match.Cancel(playerId);
            long version = match.Version;
            // Act
            match.Cancel(playerId);
            // Assert
            Assert.That(match.Version, Is.EqualTo(version));
            Assert.That(MatchProjection.CreateResult(match).WinnerIds, Is.Empty);
        }

        [Test]
        public void Play_DifferentOpeningTile_IsRejectedWithoutMutation()
        {
            // Arrange
            Match match = NewMatch();
            int playerId = match.CurrentPlayerId.Value;
            Tile other = match.GetPlayer(playerId).Tiles.First(tile
                => tile.TileId != match.OpeningTileId);
            // Act
            MatchRuleException ex = Assert.Throws<MatchRuleException>(() => match.Play(playerId,
                other.TileId, End.Left, 0));
            // Assert
            Assert.That(ex.Issue, Is.EqualTo(MatchIssue.OpeningTileRequired));
            Assert.That(match.Version, Is.Zero);
            Assert.That(match.Board.Tiles, Is.Empty);
        }

        [Test]
        public void Match_RoundFinishes_RevealsPreviousHandsAndDealsNewRound()
        {
            // Arrange
            Match match = new Match(1, new[] { 1, 2 }, 250, FixedDeck);
            int actions = 0;
            // Act
            while (match.LastRound == null && match.Phase == MatchState.InProgress
                && actions++ < 1000)
            {
                PlayNextLegalAction(match);
            }

            // Assert
            Assert.That(match.LastRound, Is.Not.Null);
            Assert.That(match.LastRound.RoundNumber, Is.EqualTo(1));
            Assert.That(match.LastRound.RemainingHands.Count, Is.EqualTo(2));
            Assert.That(match.RoundNumber, Is.EqualTo(2));
            Assert.That(match.Players.All(player => player.Tiles.Count == 7), Is.True);
            Assert.That(match.OpeningTileId, Is.EqualTo(match.GetPlayer(
                match.CurrentPlayerId.Value).Tiles.Single(tile => tile.FirstValue == 6
                && tile.SecondValue == 6).TileId));
        }

        [TestCase(2, 7, 14)]
        [TestCase(3, 5, 13)]
        [TestCase(4, 5, 8)]
        public void Match_NewRound_DealsDocumentedHandSize(int count, int size, int stock)
        {
            // Arrange
            int[] players = Enumerable.Range(1, count)
                .ToArray();
            // Act
            Match match = new Match(1, players, 100, Deck.Create);
            // Assert
            Assert.That(match.Players.All(player => player.Tiles.Count == size));
            Assert.That(match.StockCount, Is.EqualTo(stock));
        }

        [Test]
        public void Create_FullDeck_HasUniqueTilesAndFiveSpecialPowers()
        {
            // Arrange
            List<Tile> deck;
            // Act
            deck = Deck.Create();
            // Assert
            Assert.That(deck.Select(tile => tile.TileId)
                .Distinct()
                .Count(), Is.EqualTo(28));
            Assert.That(deck.Count(tile => tile.SpecialPower != SpecialPower.None), Is.EqualTo(5));
        }

        [Test]
        public void Place_OpeningDouble_EnablesFourEndsAndCountsItOnce()
        {
            // Arrange
            Board board = new Board();
            // Act
            board.Place(new Tile(1, 5, 5, SpecialPower.None), End.Left, 0);
            // Assert
            Assert.That(board.Ends.Count, Is.EqualTo(4));
            Assert.That(board.CalculatePoints(0), Is.EqualTo(10));
        }

        [Test]
        public void Place_OpeningNonDouble_EnablesTwoEnds()
        {
            // Arrange
            Board board = new Board();
            // Act
            board.Place(new Tile(1, 2, 3, SpecialPower.None), End.Left, 0);
            // Assert
            Assert.That(board.Ends.Count, Is.EqualTo(2));
            Assert.That(board.CalculatePoints(0), Is.EqualTo(5));
        }

        [Test]
        public void Place_MismatchedTile_DoesNotMutateBoard()
        {
            // Arrange
            Board board = new Board();
            board.Place(new Tile(1, 2, 3, SpecialPower.None), End.Left, 0);
            // Act
            MatchRuleException ex = Assert.Throws<MatchRuleException>(() => board.Place(
                new Tile(2, 4, 6, SpecialPower.None), End.Right, 1));
            // Assert
            Assert.That(ex.Issue, Is.EqualTo(MatchIssue.InvalidPlacement));
            Assert.That(board.Tiles.Count, Is.EqualTo(1));
        }

        [Test]
        public void Place_TerminalDouble_CountsBothHalves()
        {
            // Arrange
            Board board = new Board();
            board.Place(new Tile(1, 5, 0, SpecialPower.None), End.Left, 0);
            // Act
            board.Place(new Tile(2, 5, 5, SpecialPower.None), End.Left, 1);
            // Assert
            Assert.That(board.CalculatePoints(1), Is.EqualTo(10));
        }

        [Test]
        public void Place_Purple_BlocksOnlyNextTurnAndExcludesItsPoints()
        {
            // Arrange
            Board board = new Board();
            board.Place(new Tile(1, 2, 3, SpecialPower.None), End.Left, 0);
            board.Place(new Tile(2, 3, 5, SpecialPower.Block), End.Right, 1);
            Tile next = new Tile(3, 5, 6, SpecialPower.None);
            // Act
            bool blocked = board.CanPlay(next, End.Right, 2);
            bool released = board.CanPlay(next, End.Right, 3);
            // Assert
            Assert.That(blocked, Is.False);
            Assert.That(released, Is.True);
            Assert.That(board.Ends.Single(end => end.End == End.Right).BlockedTurn, Is.EqualTo(2));
        }

        [Test]
        public void CalculatePoints_PurpleEnd_IsExcludedDuringNextTurn()
        {
            // Arrange
            Board board = new Board();
            board.Place(new Tile(1, 5, 3, SpecialPower.None), End.Left, 0);
            board.Place(new Tile(2, 3, 5, SpecialPower.Block), End.Right, 1);
            // Act
            int nextTurnPoints = board.CalculatePoints(2);
            // Assert
            Assert.That(nextTurnPoints, Is.EqualTo(5));
            Assert.That(board.CalculatePoints(3), Is.EqualTo(10));
        }

        [Test]
        public void Play_StaleVersion_PreservesState()
        {
            // Arrange
            Match match = NewMatch();
            int playerId = match.CurrentPlayerId.Value;
            int tileId = match.OpeningTileId;
            // Act
            MatchRuleException ex = Assert.Throws<MatchRuleException>(() => match.Play(playerId,
                tileId, End.Left, -1));
            // Assert
            Assert.That(ex.Issue, Is.EqualTo(MatchIssue.StaleVersion));
            Assert.That(match.Version, Is.Zero);
            Assert.That(match.Board.Tiles.Count, Is.Zero);
        }

        [Test]
        public void Play_UnownedTile_IsRejected()
        {
            // Arrange
            Match match = NewMatch();
            // Act
            MatchRuleException ex = Assert.Throws<MatchRuleException>(() => match.Play(
                match.CurrentPlayerId.Value, 999, End.Left, 0));
            // Assert
            Assert.That(ex.Issue, Is.EqualTo(MatchIssue.TileNotOwned));
        }

        [Test]
        public void Play_RedOpening_DoublesOnlyMovePoints()
        {
            // Arrange
            List<Tile> deck = OrderedDeck(new[] { 55, 0, 1, 2, 3, 4, 5, 11, 12, 13, 14, 15, 22,
                23 });
            Tile opening = deck.Single(tile => tile.FirstValue == 5 && tile.SecondValue == 5);
            deck.Remove(opening);
            deck.Insert(0, new Tile(opening.TileId, 5, 5, SpecialPower.DoublePoints));
            Match match = new Match(1, new[] { 1, 2 }, 100, () => deck);
            // Act
            match.Play(1, opening.TileId, End.Left, 0);
            // Assert
            Assert.That(match.GetPlayer(1).Points, Is.EqualTo(20));
            Assert.That(match.Version, Is.EqualTo(1));
        }

        [Test]
        public void Draw_NoPlayableTile_DrawsUntilPlayableWithoutAdvancingTurn()
        {
            // Arrange
            List<Tile> deck = OrderedDeck(new[] { 66, 0, 1, 2, 3, 4, 5, 11, 12, 13, 14, 15, 22,
                23 });
            Match match = new Match(1, new[] { 1, 2 }, 100, () => deck);
            match.Play(1, deck[0].TileId, End.Left, 0);
            // Act
            match.Draw(2, 1);
            // Assert
            Assert.That(match.GetPlayer(2).Tiles.Count, Is.EqualTo(8));
            Assert.That(match.StockCount, Is.EqualTo(13));
            Assert.That(match.CurrentPlayerId, Is.EqualTo(2));
            Assert.That(match.Version, Is.EqualTo(2));
        }

        [Test]
        public void Pass_StockNotEmpty_IsRejected()
        {
            // Arrange
            Match match = NewMatch();
            // Act
            MatchRuleException ex = Assert.Throws<MatchRuleException>(() => match.Pass(
                match.CurrentPlayerId.Value, 0));
            // Assert
            Assert.That(ex.Issue, Is.EqualTo(MatchIssue.PassNotAllowed));
        }

        [Test]
        public void GetHand_NonParticipant_IsRejected()
        {
            // Arrange
            MatchController coordinator = new MatchController(new MatchManager(), unused =>
            {
            });
            coordinator.Create(1, new[] { 1, 2 }, 100);
            // Act
            MatchRuleException ex = Assert.Throws<MatchRuleException>(() => coordinator.GetHand(
                1, 3));
            // Assert
            Assert.That(ex.Issue, Is.EqualTo(MatchIssue.PlayerNotInMatch));
        }

        [Test]
        public void Play_ConcurrentSameVersion_AcceptsAtMostOneMutation()
        {
            // Arrange
            MatchManager store = new MatchManager();
            MatchController coordinator = new MatchController(store, unused =>
            {
            });
            coordinator.Create(1, new[] { 1, 2 }, 100);
            Match match = store.GetMatch(1).Match;
            int playerId = match.CurrentPlayerId.Value;
            int tileId = match.OpeningTileId;
            bool[] accepted = new bool[2];
            // Act
            Parallel.For(0, accepted.Length, attempt => accepted[attempt] = TryPlay(coordinator,
                playerId, tileId));
            // Assert
            Assert.That(accepted.Count(wasAccepted => wasAccepted), Is.EqualTo(1));
            Assert.That(match.Version, Is.EqualTo(1));
        }

        [Test]
        public void CreateView_ModifiedNetworkTile_DoesNotChangeDomain()
        {
            // Arrange
            Match match = NewMatch();
            int playerId = match.CurrentPlayerId.Value;
            DominoAllFives.Contracts.Gameplay.PlayerHand hand = MatchProjection.CreateHand(
                match, playerId);
            int original = match.GetPlayer(playerId).Tiles[0].FirstValue;
            // Act
            hand.Tiles[0].FirstValue = 999;
            // Assert
            Assert.That(match.GetPlayer(playerId).Tiles[0].FirstValue, Is.EqualTo(original));
        }

        [Test]
        public void Cancel_ActiveMatch_HasNoWinnerAndPreventsFurtherMoves()
        {
            // Arrange
            Match match = NewMatch();
            int playerId = match.CurrentPlayerId.Value;
            // Act
            match.Cancel(playerId);
            // Assert
            Assert.That(match.Phase, Is.EqualTo(MatchState.Cancelled));
            Assert.That(MatchProjection.CreateResult(match).WinnerIds, Is.Empty);
            Assert.Throws<MatchRuleException>(() => match.Draw(playerId, match.Version));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Match_LegalAutomatedPlay_CompletesAndConservesTiles(int playerCount)
        {
            // Arrange
            Match match = new Match(1, Enumerable.Range(1, playerCount), 100, Deck.Create);
            int actionCount = 0;
            // Act
            while (match.Phase == MatchState.InProgress && actionCount++ < 5000)
            {
                PlayNextLegalAction(match);
                Assert.That(match.Players.Sum(player => player.Tiles.Count) + match.StockCount
                    + match.Board.Tiles.Count, Is.EqualTo(28));
            }

            // Assert
            Assert.That(match.Phase, Is.EqualTo(MatchState.Completed));
            Assert.That(match.Players.Max(player => player.Points), Is.GreaterThanOrEqualTo(100));
        }

        internal static void PlayNextLegalAction(Match match)
        {
            int playerId = match.CurrentPlayerId.Value;
            foreach (Tile tile in match.GetPlayer(playerId).Tiles)
            {
                if (match.Board.Tiles.Count == 0 && tile.TileId != match.OpeningTileId)
                {
                    continue;
                }

                foreach (End end in Enum.GetValues(typeof(End)))
                {
                    if (!match.Board.CanPlay(tile, end, match.TurnNumber))
                    {
                        continue;
                    }

                    match.Play(playerId, tile.TileId, end, match.Version);
                    return;
                }
            }

            if (match.StockCount > 0)
            {
                match.Draw(playerId, match.Version);
            }
            else
            {
                match.Pass(playerId, match.Version);
            }
        }

        private static bool TryPlay(MatchController coordinator, int playerId, int tileId)
        {
            try
            {
                coordinator.Play(1, playerId, new Move { TileId = tileId, End = End.Left,
                    ExpectedVersion = 0 });
                return true;
            }
            catch (MatchRuleException ex)
            {
                Assert.That(ex.Issue, Is.EqualTo(MatchIssue.StaleVersion));
                return false;
            }
        }

        private static Match NewMatch()
        {
            return new Match(1, new[] { 1, 2 }, 100, FixedDeck);
        }

        private static List<Tile> FixedDeck()
        {
            return OrderedDeck(new[] { 66, 55, 0, 1, 2, 3, 4, 11, 12, 13, 14, 15, 22, 23 });
        }

        private static List<Tile> OrderedDeck(int[] values)
        {
            List<Tile> all = Deck.Create()
                .Select(tile => new Tile(tile.TileId, tile.FirstValue, tile.SecondValue,
                    SpecialPower.None))
                .ToList();
            List<Tile> ordered = values.Select(value => all.Single(tile => tile.FirstValue * 10
                + tile.SecondValue == value))
                .ToList();
            return ordered.Concat(all.Where(tile => !ordered.Contains(tile))
                .OrderBy(tile => tile.TileId))
                .ToList();
        }
    }
}
