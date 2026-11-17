using System;
using System.Collections.Generic;
using System.Linq;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Gameplay;
using GameplayContracts = DominoAllFives.Contracts.Gameplay;

namespace DominoAllFives.BusinessLogic.Managers
{
    public sealed class MatchManager
    {
        private const int MatchRetentionMinutes = 30;
        private static readonly MatchManager instance = new MatchManager();
        private readonly object _syncRoot = new object();
        private readonly Dictionary<int, ActiveMatch> _matches =
            new Dictionary<int, ActiveMatch>();

        public static MatchManager Instance
        {
            get
            {
                return instance;
            }
        }

        public void CreateMatch(int matchId, IEnumerable<int> players, int targetPoints)
        {
            lock (_syncRoot)
            {
                foreach (KeyValuePair<int, ActiveMatch> entry in _matches.ToArray())
                {
                    if (HasExpired(entry.Value))
                    {
                        _matches.Remove(entry.Key);
                    }
                }

                ActiveMatch active;
                if (!_matches.TryGetValue(matchId, out active))
                {
                    active = new ActiveMatch(
                        new Match(matchId, players, targetPoints, Deck.Create));
                    _matches.Add(matchId, active);
                }
            }
        }

        public void RemoveMatch(int matchId)
        {
            lock (_syncRoot)
            {
                _matches.Remove(matchId);
            }
        }

        public GameplayContracts.MatchView GetMatchView(int matchId, int playerId)
        {
            lock (_syncRoot)
            {
                Match match = GetActiveMatch(matchId).Match;
                match.GetPlayer(playerId);
                return MatchContractMapper.CreateMatchView(match);
            }
        }

        public GameplayContracts.PlayerHand GetPlayerHand(int matchId, int playerId)
        {
            lock (_syncRoot)
            {
                Match match = GetActiveMatch(matchId).Match;
                return MatchContractMapper.CreatePlayerHand(match, playerId);
            }
        }

        public MatchUpdate PlayTile(
            int matchId,
            int playerId,
            Move move)
        {
            lock (_syncRoot)
            {
                ActiveMatch activeMatch = GetActiveMatch(matchId);
                Match match = activeMatch.Match;
                match.GetPlayer(playerId);
                RoundSummary previousRound = match.LastRound;
                long previousVersion = match.Version;

                match.Play(
                    playerId,
                    move.TileId,
                    move.End,
                    move.ExpectedVersion);
                return CreateUpdate(
                    activeMatch,
                    previousRound,
                    previousVersion);
            }
        }

        public MatchUpdate DrawTiles(int matchId, int playerId, long expectedVersion)
        {
            lock (_syncRoot)
            {
                ActiveMatch activeMatch = GetActiveMatch(matchId);
                Match match = activeMatch.Match;
                match.GetPlayer(playerId);
                RoundSummary previousRound = match.LastRound;
                long previousVersion = match.Version;

                match.Draw(playerId, expectedVersion);
                return CreateUpdate(
                    activeMatch,
                    previousRound,
                    previousVersion);
            }
        }

        public MatchUpdate PassTurn(int matchId, int playerId, long expectedVersion)
        {
            lock (_syncRoot)
            {
                ActiveMatch activeMatch = GetActiveMatch(matchId);
                Match match = activeMatch.Match;
                match.GetPlayer(playerId);
                RoundSummary previousRound = match.LastRound;
                long previousVersion = match.Version;

                match.Pass(playerId, expectedVersion);
                return CreateUpdate(
                    activeMatch,
                    previousRound,
                    previousVersion);
            }
        }

        public MatchUpdate LeaveMatch(int matchId, int playerId)
        {
            lock (_syncRoot)
            {
                ActiveMatch activeMatch = GetActiveMatch(matchId);
                Match match = activeMatch.Match;
                match.GetPlayer(playerId);
                RoundSummary previousRound = match.LastRound;
                long previousVersion = match.Version;

                match.Cancel(playerId);
                return CreateUpdate(
                    activeMatch,
                    previousRound,
                    previousVersion);
            }
        }

        private static MatchUpdate CreateUpdate(
            ActiveMatch activeMatch,
            RoundSummary previousRound,
            long previousVersion)
        {
            Match match = activeMatch.Match;
            if (match.Phase != MatchState.InProgress
                && !activeMatch.FinishedAt.HasValue)
            {
                activeMatch.FinishedAt = DateTime.UtcNow;
            }

            GameplayContracts.MatchView view = MatchContractMapper.CreateMatchView(match);
            bool hasStateChange = previousVersion != match.Version;
            MatchUpdate update = new MatchUpdate
            {
                View = view,
                HasStateChange = hasStateChange
            };

            if (!hasStateChange)
            {
                return update;
            }

            update.Hands = new List<GameplayContracts.PlayerHand>();
            foreach (MatchPlayer player in match.Players)
            {
                GameplayContracts.PlayerHand hand = MatchContractMapper.CreatePlayerHand(
                    match,
                    player.PlayerId);
                update.Hands.Add(hand);
            }

            if (!ReferenceEquals(previousRound, match.LastRound))
            {
                update.Round = view.LastRound;
            }

            if (match.Phase != MatchState.InProgress)
            {
                update.Result = MatchContractMapper.CreateMatchResult(match);
            }

            return update;
        }

        private ActiveMatch GetActiveMatch(int matchId)
        {
            ActiveMatch active;
            if (!_matches.TryGetValue(matchId, out active))
            {
                throw new MatchRuleException(MatchIssue.MatchNotFound);
            }

            if (HasExpired(active))
            {
                _matches.Remove(matchId);
                throw new MatchRuleException(MatchIssue.MatchNotFound);
            }

            return active;
        }

        private static bool HasExpired(ActiveMatch active)
        {
            return active.FinishedAt.HasValue
                && DateTime.UtcNow - active.FinishedAt.Value
                    > TimeSpan.FromMinutes(MatchRetentionMinutes);
        }
    }
}
