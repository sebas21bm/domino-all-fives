using System;
using System.Collections.Generic;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    internal sealed class RoundSetup
    {
        private const int MinimumPlayers = 2;
        private const int MaximumPlayers = 4;
        private const int TwoPlayerHandSize = 7;
        private const int MultiplayerHandSize = 5;
        private const int DeckSize = 28;
        private const int MaximumSpecialTiles = 5;
        private const int DoubleRankOffset = 100;
        private const int PipValueRadix = Tile.MaximumPips + 1;

        internal PreparedRound Prepare(IReadOnlyList<int> playerIds, List<Tile> deck)
        {
            if (playerIds == null)
            {
                throw new ArgumentNullException(nameof(playerIds));
            }

            if (playerIds.Count < MinimumPlayers || playerIds.Count > MaximumPlayers)
            {
                throw new ArgumentException("Two to four players are required.", nameof(playerIds));
            }

            ValidateDeck(deck);
            List<MatchPlayer> players = CreatePlayers(playerIds);
            Queue<Tile> stock = new Queue<Tile>(deck);
            DealHands(players, stock);
            players.Sort(ComparePlayersByOpeningTile);
            MatchPlayer openingPlayer = players[0];
            Tile openingTile = FindHighestRankedTile(openingPlayer);
            return new PreparedRound(players, stock, openingTile.TileId);
        }

        private static List<MatchPlayer> CreatePlayers(IReadOnlyList<int> playerIds)
        {
            HashSet<int> uniqueIds = new HashSet<int>();
            List<MatchPlayer> players = new List<MatchPlayer>();
            foreach (int playerId in playerIds)
            {
                if (playerId <= 0 || !uniqueIds.Add(playerId))
                {
                    throw new ArgumentException(
                        "Distinct positive players are required.",
                        nameof(playerIds));
                }

                players.Add(new MatchPlayer { PlayerId = playerId });
            }

            return players;
        }

        private static void DealHands(List<MatchPlayer> players, Queue<Tile> stock)
        {
            int handSize = MultiplayerHandSize;
            if (players.Count == MinimumPlayers)
            {
                handSize = TwoPlayerHandSize;
            }

            foreach (MatchPlayer player in players)
            {
                player.Hand.Clear();
                for (int tileIndex = 0; tileIndex < handSize; tileIndex++)
                {
                    Tile tile = stock.Dequeue();
                    player.Hand.Add(tile);
                }
            }
        }

        private static Tile FindHighestRankedTile(MatchPlayer player)
        {
            Tile highestRankedTile = null;
            int highestRank = int.MinValue;
            foreach (Tile tile in player.Hand)
            {
                int tileRank = GetTileRank(tile);
                if (tileRank > highestRank)
                {
                    highestRank = tileRank;
                    highestRankedTile = tile;
                }
            }

            return highestRankedTile;
        }

        private static int GetHandRank(MatchPlayer player)
        {
            Tile highestRankedTile = FindHighestRankedTile(player);
            return GetTileRank(highestRankedTile);
        }

        private static int ComparePlayersByOpeningTile(MatchPlayer left, MatchPlayer right)
        {
            int leftRank = GetHandRank(left);
            int rightRank = GetHandRank(right);
            return rightRank.CompareTo(leftRank);
        }

        private static int GetTileRank(Tile tile)
        {
            if (tile.IsDouble)
            {
                return DoubleRankOffset + tile.FirstValue;
            }

            int highestValue = Math.Max(tile.FirstValue, tile.SecondValue);
            return tile.TotalPips * PipValueRadix + highestValue;
        }

        private static void ValidateDeck(List<Tile> deck)
        {
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck));
            }

            if (deck.Count != DeckSize)
            {
                throw new ArgumentException("A complete double-six deck is required.");
            }

            HashSet<int> tileIds = new HashSet<int>();
            HashSet<int> tileValues = new HashSet<int>();
            int specialTileCount = 0;
            foreach (Tile tile in deck)
            {
                if (tile == null)
                {
                    throw new ArgumentException("A complete double-six deck is required.");
                }

                tileIds.Add(tile.TileId);
                tileValues.Add(GetTileValueIdentity(tile));
                if (tile.SpecialPower != SpecialPower.None)
                {
                    specialTileCount++;
                }
            }

            bool hasUniqueIds = tileIds.Count == DeckSize;
            bool hasUniqueValues = tileValues.Count == DeckSize;
            bool hasValidSpecialTileCount = specialTileCount <= MaximumSpecialTiles;
            if (!hasUniqueIds || !hasUniqueValues || !hasValidSpecialTileCount)
            {
                throw new ArgumentException("Invalid deck.");
            }
        }

        private static int GetTileValueIdentity(Tile tile)
        {
            int firstValue = Math.Min(tile.FirstValue, tile.SecondValue);
            int secondValue = Math.Max(tile.FirstValue, tile.SecondValue);
            return firstValue * PipValueRadix + secondValue;
        }
    }
}
