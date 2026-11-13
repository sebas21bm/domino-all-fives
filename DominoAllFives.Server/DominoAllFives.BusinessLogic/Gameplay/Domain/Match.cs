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
        public int MatchId { get; private set; }
        public int TargetPoints { get; private set; }
        public int RoundNumber { get; private set; }
        public int OpeningTileId { get; private set; }
        public int TurnNumber { get; private set; }
        public long Version { get; private set; }
        public Board Board { get; private set; }
        public MatchPhase Phase { get; private set; }
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
                return Phase == MatchPhase.InProgress ? (int
                    ? )_players[_currentPlayerIndex].PlayerId : null;
            }
        }

        public Match(int matchId, IEnumerable<int> playerIds, int targetPoints,
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
            if (ids.Count < MinimumPlayers || ids.Count > MaximumPlayers || ids.Distinct()
                .Count() != ids.Count || ids.Any(id => id <= 0))
            {
                throw new ArgumentException("Two to four distinct players are required.",
                    nameof(playerIds));
            }

            if (!allowedTargets.Contains(targetPoints))
            {
                throw new ArgumentOutOfRangeException(nameof(targetPoints));
            }

            MatchId = matchId;
            TargetPoints = targetPoints;
            _players = ids.Select(id => new MatchPlayer { PlayerId = id })
                .ToList();
            _createDeck = createDeck ?? throw new ArgumentNullException(nameof(createDeck));
            BeginRound();
        }

        public MatchPlayer GetPlayer(int playerId)
        {
            MatchPlayer player = _players.SingleOrDefault(candidate
                => candidate.PlayerId == playerId);
            if (player == null)
            {
                throw new MatchRuleException(MatchIssue.PlayerNotInMatch);
            }

            return player;
        }

        public void Play(int playerId, int tileId, End end, long expectedVersion)
        {
            MatchPlayer player = ValidateTurn(playerId, expectedVersion);
            Tile tile = player.Hand.SingleOrDefault(candidate => candidate.TileId == tileId);
            if (tile == null)
            {
                throw new MatchRuleException(MatchIssue.TileNotOwned);
            }

            if (Board.Tiles.Count == 0 && tileId != OpeningTileId)
            {
                throw new MatchRuleException(MatchIssue.OpeningTileRequired);
            }

            Board.Place(tile, end, TurnNumber);
            player.Hand.Remove(tile);
            int points = Board.CalculatePoints(TurnNumber);
            player.Points += tile.SpecialPower == SpecialPower.DoublePoints
                ? points * DoublePointsMultiplier : points;
            LastPlay = CreatePlayScore(playerId, tile, end, points);
            _consecutivePasses = 0;
            Version++;
            if (player.Points >= TargetPoints)
            {
                Phase = MatchPhase.Completed;
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
            if (Board.HasMove(player.Hand, TurnNumber) || _stock.Count == 0)
            {
                throw new MatchRuleException(MatchIssue.DrawNotAllowed);
            }

            do
            {
                player.Hand.Add(_stock.Dequeue());
            }
            while (_stock.Count > 0 && !Board.HasMove(player.Hand, TurnNumber));
            Version++;
        }

        public void Pass(int playerId, long expectedVersion)
        {
            MatchPlayer player = ValidateTurn(playerId, expectedVersion);
            if (_stock.Count > 0 || Board.HasMove(player.Hand, TurnNumber))
            {
                throw new MatchRuleException(MatchIssue.PassNotAllowed);
            }

            _consecutivePasses++;
            Version++;
            AdvanceTurn();
            // A temporary block expiring can make a previously impossible move legal.
            bool hasAnyMove = _players.Any(candidate => Board.HasMove(candidate.Hand, TurnNumber));
            if (_consecutivePasses >= _players.Count && !hasAnyMove)
            {
                int minimum = _players.Min(candidate => candidate.Hand.Sum(tile => tile.TotalPips));
                List<MatchPlayer> winners = _players.Where(candidate => candidate.Hand.Sum(tile
                    => tile.TotalPips) == minimum)
                    .ToList();
                FinishRound(winners.Count == 1 ? (int? )winners[0].PlayerId : null);
            }
        }

        public void Cancel(int playerId)
        {
            GetPlayer(playerId);
            if (Phase != MatchPhase.InProgress)
            {
                return;
            }

            Phase = MatchPhase.Cancelled;
            Version++;
        }

        private MatchPlayer ValidateTurn(int playerId, long expectedVersion)
        {
            MatchPlayer player = GetPlayer(playerId);
            if (Phase != MatchPhase.InProgress)
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

        private PlayScore CreatePlayScore(int playerId, Tile tile, End end, int points)
        {
            int multiplier = tile.SpecialPower == SpecialPower.DoublePoints
                ? DoublePointsMultiplier : 1;
            int[] values = Board.GetScoringValues(TurnNumber);
            return new PlayScore
            {
                PlayerId = playerId,
                RoundNumber = RoundNumber,
                Tile = tile,
                PlayedEnd = end,
                EndValues = Array.AsReadOnly(values),
                BlockedEnds = Board.Ends.Where(target => target.BlockedTurn == TurnNumber)
                    .Select(target => target.End)
                    .ToList().AsReadOnly(),
                IncludesOpeningDouble = Board.Ends.Any(target => target.IsOpeningEnd
                    && target.BlockedTurn != TurnNumber),
                Total = values.Sum(),
                BasePoints = points,
                Multiplier = multiplier,
                Points = points * multiplier
            };
        }

        private void FinishRound(int? winnerId)
        {
            int total = _players.Sum(player => player.Hand.Sum(tile => tile.TotalPips));
            int award = winnerId.HasValue ? ((total
                + RoundingOffset) / ScoringMultiple) * ScoringMultiple : 0;
            if (winnerId.HasValue)
            {
                GetPlayer(winnerId.Value).Points += award;
            }

            LastRound = new RoundSummary
            {
                RoundNumber = RoundNumber,
                WinnerId = winnerId,
                AwardedPoints = award,
                Scores = _players.ToDictionary(player => player.PlayerId, player => player.Points),
                RemainingHands = _players.ToDictionary(player => player.PlayerId, player
                    => player.Hand.ToArray())
            };
            if (_players.Any(player => player.Points >= TargetPoints))
            {
                Phase = MatchPhase.Completed;
                return;
            }

            BeginRound();
        }

        private void BeginRound()
        {
            List<Tile> deck = _createDeck();
            ValidateDeck(deck);
            _stock = new Queue<Tile>(deck);
            int handSize = _players.Count == MinimumPlayers
                ? TwoPlayerHandSize : MultiplayerHandSize;
            foreach (MatchPlayer player in _players)
            {
                player.Hand.Clear();
                for (int tileIndex = 0; tileIndex < handSize; tileIndex++)
                {
                    player.Hand.Add(_stock.Dequeue());
                }
            }

            _players.Sort((left, right) => GetHandRank(right).CompareTo(GetHandRank(left)));
            OpeningTileId = _players[0].Hand.OrderByDescending(GetTileRank)
                .First().TileId;
            Board = new Board();
            RoundNumber++;
            TurnNumber = 0;
            _currentPlayerIndex = 0;
            _consecutivePasses = 0;
        }

        private static int GetHandRank(MatchPlayer player)
        {
            return player.Hand.Max(GetTileRank);
        }

        private static int GetTileRank(Tile tile)
        {
            return tile.IsDouble ? DoubleRankOffset
                + tile.FirstValue : tile.TotalPips * PipValueRadix + Math.Max(tile.FirstValue,
                tile.SecondValue);
        }

        private static void ValidateDeck(List<Tile> deck)
        {
            if (deck == null || deck.Count != DeckSize || deck.Any(tile => tile == null))
            {
                throw new ArgumentException("A complete double-six deck is required.");
            }

            bool uniqueIds = deck.Select(tile => tile.TileId)
                .Distinct()
                .Count() == DeckSize;
            bool uniqueValues = deck.Select(tile => Math.Min(tile.FirstValue,
                tile.SecondValue) * PipValueRadix + Math.Max(tile.FirstValue, tile.SecondValue))
                .Distinct()
                .Count() == DeckSize;
            if (!uniqueIds || !uniqueValues || deck.Count(tile
                => tile.SpecialPower != SpecialPower.None) > MaximumSpecialTiles)
            {
                throw new ArgumentException("Invalid deck.");
            }
        }
    }
}
