using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DominoAllFives.Client.WPF.Models;
using MatchService;
using NUnit.Framework;

namespace DominoAllFives.Client.Tests
{
    [TestFixture]
    public sealed class BoardLayoutTests
    {
        [TestCase(BoardEnd.Left)]
        [TestCase(BoardEnd.Right)]
        [TestCase(BoardEnd.Top)]
        [TestCase(BoardEnd.Bottom)]
        public void Arrange_LongEnd_BendsWithoutOverlaps(BoardEnd end)
        {
            // Arrange
            PublicBoard board = new PublicBoard
            {
                Tiles = new List<PlacedTile>()
            };
            for (int tileIndex = 0; tileIndex < 28; tileIndex++)
            {
                board.Tiles.Add(new PlacedTile { Tile = new DominoTile { TileId = tileIndex + 1,
                    FirstValue = 3, SecondValue = tileIndex == 0 ? 3 : 5 }, End = end,
                    ConnectingValue = 3, ExposedValue = 5 });
            }

            // Act
            List<TilePosition> positions = BoardLayout.Arrange(board);
            // Assert
            Assert.That(positions.Count, Is.EqualTo(28));
            List<Rect> rectangles = positions.Select(position => Bounds(position))
                .ToList();
            for (int tileIndex = 0; tileIndex < rectangles.Count; tileIndex++)
            {
                Assert.That(rectangles[tileIndex].Left, Is.GreaterThanOrEqualTo(0));
                Assert.That(rectangles[tileIndex].Top, Is.GreaterThanOrEqualTo(0));
                for (int otherIndex = tileIndex + 1; otherIndex < rectangles.Count; otherIndex++)
                {
                    Assert.That(rectangles[tileIndex].IntersectsWith(rectangles[otherIndex]),
                        Is.False);
                }
            }
        }

        [Test]
        public void Arrange_FourEnds_PreservesAllTilesAcrossMixedLayouts()
        {
            // Arrange
            Random random = new Random(42);
            for (int scenario = 0; scenario < 100; scenario++)
            {
                PublicBoard board = new PublicBoard
                {
                    Tiles = new List<PlacedTile>()
                };
                board.Tiles.Add(new PlacedTile { Tile = new DominoTile { TileId = 1,
                    FirstValue = 3, SecondValue = 3 } });
                for (int tileIndex = 1; tileIndex < 28; tileIndex++)
                {
                    board.Tiles.Add(new PlacedTile { Tile = new DominoTile { TileId = tileIndex
                        + 1, FirstValue = 3, SecondValue = random.Next(7) }, End = (
                        BoardEnd)random.Next(4), ConnectingValue = 3, ExposedValue = 3 });
                }

                // Act
                List<TilePosition> positions = BoardLayout.Arrange(board);
                // Assert
                Assert.That(positions.Count, Is.EqualTo(28));
                List<Rect> rectangles = positions.Select(Bounds)
                    .ToList();
                for (int tileIndex = 0; tileIndex < rectangles.Count; tileIndex++)
                {
                    for (int otherIndex = tileIndex
                        + 1; otherIndex < rectangles.Count; otherIndex++)
                    {
                        Assert.That(rectangles[tileIndex].IntersectsWith(
                            rectangles[otherIndex]), Is.False);
                    }
                }
            }
        }

        private static Rect Bounds(TilePosition position)
        {
            bool horizontal = Math.Abs(position.Angle % 180) == 90;
            double width = horizontal ? 96 : 48;
            double height = horizontal ? 48 : 96;
            return new Rect(position.X + 24 - width / 2, position.Y + 48 - height / 2, width,
                height);
        }
    }
}
