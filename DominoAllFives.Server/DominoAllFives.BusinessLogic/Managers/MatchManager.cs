using System;
using System.Collections.Generic;
using System.Linq;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Gameplay;
using Network = DominoAllFives.Contracts.Gameplay;

namespace DominoAllFives.BusinessLogic.Managers
{
    public sealed class MatchManager
    {
        private const int MatchRetentionMinutes = 30;
        private readonly object _syncRoot = new object();
        private readonly Dictionary<int, ActiveMatch> _matches =
            new Dictionary<int, ActiveMatch>();

        public ActiveMatch GetMatch(int matchId)
        {
            lock (_syncRoot)
            {
                return GetMatchInternal(matchId);
            }
        }

        public ActiveMatch CreateMatch(int matchId, IEnumerable<int> players, int targetPoints)
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

                return active;
            }
        }

        public void RemoveMatch(int matchId)
        {
            lock (_syncRoot)
            {
                _matches.Remove(matchId);
            }
        }

        public Network.MatchView GetMatchView(int matchId, int playerId)
        {
            lock (_syncRoot)
            {
                Match match = GetMatchInternal(matchId).Match;
                match.GetPlayer(playerId);
                return MatchProjection.CreateView(match);
            }
        }

        public Network.PlayerHand GetPlayerHand(int matchId, int playerId)
        {
            lock (_syncRoot)
            {
                return MatchProjection.CreateHand(GetMatchInternal(matchId).Match, playerId);
            }
        }

        public Network.MatchView ChangeMatch(
            int matchId,
            int playerId,
            Action<Match> change,
            Action<MatchUpdate> enqueue)
        {
            lock (_syncRoot)
            {
                ActiveMatch active = GetMatchInternal(matchId);
                Match match = active.Match;
                match.GetPlayer(playerId);
                RoundSummary previousRound = match.LastRound;
                long previousVersion = match.Version;
                change(match);
                if (match.Phase != MatchState.InProgress && !active.FinishedAt.HasValue)
                {
                    active.FinishedAt = DateTime.UtcNow;
                }

                Network.MatchView view = MatchProjection.CreateView(match);
                if (previousVersion == match.Version)
                {
                    return view;
                }

                // Queue snapshots atomically; the publisher delivers WCF notifications later.
                enqueue(new MatchUpdate
                {
                    View = view,
                    Hands = match.Players
                        .Select(player => MatchProjection.CreateHand(match, player.PlayerId))
                        .ToList(),
                    Round = ReferenceEquals(previousRound, match.LastRound) ? null : view.LastRound,
                    Result = match.Phase == MatchState.InProgress
                        ? null : MatchProjection.CreateResult(match)
                });
                return view;
            }
        }

        private ActiveMatch GetMatchInternal(int matchId)
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
