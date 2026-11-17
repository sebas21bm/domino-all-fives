using System;
using DominoAllFives.BusinessLogic.Gameplay;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class PlayScoringTests
    {
        [TestCase(SpecialPower.None, 1, 10)]
        [TestCase(SpecialPower.DoublePoints, 2, 20)]
        [TestCase(SpecialPower.Block, 1, 10)]
        public void Evaluate_OpeningDouble_ReportsExactAward(
            SpecialPower power,
            int multiplier,
            int awardedPoints)
        {
            // Arrange
            Board board = new Board();
            Tile tile = new Tile(1, 5, 5, power);
            board.Place(tile, BoardEndPosition.Left, 0);
            // Act
            PlayScore score = PlayScoring.Evaluate(board, 0);
            // Assert
            Assert.That(
                new object[]
                {
                    score.Total,
                    score.BasePoints,
                    score.Multiplier,
                    score.Points,
                    score.Tile,
                    score.PlayedEnd,
                    score.EndValues,
                    score.IncludesOpeningDouble,
                    score.BlockedEnds.Count
                },
                Is.EqualTo(new object[]
                {
                    10, 10, multiplier, awardedPoints, tile, BoardEndPosition.Left,
                    new[] { 10 }, true, 0
                }));
        }

        [TestCase(SpecialPower.None, 1)]
        [TestCase(SpecialPower.DoublePoints, 2)]
        public void Evaluate_NonScoringTotal_PreservesTotalWithoutAward(
            SpecialPower power,
            int multiplier)
        {
            // Arrange
            Board board = new Board();
            board.Place(new Tile(1, 3, 3, power), BoardEndPosition.Left, 0);
            // Act
            PlayScore score = PlayScoring.Evaluate(board, 0);
            // Assert
            Assert.That(
                new object[] { score.Total, score.BasePoints, score.Multiplier, score.Points },
                Is.EqualTo(new object[] { 6, 0, multiplier, 0 }));
        }

        [TestCase(SpecialPower.None, 1)]
        [TestCase(SpecialPower.DoublePoints, 2)]
        public void Evaluate_BlankDouble_ReportsZeroAward(SpecialPower power, int multiplier)
        {
            // Arrange
            Board board = new Board();
            board.Place(new Tile(1, 0, 0, power), BoardEndPosition.Left, 0);
            // Act
            PlayScore score = PlayScoring.Evaluate(board, 0);
            // Assert
            Assert.That(
                new object[]
                {
                    score.Total,
                    score.BasePoints,
                    score.Multiplier,
                    score.Points,
                    score.EndValues,
                    score.IncludesOpeningDouble
                },
                Is.EqualTo(new object[] { 0, 0, multiplier, 0, new[] { 0 }, true }));
        }

        [TestCase(SpecialPower.None, 1, 10)]
        [TestCase(SpecialPower.DoublePoints, 2, 20)]
        public void Evaluate_TerminalDouble_CountsBothHalves(
            SpecialPower power,
            int multiplier,
            int awardedPoints)
        {
            // Arrange
            Board board = new Board();
            board.Place(new Tile(1, 0, 5, SpecialPower.None), BoardEndPosition.Left, 0);
            Tile tile = new Tile(2, 5, 5, power);
            board.Place(tile, BoardEndPosition.Right, 1);
            // Act
            PlayScore score = PlayScoring.Evaluate(board, 1);
            // Assert
            Assert.That(
                new object[]
                {
                    score.EndValues,
                    score.Total,
                    score.BasePoints,
                    score.Multiplier,
                    score.Points,
                    score.Tile,
                    score.PlayedEnd,
                    score.IncludesOpeningDouble
                },
                Is.EqualTo(new object[]
                {
                    new[] { 0, 10 }, 10, 10, multiplier, awardedPoints,
                    tile, BoardEndPosition.Right, false
                }));
        }

        [Test]
        public void Evaluate_OpeningDoubleWithUnusedEnds_CountsOpeningOnlyOnce()
        {
            // Arrange
            Board board = new Board();
            board.Place(new Tile(1, 5, 5, SpecialPower.None), BoardEndPosition.Left, 0);
            board.Place(new Tile(2, 0, 5, SpecialPower.None), BoardEndPosition.Left, 1);
            // Act
            PlayScore score = PlayScoring.Evaluate(board, 1);
            // Assert
            Assert.That(
                new object[] { score.EndValues, score.Total, score.IncludesOpeningDouble },
                Is.EqualTo(new object[] { new[] { 0, 10 }, 10, true }));
        }

        [Test]
        public void Evaluate_BlockedEnd_ExcludesItsValueAndExplainsBlock()
        {
            // Arrange
            Board board = CreateBoardWithBlockedEnd();
            // Act
            PlayScore score = PlayScoring.Evaluate(board, 2);
            // Assert
            Assert.That(
                new object[]
                {
                    score.EndValues,
                    score.BlockedEnds,
                    score.Total,
                    score.BasePoints,
                    score.Points
                },
                Is.EqualTo(new object[]
                {
                    new[] { 0 }, new[] { BoardEndPosition.Right }, 0, 0, 0
                }));
        }

        [Test]
        public void Evaluate_ExpiredBlock_IncludesReleasedEnd()
        {
            // Arrange
            Board board = CreateBoardWithBlockedEnd();
            board.Place(new Tile(4, 4, 5, SpecialPower.None), BoardEndPosition.Right, 3);
            // Act
            PlayScore score = PlayScoring.Evaluate(board, 3);
            // Assert
            Assert.That(
                new object[]
                {
                    score.EndValues,
                    score.Total,
                    score.Points,
                    score.BlockedEnds.Count
                },
                Is.EqualTo(new object[] { new[] { 0, 5 }, 5, 5, 0 }));
        }

        [Test]
        public void Evaluate_BoardChanges_PreservesPreviousExplanation()
        {
            // Arrange
            Board board = CreateBoardWithBlockedEnd();
            PlayScore previousScore = PlayScoring.Evaluate(board, 2);
            board.Place(new Tile(4, 4, 5, SpecialPower.None), BoardEndPosition.Right, 3);
            // Act
            PlayScore nextScore = PlayScoring.Evaluate(board, 3);
            // Assert
            Assert.That(
                new object[]
                {
                    previousScore.EndValues,
                    previousScore.BlockedEnds,
                    previousScore.Points,
                    nextScore.EndValues,
                    nextScore.BlockedEnds.Count,
                    nextScore.Points
                },
                Is.EqualTo(new object[]
                {
                    new[] { 0 }, new[] { BoardEndPosition.Right }, 0, new[] { 0, 5 }, 0, 5
                }));
        }

        [TestCase(0, typeof(ArgumentNullException), "board")]
        [TestCase(1, typeof(ArgumentOutOfRangeException), "turnNumber")]
        [TestCase(2, typeof(InvalidOperationException), null)]
        public void Evaluate_InvalidBoardOrTurn_ReportsFailure(
            int scenario,
            Type exceptionType,
            string parameterName)
        {
            // Arrange
            Board board = new Board();
            int turnNumber = 0;
            if (scenario == 0)
            {
                board = null;
            }
            else if (scenario == 1)
            {
                board.Place(new Tile(1, 0, 0, SpecialPower.None), BoardEndPosition.Left, 0);
                turnNumber = -1;
            }

            Exception failure = null;
            // Act
            try
            {
                PlayScoring.Evaluate(board, turnNumber);
            }
            catch (ArgumentException ex)
            {
                failure = ex;
            }
            catch (InvalidOperationException ex)
            {
                failure = ex;
            }
            // Assert
            ArgumentException argumentFailure = failure as ArgumentException;
            Assert.That(
                new object[] { failure?.GetType(), argumentFailure?.ParamName },
                Is.EqualTo(new object[] { exceptionType, parameterName }));
        }

        private static Board CreateBoardWithBlockedEnd()
        {
            Board board = new Board();
            board.Place(new Tile(1, 5, 2, SpecialPower.None), BoardEndPosition.Left, 0);
            board.Place(new Tile(2, 2, 4, SpecialPower.Block), BoardEndPosition.Right, 1);
            board.Place(new Tile(3, 0, 5, SpecialPower.None), BoardEndPosition.Left, 2);
            return board;
        }
    }
}
