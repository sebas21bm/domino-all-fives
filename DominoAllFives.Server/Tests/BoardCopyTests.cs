using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Gameplay;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class BoardCopyTests
    {
        [Test]
        public void Copy_EmptyBoard_ReturnsIndependentEmptyBoard()
        {
            // Arrange
            Board board = new Board();
            // Act
            Board copy = board.Copy();
            // Assert
            Assert.That(
                new object[]
                {
                    ReferenceEquals(board, copy), copy.Tiles.Count, copy.Ends.Count
                },
                Is.EqualTo(new object[] { false, 0, 0 }));
        }

        [TestCase(SpecialPower.None)]
        [TestCase(SpecialPower.Block)]
        [TestCase(SpecialPower.DoublePoints)]
        public void Copy_ExistingBoard_PreservesEveryField(SpecialPower power)
        {
            // Arrange
            Board board = CreateBoard(power);
            // Act
            Board copy = board.Copy();
            // Assert
            Assert.That(
                new object[]
                {
                    Capture(copy), copy.GetScoringValues(0), copy.GetScoringValues(1)
                },
                Is.EqualTo(new object[]
                {
                    Capture(board), board.GetScoringValues(0), board.GetScoringValues(1)
                }));
        }

        [TestCase(SpecialPower.None)]
        [TestCase(SpecialPower.Block)]
        [TestCase(SpecialPower.DoublePoints)]
        public void Copy_PlacementOnCopy_PreservesOriginalBoard(SpecialPower power)
        {
            // Arrange
            Board board = CreateBoard(power);
            int[] before = Capture(board);
            Board copy = board.Copy();
            Tile tile = new Tile(2, 5, 6, SpecialPower.Block);
            // Act
            copy.Place(tile, BoardEndPosition.Right, 1);
            // Assert
            Assert.That(
                new object[]
                {
                    Capture(board), copy.Tiles.Count
                },
                Is.EqualTo(new object[] { before, 2 }));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Copy_ChangedMutableRecord_PreservesOriginalRecord(bool changeEnd)
        {
            // Arrange
            Board board = CreateBoard(SpecialPower.Block);
            int[] before = Capture(board);
            Board copy = board.Copy();
            // Act
            if (changeEnd)
            {
                copy.Ends[0].Value = 99;
                copy.Ends[0].BlockedTurn = 99;
            }
            else
            {
                copy.Tiles[0].ExposedValue = 99;
                copy.Tiles[0].ConnectingValue = 99;
            }
            // Assert
            Assert.That(Capture(board), Is.EqualTo(before));
        }

        private static Board CreateBoard(SpecialPower power)
        {
            Board board = new Board();
            board.Place(new Tile(1, 6, 6, power), BoardEndPosition.Left, 0);
            return board;
        }

        private static int[] Capture(Board board)
        {
            List<int> values = new List<int>();
            foreach (Placement placement in board.Tiles)
            {
                values.Add(placement.Tile.TileId);
                values.Add((int)placement.End);
                values.Add(placement.ConnectingValue);
                values.Add(placement.ExposedValue);
            }

            foreach (BoardEndState end in board.Ends)
            {
                values.Add((int)end.End);
                values.Add(end.Value);
                values.Add(end.ScoreValue);
                values.Add(end.BlockedTurn);
                values.Add(System.Convert.ToInt32(end.IsOpeningEnd));
            }

            return values.ToArray();
        }
    }
}
