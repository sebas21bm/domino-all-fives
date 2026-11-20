using System;
using System.Collections.Generic;
using System.Windows;
using MatchService;

namespace DominoAllFives.Client.WPF.Models
{
    public static class BoardLayout
    {
        private const int LayoutAttempts = 4;
        private const int DirectionCount = 4;
        private const double AreaGrowthFactor = 2;
        private const double BorderThickness = 100;
        private const double HalfTurn = 180;
        private const double QuarterTurn = 90;
        public const double Width = 2400;
        public const double Height = 1600;
        private const double TileLength = 96;
        private const double TileWidth = 48;
        private const double Gap = 4;

        public static List<TilePosition> Arrange(PublicBoard board)
        {
            for (int attempt = 0; attempt < LayoutAttempts; attempt++)
            {
                double scale = Math.Pow(AreaGrowthFactor, attempt);
                Size area = new Size(Width * scale, Height * scale);
                if (TryArrange(board, area, out List<TilePosition> layout))
                {
                    return layout;
                }
            }

            throw new InvalidOperationException("The board could not be laid out without overlap.");
        }

        private static bool TryArrange(
            PublicBoard board,
            Size area,
            out List<TilePosition> positions)
        {
            positions = new List<TilePosition>();
            if (board.Tiles.Count == 0)
            {
                return true;
            }

            List<Rect> occupied = CreateBorders(area);
            DominoTile first = board.Tiles[0].Tile;
            bool isDouble = first.FirstValue == first.SecondValue;
            Point center = new Point(area.Width / 2, area.Height / 2);
            double angle = isDouble ? 0 : -QuarterTurn;
            AddPosition(positions, first, center, angle);
            double openingWidth = isDouble ? TileWidth : TileLength;
            double openingHeight = isDouble ? TileLength : TileWidth;
            Rect openingBounds = GetBounds(center, openingWidth, openingHeight);
            occupied.Add(openingBounds);

            Dictionary<BoardEnd, Cursor> cursors = CreateCursors(center, openingBounds);
            for (int tileIndex = 1; tileIndex < board.Tiles.Count; tileIndex++)
            {
                if (!Place(board.Tiles[tileIndex], cursors, positions, occupied))
                {
                    return false;
                }
            }

            return true;
        }

        private static List<Rect> CreateBorders(Size area)
        {
            double fullWidth = area.Width + BorderThickness * 2;
            return new List<Rect>
            {
                new Rect(-BorderThickness, -BorderThickness, fullWidth, BorderThickness),
                new Rect(-BorderThickness, area.Height, fullWidth, BorderThickness),
                new Rect(-BorderThickness, 0, BorderThickness, area.Height),
                new Rect(area.Width, 0, BorderThickness, area.Height)
            };
        }

        private static Dictionary<BoardEnd, Cursor> CreateCursors(Point center, Rect opening)
        {
            Cursor left = new Cursor(new Point(opening.Left, center.Y), new Vector(-1, 0));
            Cursor right = new Cursor(new Point(opening.Right, center.Y), new Vector(1, 0));
            Cursor top = new Cursor(new Point(center.X, opening.Top), new Vector(0, -1));
            Cursor bottom = new Cursor(new Point(center.X, opening.Bottom), new Vector(0, 1));
            return new Dictionary<BoardEnd, Cursor>
            {
                { BoardEnd.Left, left },
                { BoardEnd.Right, right },
                { BoardEnd.Top, top },
                { BoardEnd.Bottom, bottom }
            };
        }

        private static bool Place(
            PlacedTile placement,
            Dictionary<BoardEnd, Cursor> cursors,
            List<TilePosition> positions,
            List<Rect> occupied)
        {
            Cursor cursor = cursors[placement.End];
            bool isDouble = placement.Tile.FirstValue == placement.Tile.SecondValue;
            double length = isDouble ? TileWidth : TileLength;
            double breadth = isDouble ? TileLength : TileWidth;
            for (int directionIndex = 0; directionIndex < DirectionCount; directionIndex++)
            {
                Point center = cursor.Anchor + cursor.Direction * (length / 2 + Gap);
                bool isHorizontal = cursor.Direction.X != 0;
                double width = isHorizontal ? length : breadth;
                double height = isHorizontal ? breadth : length;
                Rect bounds = GetBounds(center, width, height);
                if (Fits(bounds, occupied))
                {
                    double angle = GetTileAngle(cursor.Direction, isDouble);
                    AddPosition(positions, placement.Tile, center, angle);
                    TilePosition position = positions[positions.Count - 1];
                    position.TopValue = placement.ConnectingValue;
                    position.BottomValue = placement.ExposedValue;
                    occupied.Add(bounds);
                    cursor.Anchor = center + cursor.Direction * (length / 2);
                    return true;
                }

                cursor.TurnCounterclockwise();
                cursor.Anchor += cursor.Direction * (breadth / 2 + Gap);
            }

            return false;
        }

        private static Rect GetBounds(Point center, double width, double height)
        {
            return new Rect(center.X - width / 2, center.Y - height / 2, width, height);
        }

        private static double GetTileAngle(Vector direction, bool isDouble)
        {
            double angle = Math.Atan2(direction.Y, direction.X) * HalfTurn / Math.PI - QuarterTurn;
            if (isDouble)
            {
                angle += QuarterTurn;
            }

            return angle;
        }

        private static bool Fits(Rect bounds, List<Rect> occupied)
        {
            foreach (Rect previous in occupied)
            {
                if (bounds.IntersectsWith(previous))
                {
                    return false;
                }
            }

            return true;
        }

        private static void AddPosition(
            List<TilePosition> positions,
            DominoTile tile,
            Point center,
            double angle)
        {
            TilePosition position = new TilePosition
            {
                X = center.X - TileWidth / 2,
                Y = center.Y - TileLength / 2,
                Angle = angle,
                TopValue = tile.FirstValue,
                BottomValue = tile.SecondValue,
                TileType = (DominoTileType)tile.SpecialPower
            };
            positions.Add(position);
        }

        private sealed class Cursor
        {
            public Point Anchor { get; set; }
            public Vector Direction { get; private set; }

            public Cursor(Point anchor, Vector direction)
            {
                Anchor = anchor;
                Direction = direction;
            }

            public void TurnCounterclockwise()
            {
                Direction = new Vector(Direction.Y, -Direction.X);
            }
        }
    }
}
