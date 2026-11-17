using System;
using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Gameplay;

namespace DominoAllFives.Server.Tests
{
    internal static class MatchTestDeck
    {
        public static List<Tile> Ordered(params int[] firstValues)
        {
            List<Tile> remaining = new List<Tile>();
            for (int first = 0; first <= Tile.MaximumPips; first++)
            {
                for (int second = first; second <= Tile.MaximumPips; second++)
                {
                    Tile tile = new Tile(remaining.Count + 1, first, second, SpecialPower.None);
                    remaining.Add(tile);
                }
            }

            List<Tile> ordered = new List<Tile>();
            foreach (int value in firstValues)
            {
                int index = FindValueIndex(remaining, value);
                ordered.Add(remaining[index]);
                remaining.RemoveAt(index);
            }

            ordered.AddRange(remaining);
            return ordered;
        }

        private static int FindValueIndex(List<Tile> tiles, int value)
        {
            for (int index = 0; index < tiles.Count; index++)
            {
                Tile tile = tiles[index];
                if (tile.FirstValue * 10 + tile.SecondValue == value)
                {
                    return index;
                }
            }

            throw new ArgumentException("The test deck contains an invalid or repeated value.");
        }
    }
}
