using System;
using System.Collections.Generic;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    internal static class RoundResolution
    {
        private const int MinimumPlayers = 2;
        private const int MaximumPlayers = 4;
        private const int ScoringMultiple = 5;
        private const int RoundingOffset = 2;

        public static RoundSummary ResolvePlayedOut(
            IReadOnlyList<MatchPlayer> players,
            int roundNumber,
            int winnerId)
        {
            ValidateRound(players, roundNumber);
            ValidatePlayedOutWinner(players, winnerId);
            int totalRemainingPips = 0;
            foreach (MatchPlayer player in players)
            {
                totalRemainingPips += CalculateRemainingPips(player);
            }

            int awardedPoints = CalculateRoundedPoints(totalRemainingPips);
            return new RoundSummary(roundNumber, winnerId, awardedPoints, players);
        }

        public static RoundSummary ResolveBlocked(
            IReadOnlyList<MatchPlayer> players,
            int roundNumber)
        {
            ValidateRound(players, roundNumber);
            int totalRemainingPips = 0;
            int minimumRemainingPips = int.MaxValue;
            int? winnerId = null;
            bool hasTie = false;
            foreach (MatchPlayer player in players)
            {
                int remainingPips = CalculateRemainingPips(player);
                totalRemainingPips += remainingPips;
                if (remainingPips < minimumRemainingPips)
                {
                    minimumRemainingPips = remainingPips;
                    winnerId = player.PlayerId;
                    hasTie = false;
                }
                else if (remainingPips == minimumRemainingPips)
                {
                    hasTie = true;
                }
            }

            if (hasTie)
            {
                winnerId = null;
            }

            int awardedPoints = 0;
            if (winnerId.HasValue)
            {
                awardedPoints = CalculateRoundedPoints(totalRemainingPips);
            }

            return new RoundSummary(roundNumber, winnerId, awardedPoints, players);
        }

        private static void ValidateRound(IReadOnlyList<MatchPlayer> players, int roundNumber)
        {
            if (players == null)
            {
                throw new ArgumentNullException(nameof(players));
            }

            if (players.Count < MinimumPlayers || players.Count > MaximumPlayers)
            {
                throw new ArgumentException("Two to four players are required.", nameof(players));
            }

            if (roundNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(roundNumber));
            }
        }

        private static void ValidatePlayedOutWinner(
            IReadOnlyList<MatchPlayer> players,
            int winnerId)
        {
            foreach (MatchPlayer player in players)
            {
                if (player.PlayerId != winnerId)
                {
                    continue;
                }

                if (player.Tiles.Count != 0)
                {
                    throw new InvalidOperationException("The winner must have an empty hand.");
                }

                return;
            }

            throw new MatchRuleException(MatchIssue.PlayerNotInMatch);
        }

        private static int CalculateRemainingPips(MatchPlayer player)
        {
            int remainingPips = 0;
            foreach (Tile tile in player.Tiles)
            {
                remainingPips += tile.TotalPips;
            }

            return remainingPips;
        }

        private static int CalculateRoundedPoints(int totalRemainingPips)
        {
            return ((totalRemainingPips + RoundingOffset) / ScoringMultiple) * ScoringMultiple;
        }
    }
}
