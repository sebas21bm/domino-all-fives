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
            ArgumentOutOfRangeException exception = null;
            try
            {
                new Tile(tileId, first, second, power);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                exception = ex;
            }
            // Assert
            Assert.That(exception, Is.TypeOf<ArgumentOutOfRangeException>());
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
            List<Tile> initialDeck = FixedDeck();
            // Act
            ArgumentOutOfRangeException exception = null;
            try
            {
                new Match(1, players, targetPoints, initialDeck);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                exception = ex;
            }
            // Assert
            Assert.That(exception, Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCase(-1)]
        [TestCase(4)]
        public void Place_UndefinedEnd_PreservesBoard(int invalidEnd)
        {
            // Arrange
            Board board = new Board();
            Tile tile = new Tile(1, 6, 6, SpecialPower.None);
            // Act
            MatchRuleException exception = null;
            try
            {
                board.Place(tile, (BoardEndPosition)invalidEnd, 0);
            }
            catch (MatchRuleException ex)
            {
                exception = ex;
            }
            // Assert
            Assert.That(
                new object[] { exception?.Issue, board.Tiles.Count, board.Ends.Count },
                Is.EqualTo(new object[] { MatchIssue.InvalidPlacement, 0, 0 }));
        }

        [Test]
        public void Draw_PlayableHand_RejectsWithoutConsumingStock()
        {
            // Arrange
            Match match = NewMatch();
            int playerId = match.CurrentPlayerId.Value;
            int stockCount = match.StockCount;
            // Act
            MatchRuleException exception = null;
            try
            {
                match.Draw(playerId, 0);
            }
            catch (MatchRuleException ex)
            {
                exception = ex;
            }
            // Assert
            Assert.That(
                new object[] { exception?.Issue, match.StockCount, match.Version },
                Is.EqualTo(new object[] { MatchIssue.DrawNotAllowed, stockCount, 0L }));
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
            Assert.That(
                new object[]
                {
                    match.Version,
                    MatchContractMapper.CreateMatchResult(match).WinnerIds.Count
                },
                Is.EqualTo(new object[] { version, 0 }));
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
            MatchRuleException exception = null;
            try
            {
                match.Play(playerId, other.TileId, BoardEndPosition.Left, 0);
            }
            catch (MatchRuleException ex)
            {
                exception = ex;
            }
            // Assert
            Assert.That(
                new object[] { exception?.Issue, match.Version, match.Board.Tiles.Count },
                Is.EqualTo(new object[] { MatchIssue.OpeningTileRequired, 0L, 0 }));
        }

        [Test]
        public void Match_RoundFinishes_RevealsPreviousHandsAndDealsNewRound()
        {
            // Arrange
            List<Tile> initialDeck = FixedDeck();
            Match match = new Match(1, new[] { 1, 2 }, 250, initialDeck);
            int actions = 0;
            // Act
            while (match.LastRound == null && match.Phase == MatchState.InProgress
                && actions++ < 1000)
            {
                PlayNextLegalAction(match);
            }

            // Assert
            bool openingPlayerOwnsRequiredTile = OwnsOpeningTile(match);
            Assert.That(
                new object[]
                {
                    match.LastRound?.RoundNumber,
                    match.LastRound?.RemainingHands.Count,
                    match.RoundNumber,
                    match.Players.All(player => player.Tiles.Count == 7),
                    openingPlayerOwnsRequiredTile
                },
                Is.EqualTo(new object[] { 1, 2, 2, true, true }));
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
            Match match = new Match(1, players, 100, new DeckGenerator().Generate());
            // Assert
            Assert.That(
                new object[]
                {
                    match.Players.All(player => player.Tiles.Count == size),
                    match.StockCount
                },
                Is.EqualTo(new object[] { true, stock }));
        }

        [Test]
        public void Create_FullDeck_HasUniqueTilesAndFiveSpecialPowers()
        {
            // Arrange
            List<Tile> deck;
            // Act
            deck = new DeckGenerator().Generate();
            // Assert
            Assert.That(
                new object[]
                {
                    deck.Select(tile => tile.TileId).Distinct().Count(),
                    deck.Count(tile => tile.SpecialPower != SpecialPower.None)
                },
                Is.EqualTo(new object[] { 28, 5 }));
        }

        [Test]
        public void Place_OpeningDouble_EnablesFourEndsAndCountsItOnce()
        {
            // Arrange
            Board board = new Board();
            // Act
            board.Place(new Tile(1, 5, 5, SpecialPower.None), BoardEndPosition.Left, 0);
            // Assert
            Assert.That(
                new object[] { board.Ends.Count, PlayScoring.Evaluate(board, 0).BasePoints },
                Is.EqualTo(new object[] { 4, 10 }));
        }

        [Test]
        public void Place_OpeningNonDouble_EnablesTwoEnds()
        {
            // Arrange
            Board board = new Board();
            // Act
            board.Place(new Tile(1, 2, 3, SpecialPower.None), BoardEndPosition.Left, 0);
            // Assert
            Assert.That(
                new object[] { board.Ends.Count, PlayScoring.Evaluate(board, 0).BasePoints },
                Is.EqualTo(new object[] { 2, 5 }));
        }

        [Test]
        public void Place_MismatchedTile_DoesNotMutateBoard()
        {
            // Arrange
            Board board = new Board();
            board.Place(new Tile(1, 2, 3, SpecialPower.None), BoardEndPosition.Left, 0);
            // Act
            MatchRuleException exception = null;
            try
            {
                board.Place(new Tile(2, 4, 6, SpecialPower.None), BoardEndPosition.Right, 1);
            }
            catch (MatchRuleException ex)
            {
                exception = ex;
            }
            // Assert
            Assert.That(
                new object[] { exception?.Issue, board.Tiles.Count },
                Is.EqualTo(new object[] { MatchIssue.InvalidPlacement, 1 }));
        }

        [Test]
        public void Place_TerminalDouble_CountsBothHalves()
        {
            // Arrange
            Board board = new Board();
            board.Place(new Tile(1, 5, 0, SpecialPower.None), BoardEndPosition.Left, 0);
            // Act
            board.Place(new Tile(2, 5, 5, SpecialPower.None), BoardEndPosition.Left, 1);
            // Assert
            Assert.That(PlayScoring.Evaluate(board, 1).BasePoints, Is.EqualTo(10));
        }

        [Test]
        public void Place_Purple_BlocksOnlyNextTurnAndExcludesItsPoints()
        {
            // Arrange
            Board board = new Board();
            board.Place(new Tile(1, 2, 3, SpecialPower.None), BoardEndPosition.Left, 0);
            board.Place(new Tile(2, 3, 5, SpecialPower.Block), BoardEndPosition.Right, 1);
            Tile next = new Tile(3, 5, 6, SpecialPower.None);
            // Act
            bool blocked = board.CanPlay(next, BoardEndPosition.Right, 2);
            bool released = board.CanPlay(next, BoardEndPosition.Right, 3);
            // Assert
            Assert.That(
                new object[]
                {
                    blocked,
                    released,
                    board.Ends.Single(end => end.End == BoardEndPosition.Right).BlockedTurn
                },
                Is.EqualTo(new object[] { false, true, 2 }));
        }

        [Test]
        public void Evaluate_PurpleEnd_IsExcludedDuringNextTurn()
        {
            // Arrange
            Board board = new Board();
            board.Place(new Tile(1, 5, 3, SpecialPower.None), BoardEndPosition.Left, 0);
            board.Place(new Tile(2, 3, 5, SpecialPower.Block), BoardEndPosition.Right, 1);
            // Act
            int nextTurnPoints = PlayScoring.Evaluate(board, 2).BasePoints;
            // Assert
            Assert.That(
                new object[] { nextTurnPoints, PlayScoring.Evaluate(board, 3).BasePoints },
                Is.EqualTo(new object[] { 5, 10 }));
        }

        [Test]
        public void Play_StaleVersion_PreservesState()
        {
            // Arrange
            Match match = NewMatch();
            int playerId = match.CurrentPlayerId.Value;
            int tileId = match.OpeningTileId;
            // Act
            MatchRuleException exception = null;
            try
            {
                match.Play(playerId, tileId, BoardEndPosition.Left, -1);
            }
            catch (MatchRuleException ex)
            {
                exception = ex;
            }
            // Assert
            Assert.That(
                new object[] { exception?.Issue, match.Version, match.Board.Tiles.Count },
                Is.EqualTo(new object[] { MatchIssue.StaleVersion, 0L, 0 }));
        }

        [Test]
        public void Play_UnownedTile_IsRejected()
        {
            // Arrange
            Match match = NewMatch();
            // Act
            MatchRuleException exception = null;
            try
            {
                match.Play(
                    match.CurrentPlayerId.Value,
                    999,
                    BoardEndPosition.Left,
                    0);
            }
            catch (MatchRuleException ex)
            {
                exception = ex;
            }
            // Assert
            Assert.That(exception?.Issue, Is.EqualTo(MatchIssue.TileNotOwned));
        }

        [Test]
        public void Play_RedOpening_DoublesOnlyMovePoints()
        {
            // Arrange
            List<Tile> deck = OrderedDeck(new[] { 55, 0, 1, 2, 3, 4, 5, 11, 12, 13, 14, 15, 22,
                23 });
            Tile opening = FindTileByValues(deck, 5, 5);
            deck.Remove(opening);
            deck.Insert(0, new Tile(opening.TileId, 5, 5, SpecialPower.DoublePoints));
            List<Tile> initialDeck = deck;
            Match match = new Match(1, new[] { 1, 2 }, 100, initialDeck);
            // Act
            match.Play(1, opening.TileId, BoardEndPosition.Left, 0);
            // Assert
            Assert.That(
                new object[] { match.GetPlayer(1).Points, match.Version },
                Is.EqualTo(new object[] { 20, 1L }));
        }

        [Test]
        public void Draw_NoPlayableTile_DrawsUntilPlayableWithoutAdvancingTurn()
        {
            // Arrange
            List<Tile> deck = OrderedDeck(new[] { 66, 0, 1, 2, 3, 4, 5, 11, 12, 13, 14, 15, 22,
                23 });
            List<Tile> initialDeck = deck;
            Match match = new Match(1, new[] { 1, 2 }, 100, initialDeck);
            match.Play(1, deck[0].TileId, BoardEndPosition.Left, 0);
            // Act
            match.Draw(2, 1);
            // Assert
            Assert.That(
                new object[]
                {
                    match.GetPlayer(2).Tiles.Count,
                    match.StockCount,
                    match.CurrentPlayerId,
                    match.Version
                },
                Is.EqualTo(new object[] { 8, 13, 2, 2L }));
        }

        [Test]
        public void Pass_StockNotEmpty_IsRejected()
        {
            // Arrange
            Match match = NewMatch();
            // Act
            MatchRuleException exception = null;
            try
            {
                match.Pass(match.CurrentPlayerId.Value, 0);
            }
            catch (MatchRuleException ex)
            {
                exception = ex;
            }
            // Assert
            Assert.That(exception?.Issue, Is.EqualTo(MatchIssue.PassNotAllowed));
        }

        [Test]
        public void GetHand_NonParticipant_IsRejected()
        {
            // Arrange
            MatchController coordinator = new MatchController(new MatchManager());
            coordinator.CreateMatch(1, new[] { 1, 2 }, 100);
            // Act
            MatchRuleException exception = null;
            try
            {
                coordinator.GetPlayerHand(
                    1,
                    3);
            }
            catch (MatchRuleException ex)
            {
                exception = ex;
            }
            // Assert
            Assert.That(exception?.Issue, Is.EqualTo(MatchIssue.PlayerNotInMatch));
        }

        [Test]
        public void Play_ConcurrentSameVersion_AcceptsAtMostOneMutation()
        {
            // Arrange
            MatchManager store = new MatchManager();
            MatchController coordinator = new MatchController(store);
            coordinator.CreateMatch(1, new[] { 1, 2 }, 100);
            DominoAllFives.Contracts.Gameplay.MatchView initial = coordinator.GetMatch(1, 1);
            int playerId = initial.CurrentPlayerId.Value;
            int tileId = initial.OpeningTileId;
            bool[] accepted = new bool[2];
            // Act
            ConcurrentPlayAttempt playAttempt = new ConcurrentPlayAttempt(
                coordinator,
                playerId,
                tileId,
                accepted);
            Parallel.For(
                0,
                accepted.Length,
                playAttempt.Execute);
            // Assert
            Assert.That(
                new object[]
                {
                    accepted.Count(wasAccepted => wasAccepted),
                    coordinator.GetMatch(1, playerId).Version
                },
                Is.EqualTo(new object[] { 1, 1L }));
        }

        [Test]
        public void CreateView_ModifiedNetworkTile_DoesNotChangeDomain()
        {
            // Arrange
            Match match = NewMatch();
            int playerId = match.CurrentPlayerId.Value;
            DominoAllFives.Contracts.Gameplay.PlayerHand hand = MatchContractMapper.CreatePlayerHand(
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
            MatchRuleException exception = null;
            try
            {
                match.Draw(playerId, match.Version);
            }
            catch (MatchRuleException ex)
            {
                exception = ex;
            }
            // Assert
            Assert.That(
                new object[]
                {
                    match.Phase,
                    MatchContractMapper.CreateMatchResult(match).WinnerIds.Count,
                    exception?.Issue
                },
                Is.EqualTo(new object[]
                {
                    MatchState.Cancelled,
                    0,
                    MatchIssue.MatchNotInProgress
                }));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Match_LegalAutomatedPlay_CompletesAndConservesTiles(int playerCount)
        {
            // Arrange
            DeckGenerator generator = new DeckGenerator();
            List<Tile> initialDeck = generator.Generate();
            IEnumerable<int> playerIds = Enumerable.Range(1, playerCount);
            Match match = new Match(1, playerIds, 100, initialDeck);
            int actionCount = 0;
            bool tileCountWasPreserved = true;
            // Act
            while (match.Phase == MatchState.InProgress && actionCount++ < 5000)
            {
                PlayNextLegalAction(match);
                tileCountWasPreserved &= match.Players.Sum(player => player.Tiles.Count)
                    + match.StockCount
                    + match.Board.Tiles.Count
                    == 28;
            }

            // Assert
            Assert.That(
                new object[]
                {
                    match.Phase,
                    match.Players.Max(player => player.Points) >= 100,
                    tileCountWasPreserved
                },
                Is.EqualTo(new object[] { MatchState.Completed, true, true }));
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

                foreach (BoardEndPosition end in Enum.GetValues(typeof(BoardEndPosition)))
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
                coordinator.PlayTile(
                    1,
                    playerId,
                    new Move
                    {
                        TileId = tileId,
                        End = BoardEndPosition.Left,
                        ExpectedVersion = 0
                    });
                return true;
            }
            catch (MatchRuleException ex) when (ex.Issue == MatchIssue.StaleVersion)
            {
                return false;
            }
        }

        private static Match NewMatch()
        {
            List<Tile> initialDeck = FixedDeck();
            return new Match(1, new[] { 1, 2 }, 100, initialDeck);
        }

        private static bool OwnsOpeningTile(Match match)
        {
            MatchPlayer openingPlayer = match.GetPlayer(match.CurrentPlayerId.Value);
            foreach (Tile tile in openingPlayer.Tiles)
            {
                if (tile.TileId == match.OpeningTileId)
                {
                    return true;
                }
            }

            return false;
        }

        private static Tile FindTileByValues(
            IEnumerable<Tile> tiles,
            int firstValue,
            int secondValue)
        {
            foreach (Tile tile in tiles)
            {
                if (tile.FirstValue == firstValue
                    && tile.SecondValue == secondValue)
                {
                    return tile;
                }
            }

            throw new InvalidOperationException("The requested tile is not available.");
        }

        private static List<Tile> FixedDeck()
        {
            return OrderedDeck(new[] { 66, 55, 0, 1, 2, 3, 4, 11, 12, 13, 14, 15, 22, 23 });
        }

        private static List<Tile> OrderedDeck(int[] values)
        {
            List<Tile> allTiles = new List<Tile>();
            foreach (Tile tile in new DeckGenerator().Generate())
            {
                Tile tileWithoutPower = new Tile(
                    tile.TileId,
                    tile.FirstValue,
                    tile.SecondValue,
                    SpecialPower.None);
                allTiles.Add(tileWithoutPower);
            }

            List<Tile> orderedTiles = new List<Tile>();
            foreach (int value in values)
            {
                int firstValue = value / 10;
                int secondValue = value % 10;
                Tile tile = FindTileByValues(
                    allTiles,
                    firstValue,
                    secondValue);
                orderedTiles.Add(tile);
            }

            List<Tile> remainingTiles = new List<Tile>();
            foreach (Tile tile in allTiles)
            {
                if (!orderedTiles.Contains(tile))
                {
                    remainingTiles.Add(tile);
                }
            }

            remainingTiles.Sort(CompareTilesById);
            orderedTiles.AddRange(remainingTiles);
            return orderedTiles;
        }

        private static int CompareTilesById(Tile left, Tile right)
        {
            return left.TileId.CompareTo(right.TileId);
        }

        private sealed class ConcurrentPlayAttempt
        {
            private readonly MatchController _matchController;
            private readonly int _playerId;
            private readonly int _tileId;
            private readonly bool[] _accepted;

            public ConcurrentPlayAttempt(
                MatchController matchController,
                int playerId,
                int tileId,
                bool[] accepted)
            {
                _matchController = matchController
                    ?? throw new ArgumentNullException(nameof(matchController));
                _playerId = playerId;
                _tileId = tileId;
                _accepted = accepted
                    ?? throw new ArgumentNullException(nameof(accepted));
            }

            public void Execute(int attempt)
            {
                _accepted[attempt] = TryPlay(
                    _matchController,
                    _playerId,
                    _tileId);
            }
        }
    }
}
