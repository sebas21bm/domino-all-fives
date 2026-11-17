using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class Match
    {
        private const int MinimumPlayers = 2;
        private const int MaximumPlayers = 4;
        private const int TwoPlayerHandSize = 7;
        private const int MultiplayerHandSize = 5;
        private const int DeckSize = 28;
        private const int MaximumSpecialTiles = 5;
        private const int DoublePointsMultiplier = 2;
        private const int ScoringMultiple = 5;
        private const int RoundingOffset = 2;
        private const int DoubleRankOffset = 100;
        private const int PipValueRadix = Tile.MaximumPips + 1;
        private static readonly int[] allowedTargets =
        {
            100,
            150,
            200,
            250
        };
        private readonly List<MatchPlayer> _players;
        private readonly Func<List<Tile>> _createDeck;
        private Queue<Tile> _stock;
        private int _currentPlayerIndex;
        private int _consecutivePasses;

        public int MatchId { get; }
        public int TargetPoints { get; }
        public int RoundNumber { get; private set; }
        public int OpeningTileId { get; private set; }
        public int TurnNumber { get; private set; }
        public long Version { get; private set; }
        public Board Board { get; private set; }
        public MatchState Phase { get; private set; }
        public RoundSummary LastRound { get; private set; }
        public PlayScore LastPlay { get; private set; }

        public int StockCount
        {
            get
            {
                return _stock.Count;
            }
        }

        public IReadOnlyList<MatchPlayer> Players
        {
            get
            {
                return _players.AsReadOnly();
            }
        }

        public int? CurrentPlayerId
        {
            get
            {
                if (Phase != MatchState.InProgress)
                {
                    return null;
                }

                MatchPlayer currentPlayer = _players[_currentPlayerIndex];
                return currentPlayer.PlayerId;
            }
        }

        public Match(
            int matchId,
            IEnumerable<int> playerIds,
            int targetPoints,
            Func<List<Tile>> createDeck)
        {
            if (matchId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(matchId));
            }

            if (playerIds == null)
            {
                throw new ArgumentNullException(nameof(playerIds));
            }

            List<int> ids = playerIds.ToList();
            ValidatePlayerIds(ids, nameof(playerIds));
            if (!allowedTargets.Contains(targetPoints))
            {
                throw new ArgumentOutOfRangeException(nameof(targetPoints));
            }

            MatchId = matchId;
            TargetPoints = targetPoints;
            _players = CreatePlayers(ids);
            _createDeck = createDeck
                ?? throw new ArgumentNullException(nameof(createDeck));
            BeginRound();
        }

        public MatchPlayer GetPlayer(int playerId)
        {
            MatchPlayer player = _players.SingleOrDefault(
                candidate => candidate.PlayerId == playerId);
            if (player == null)
            {
                throw new MatchRuleException(MatchIssue.PlayerNotInMatch);
            }

            return player;
        }

        public void Play(
            int playerId,
            int tileId,
            BoardEndPosition end,
            long expectedVersion)
        {
            MatchPlayer player = ValidateTurn(playerId, expectedVersion);
            Tile tile = player.Hand.SingleOrDefault(
                candidate => candidate.TileId == tileId);
            if (tile == null)
            {
                throw new MatchRuleException(MatchIssue.TileNotOwned);
            }

            if (Board.Tiles.Count == 0
                && tileId != OpeningTileId)
            {
                throw new MatchRuleException(MatchIssue.OpeningTileRequired);
            }

            Board.Place(tile, end, TurnNumber);
            player.Hand.Remove(tile);

            int basePoints = Board.CalculatePoints(TurnNumber);
            int awardedPoints = CalculateAwardedPlayPoints(tile, basePoints);
            player.Points += awardedPoints;

            LastPlay = CreatePlayScore(
                playerId,
                tile,
                end,
                basePoints);
            _consecutivePasses = 0;
            Version++;

            if (player.Points >= TargetPoints)
            {
                Phase = MatchState.Completed;
                return;
            }

            if (player.Hand.Count == 0)
            {
                FinishRound(player.PlayerId);
                LastPlay.RoundPoints = LastRound.AwardedPoints;
                return;
            }

            AdvanceTurn();
        }

        public void Draw(int playerId, long expectedVersion)
        {
            MatchPlayer player = ValidateTurn(playerId, expectedVersion);
            bool playerHasMove = Board.HasMove(player.Hand, TurnNumber);
            bool stockIsEmpty = _stock.Count == 0;
            if (playerHasMove || stockIsEmpty)
            {
                throw new MatchRuleException(MatchIssue.DrawNotAllowed);
            }

            do
            {
                Tile drawnTile = _stock.Dequeue();
                player.Hand.Add(drawnTile);
                playerHasMove = Board.HasMove(player.Hand, TurnNumber);
                stockIsEmpty = _stock.Count == 0;
            }
            while (!stockIsEmpty && !playerHasMove);

            Version++;
        }

        public void Pass(int playerId, long expectedVersion)
        {
            MatchPlayer player = ValidateTurn(playerId, expectedVersion);
            bool stockHasTiles = _stock.Count > 0;
            bool playerHasMove = Board.HasMove(player.Hand, TurnNumber);
            if (stockHasTiles || playerHasMove)
            {
                throw new MatchRuleException(MatchIssue.PassNotAllowed);
            }

            _consecutivePasses++;
            Version++;
            AdvanceTurn();

            bool everyPlayerPassed = _consecutivePasses >= _players.Count;
            bool anyPlayerHasMove = AnyPlayerHasMove();
            if (everyPlayerPassed && !anyPlayerHasMove)
            {
                int minimumRemainingPips = FindMinimumRemainingPips();
                List<MatchPlayer> roundWinners = FindPlayersWithRemainingPips(
                    minimumRemainingPips);
                int? winnerId = GetSingleWinnerId(roundWinners);
                FinishRound(winnerId);
            }
        }

        public void Cancel(int playerId)
        {
            GetPlayer(playerId);
            if (Phase != MatchState.InProgress)
            {
                return;
            }

            Phase = MatchState.Cancelled;
            Version++;
        }

        private static List<MatchPlayer> CreatePlayers(IEnumerable<int> playerIds)
        {
            List<MatchPlayer> players = new List<MatchPlayer>();
            foreach (int playerId in playerIds)
            {
                MatchPlayer player = new MatchPlayer
                {
                    PlayerId = playerId
                };
                players.Add(player);
            }

            return players;
        }

        private static void ValidatePlayerIds(List<int> playerIds, string parameterName)
        {
            bool hasValidPlayerCount = playerIds.Count >= MinimumPlayers
                && playerIds.Count <= MaximumPlayers;
            HashSet<int> uniquePlayerIds = new HashSet<int>();
            bool hasInvalidPlayerId = false;
            foreach (int playerId in playerIds)
            {
                if (playerId <= 0)
                {
                    hasInvalidPlayerId = true;
                }

                uniquePlayerIds.Add(playerId);
            }

            bool hasUniquePlayerIds = uniquePlayerIds.Count == playerIds.Count;
            if (!hasValidPlayerCount
                || hasInvalidPlayerId
                || !hasUniquePlayerIds)
            {
                throw new ArgumentException(
                    "Two to four distinct players are required.",
                    parameterName);
            }
        }

        private MatchPlayer ValidateTurn(int playerId, long expectedVersion)
        {
            MatchPlayer player = GetPlayer(playerId);
            if (Phase != MatchState.InProgress)
            {
                throw new MatchRuleException(MatchIssue.MatchNotInProgress);
            }

            if (expectedVersion != Version)
            {
                throw new MatchRuleException(MatchIssue.StaleVersion);
            }

            if (CurrentPlayerId != playerId)
            {
                throw new MatchRuleException(MatchIssue.NotPlayersTurn);
            }

            return player;
        }

        private void AdvanceTurn()
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
            TurnNumber++;
        }

        private static int CalculateAwardedPlayPoints(Tile tile, int basePoints)
        {
            int awardedPoints = basePoints;
            if (tile.SpecialPower == SpecialPower.DoublePoints)
            {
                awardedPoints = basePoints * DoublePointsMultiplier;
            }

            return awardedPoints;
        }

        private PlayScore CreatePlayScore(
            int playerId,
            Tile tile,
            BoardEndPosition end,
            int basePoints)
        {
            int multiplier = 1;
            if (tile.SpecialPower == SpecialPower.DoublePoints)
            {
                multiplier = DoublePointsMultiplier;
            }

            int[] scoringValues = Board.GetScoringValues(TurnNumber);
            List<BoardEndPosition> blockedEnds = new List<BoardEndPosition>();
            bool includesOpeningDouble = false;
            foreach (BoardEndState boardEnd in Board.Ends)
            {
                if (boardEnd.BlockedTurn == TurnNumber)
                {
                    blockedEnds.Add(boardEnd.End);
                }

                if (boardEnd.IsOpeningEnd
                    && boardEnd.BlockedTurn != TurnNumber)
                {
                    includesOpeningDouble = true;
                }
            }

            int total = 0;
            foreach (int scoringValue in scoringValues)
            {
                total += scoringValue;
            }

            return new PlayScore
            {
                PlayerId = playerId,
                RoundNumber = RoundNumber,
                Tile = tile,
                PlayedEnd = end,
                EndValues = Array.AsReadOnly(scoringValues),
                BlockedEnds = blockedEnds.AsReadOnly(),
                IncludesOpeningDouble = includesOpeningDouble,
                Total = total,
                BasePoints = basePoints,
                Multiplier = multiplier,
                Points = basePoints * multiplier
            };
        }

        private bool AnyPlayerHasMove()
        {
            foreach (MatchPlayer player in _players)
            {
                if (Board.HasMove(player.Hand, TurnNumber))
                {
                    return true;
                }
            }

            return false;
        }

        private int FindMinimumRemainingPips()
        {
            int minimumRemainingPips = int.MaxValue;
            foreach (MatchPlayer player in _players)
            {
                int remainingPips = CalculateRemainingPips(player);
                if (remainingPips < minimumRemainingPips)
                {
                    minimumRemainingPips = remainingPips;
                }
            }

            return minimumRemainingPips;
        }

        private List<MatchPlayer> FindPlayersWithRemainingPips(int remainingPips)
        {
            List<MatchPlayer> players = new List<MatchPlayer>();
            foreach (MatchPlayer player in _players)
            {
                int playerRemainingPips = CalculateRemainingPips(player);
                if (playerRemainingPips == remainingPips)
                {
                    players.Add(player);
                }
            }

            return players;
        }

        private static int? GetSingleWinnerId(List<MatchPlayer> winners)
        {
            int? winnerId = null;
            if (winners.Count == 1)
            {
                MatchPlayer winner = winners[0];
                winnerId = winner.PlayerId;
            }

            return winnerId;
        }

        private static int CalculateRemainingPips(MatchPlayer player)
        {
            int remainingPips = 0;
            foreach (Tile tile in player.Hand)
            {
                remainingPips += tile.TotalPips;
            }

            return remainingPips;
        }

        private void FinishRound(int? winnerId)
        {
            int totalRemainingPips = 0;
            Dictionary<int, int> scores = new Dictionary<int, int>();
            Dictionary<int, Tile[]> remainingHands = new Dictionary<int, Tile[]>();

            foreach (MatchPlayer player in _players)
            {
                totalRemainingPips += CalculateRemainingPips(player);
                scores.Add(player.PlayerId, player.Points);
                remainingHands.Add(player.PlayerId, player.Hand.ToArray());
            }

            int awardedPoints = 0;
            if (winnerId.HasValue)
            {
                awardedPoints = CalculateRoundedRoundPoints(totalRemainingPips);
                MatchPlayer winner = GetPlayer(winnerId.Value);
                winner.Points += awardedPoints;
                scores[winner.PlayerId] = winner.Points;
            }

            LastRound = new RoundSummary
            {
                RoundNumber = RoundNumber,
                WinnerId = winnerId,
                AwardedPoints = awardedPoints,
                Scores = scores,
                RemainingHands = remainingHands
            };

            if (HasPlayerReachedTarget())
            {
                Phase = MatchState.Completed;
                return;
            }

            BeginRound();
        }

        private static int CalculateRoundedRoundPoints(int totalRemainingPips)
        {
            return ((totalRemainingPips + RoundingOffset) / ScoringMultiple)
                * ScoringMultiple;
        }

        private bool HasPlayerReachedTarget()
        {
            foreach (MatchPlayer player in _players)
            {
                if (player.Points >= TargetPoints)
                {
                    return true;
                }
            }

            return false;
        }

        private void BeginRound()
        {
            List<Tile> deck = _createDeck();
            ValidateDeck(deck);
            _stock = new Queue<Tile>(deck);

            int handSize = MultiplayerHandSize;
            if (_players.Count == MinimumPlayers)
            {
                handSize = TwoPlayerHandSize;
            }

            foreach (MatchPlayer player in _players)
            {
                player.Hand.Clear();
                for (int tileIndex = 0; tileIndex < handSize; tileIndex++)
                {
                    Tile tile = _stock.Dequeue();
                    player.Hand.Add(tile);
                }
            }

            _players.Sort(ComparePlayersByOpeningTile);
            MatchPlayer openingPlayer = _players[0];
            Tile openingTile = FindHighestRankedTile(openingPlayer);
            OpeningTileId = openingTile.TileId;
            Board = new Board();
            RoundNumber++;
            TurnNumber = 0;
            _currentPlayerIndex = 0;
            _consecutivePasses = 0;
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

        private static int ComparePlayersByOpeningTile(
            MatchPlayer left,
            MatchPlayer right)
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
            if (deck == null
                || deck.Count != DeckSize)
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
            if (!hasUniqueIds
                || !hasUniqueValues
                || !hasValidSpecialTileCount)
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
