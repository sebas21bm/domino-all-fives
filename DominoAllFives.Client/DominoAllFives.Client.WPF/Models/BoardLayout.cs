using System;
using System.Collections.Generic;
using System.Windows;
using DominoAllFives.Contracts.Gameplay;

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
                List<TilePosition> layout = TryArrange(board, new Size(Width * scale,
                    Height * scale));
                if (layout != null)
                {
                    return layout;
                }
            }

            throw new InvalidOperationException("The board could not be laid out without overlap.");
        }

        private static List<TilePosition> TryArrange(PublicBoard board, Size area)
        {
            List<TilePosition> positions = new List<TilePosition>();
            if (board.Tiles.Count == 0)
            {
                return positions;
            }

            List<Rect> occupied = new List<Rect>
            {
                new Rect(-BorderThickness, -BorderThickness, area.Width + BorderThickness * 2,
                    BorderThickness),
                new Rect(-BorderThickness, area.Height, area.Width + BorderThickness * 2,
                    BorderThickness),
                new Rect(-BorderThickness, 0, BorderThickness, area.Height),
                new Rect(area.Width, 0, BorderThickness, area.Height)
            };
            Dictionary<BoardEnd, Cursor> cursors = new Dictionary<BoardEnd, Cursor>();
            DominoTile first = board.Tiles[0].Tile;
            bool isDouble = first.FirstValue == first.SecondValue;
            Point center = new Point(area.Width / 2, area.Height / 2);
            AddPosition(positions, first, center, isDouble ? 0 : -QuarterTurn);
            Rect opening = new Rect(center.X - (isDouble ? TileWidth : TileLength) / 2,
                center.Y - (isDouble ? TileLength : TileWidth) / 2, isDouble
                ? TileWidth : TileLength, isDouble ? TileLength : TileWidth);
            occupied.Add(opening);
            cursors.Add(BoardEnd.Left, new Cursor(new Point(opening.Left, center.Y), new Vector(
                -1, 0)));
            cursors.Add(BoardEnd.Right, new Cursor(new Point(opening.Right, center.Y),
                new Vector(1, 0)));
            cursors.Add(BoardEnd.Top, new Cursor(new Point(center.X, opening.Top), new Vector(0,
                -1)));
            cursors.Add(BoardEnd.Bottom, new Cursor(new Point(center.X, opening.Bottom),
                new Vector(0, 1)));
            for (int tileIndex = 1; tileIndex < board.Tiles.Count; tileIndex++)
            {
                if (!Place(board.Tiles[tileIndex], cursors, positions, occupied))
                {
                    return null;
                }
            }

            return positions;
        }

        private static bool Place(PlacedTile placement, Dictionary<BoardEnd, Cursor> cursors,
            List<TilePosition> positions, List<Rect> occupied)
        {
            Cursor cursor = cursors[placement.End];
            bool isDouble = placement.Tile.FirstValue == placement.Tile.SecondValue;
            double length = isDouble ? TileWidth : TileLength;
            double breadth = isDouble ? TileLength : TileWidth;
            for (int directionIndex = 0; directionIndex < DirectionCount; directionIndex++)
            {
                Point center = cursor.Anchor + cursor.Direction * (length / 2 + Gap);
                bool horizontal = cursor.Direction.X != 0;
                Rect bounds = new Rect(center.X - (horizontal ? length : breadth) / 2,
                    center.Y - (horizontal ? breadth : length) / 2, horizontal
                    ? length : breadth, horizontal ? breadth : length);
                if (Fits(bounds, occupied))
                {
                    double angle = Math.Atan2(cursor.Direction.Y,
                        cursor.Direction.X) * HalfTurn / Math.PI - QuarterTurn;
                    AddPosition(positions, placement.Tile, center, isDouble ? angle
                        + QuarterTurn : angle);
                    TilePosition position = positions[positions.Count - 1];
                    position.TopValue = placement.ConnectingValue;
                    position.BottomValue = placement.ExposedValue;
                    occupied.Add(bounds);
                    cursor.Anchor = center + cursor.Direction * (length / 2);
                    return true;
                }

                // Screen Y grows downward: (y, -x) turns counterclockwise.
                cursor.Direction = new Vector(cursor.Direction.Y, -cursor.Direction.X);
                cursor.Anchor += cursor.Direction * (breadth / 2 + Gap);
            }

            return false;
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

        private static void AddPosition(List<TilePosition> positions, DominoTile tile,
            Point center, double angle)
        {
            positions.Add(new TilePosition { X = center.X - TileWidth / 2, Y =
                center.Y - TileLength / 2, Angle = angle, TopValue = tile.FirstValue,
                BottomValue = tile.SecondValue, TileType = (DominoTileType)tile.SpecialPower });
        }

        private sealed class Cursor
        {
            public Point Anchor { get; set; }
            public Vector Direction { get; set; }

            public Cursor(Point anchor, Vector direction)
            {
                Anchor = anchor;
                Direction = direction;
            }
        }
    }
}
