using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public static class Deck
    {
        private const int PowerOptions = 2;
        private const int SpecialTileCount = 5;

        public static List<Tile> Create()
        {
            List<Tile> tiles = new List<Tile>();
            for (int firstValue = 0; firstValue <= Tile.MaximumPips; firstValue++)
            {
                for (int secondValue = firstValue; secondValue <= Tile.MaximumPips; secondValue++)
                {
                    Tile tile = new Tile(
                        tiles.Count + 1,
                        firstValue,
                        secondValue,
                        SpecialPower.None);
                    tiles.Add(tile);
                }
            }

            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                Shuffle(tiles, random);
                for (int tileIndex = 0; tileIndex < SpecialTileCount; tileIndex++)
                {
                    Tile tile = tiles[tileIndex];
                    SpecialPower power = SpecialPower.Block;
                    if (Next(random, PowerOptions) == 0)
                    {
                        power = SpecialPower.DoublePoints;
                    }

                    tiles[tileIndex] = new Tile(
                        tile.TileId,
                        tile.FirstValue,
                        tile.SecondValue,
                        power);
                }

                Shuffle(tiles, random);
            }

            return tiles;
        }

        private static void Shuffle(List<Tile> tiles, RandomNumberGenerator random)
        {
            for (int tileIndex = tiles.Count - 1; tileIndex > 0; tileIndex--)
            {
                int otherIndex = Next(random, tileIndex + 1);
                Tile saved = tiles[tileIndex];
                tiles[tileIndex] = tiles[otherIndex];
                tiles[otherIndex] = saved;
            }
        }

        private static int Next(RandomNumberGenerator random, int exclusiveMaximum)
        {
            byte[] bytes = new byte[sizeof(uint)];
            uint limit = uint.MaxValue - uint.MaxValue % (uint)exclusiveMaximum;
            uint value;
            do
            {
                random.GetBytes(bytes);
                value = BitConverter.ToUInt32(bytes, 0);
            }
            while (value >= limit);
            return (int)(value % exclusiveMaximum);
        }
    }
}
