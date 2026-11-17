using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class Match
    {
        private const int MinimumPlayers = 2;
        private const int MaximumPlayers = 4;
        private static readonly int[] allowedTargets =
        {
            100,
            150,
            200,
            250
        };
        private List<MatchPlayer> _players;
        private readonly DeckGenerator _deckGenerator = new DeckGenerator();
        private readonly RoundSetup _roundSetup = new RoundSetup();
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
            List<Tile> initialDeck)
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
            BeginRound(initialDeck);
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

            PreparedPlay play = PreparePlay(player, tile, end);
            ApplyPlay(play);
        }

        private PreparedPlay PreparePlay(MatchPlayer player, Tile tile, BoardEndPosition end)
        {
            Board board = Board.Copy();
            board.Place(tile, end, TurnNumber);
            MatchPlayer projectedPlayer = new MatchPlayer
            {
                PlayerId = player.PlayerId,
                Points = player.Points,
                Hand = new List<Tile>(player.Hand)
            };
            projectedPlayer.Hand.Remove(tile);

            PlayScore score = PlayScoring.Evaluate(board, TurnNumber);
            score.PlayerId = player.PlayerId;
            score.RoundNumber = RoundNumber;
            projectedPlayer.Points += score.Points;
            PreparedPlay play = new PreparedPlay(board, projectedPlayer, score);

            if (projectedPlayer.Points >= TargetPoints)
            {
                play.Phase = MatchState.Completed;
                return play;
            }

            if (projectedPlayer.Hand.Count == 0)
            {
                List<MatchPlayer> projectedPlayers = new List<MatchPlayer>(_players);
                projectedPlayers[_currentPlayerIndex] = projectedPlayer;
                RoundSummary round = RoundResolution.ResolvePlayedOut(
                    projectedPlayers,
                    RoundNumber,
                    player.PlayerId);
                play.RoundEnd = PrepareRoundEnd(round);
                score.RoundPoints = round.AwardedPoints;
            }

            return play;
        }

        private void ApplyPlay(PreparedPlay play)
        {
            MatchPlayer player = _players[_currentPlayerIndex];
            player.Hand = play.Player.Hand;
            player.Points = play.Player.Points;
            Board = play.Board;
            LastPlay = play.Score;
            Phase = play.Phase;
            _consecutivePasses = 0;
            if (play.RoundEnd != null)
            {
                ApplyRoundEnd(play.RoundEnd);
            }
            else if (Phase == MatchState.InProgress)
            {
                AdvanceTurn();
            }

            Version++;
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

            int nextPassCount = _consecutivePasses + 1;
            int nextTurnNumber = TurnNumber + 1;
            PreparedRoundEnd roundEnd = null;
            if (nextPassCount >= _players.Count && !AnyPlayerHasMove(nextTurnNumber))
            {
                RoundSummary round = RoundResolution.ResolveBlocked(_players, RoundNumber);
                roundEnd = PrepareRoundEnd(round);
            }

            _consecutivePasses = nextPassCount;
            AdvanceTurn();
            if (roundEnd != null)
            {
                ApplyRoundEnd(roundEnd);
            }

            Version++;
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

        private bool AnyPlayerHasMove(int turnNumber)
        {
            foreach (MatchPlayer player in _players)
            {
                if (Board.HasMove(player.Hand, turnNumber))
                {
                    return true;
                }
            }

            return false;
        }

        private PreparedRoundEnd PrepareRoundEnd(RoundSummary round)
        {
            MatchPlayer winner = null;
            if (round.WinnerId.HasValue)
            {
                winner = GetPlayer(round.WinnerId.Value);
            }

            foreach (int score in round.Scores.Values)
            {
                if (score >= TargetPoints)
                {
                    return new PreparedRoundEnd(round, winner, null, null);
                }
            }

            List<Tile> deck = _deckGenerator.Generate();
            PreparedRound nextRound = _roundSetup.Prepare(GetPlayerIds(), deck);
            List<MatchPlayer> orderedPlayers = OrderPlayers(nextRound);
            return new PreparedRoundEnd(round, winner, nextRound, orderedPlayers);
        }

        private void ApplyRoundEnd(PreparedRoundEnd roundEnd)
        {
            if (roundEnd.Winner != null)
            {
                roundEnd.Winner.Points += roundEnd.Summary.AwardedPoints;
            }

            LastRound = roundEnd.Summary;
            if (roundEnd.NextRound == null)
            {
                Phase = MatchState.Completed;
                return;
            }

            ApplyPreparedRound(roundEnd.NextRound, roundEnd.OrderedPlayers);
        }

        private void BeginRound(List<Tile> deck)
        {
            PreparedRound round = _roundSetup.Prepare(GetPlayerIds(), deck);
            List<MatchPlayer> orderedPlayers = OrderPlayers(round);
            ApplyPreparedRound(round, orderedPlayers);
        }

        private List<int> GetPlayerIds()
        {
            List<int> playerIds = new List<int>();
            foreach (MatchPlayer player in _players)
            {
                playerIds.Add(player.PlayerId);
            }

            return playerIds;
        }

        private List<MatchPlayer> OrderPlayers(PreparedRound round)
        {
            List<MatchPlayer> orderedPlayers = new List<MatchPlayer>();
            foreach (MatchPlayer player in round.Players)
            {
                orderedPlayers.Add(GetPlayer(player.PlayerId));
            }

            return orderedPlayers;
        }

        private void ApplyPreparedRound(PreparedRound round, List<MatchPlayer> orderedPlayers)
        {
            for (int index = 0; index < orderedPlayers.Count; index++)
            {
                orderedPlayers[index].Hand = round.Players[index].Hand;
            }

            _players = orderedPlayers;
            _stock = round.Stock;
            OpeningTileId = round.OpeningTileId;
            Board = round.Board;
            RoundNumber++;
            TurnNumber = 0;
            _currentPlayerIndex = 0;
            _consecutivePasses = 0;
        }
    }
}
