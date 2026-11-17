using System;
using System.Collections.Generic;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    internal static class PlayScoring
    {
        private const int ScoringMultiple = 5;
        private const int DoublePointsMultiplier = 2;

        public static PlayScore Evaluate(Board board, int turnNumber)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (turnNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(turnNumber));
            }

            if (board.Tiles.Count == 0)
            {
                throw new InvalidOperationException("A placed tile is required to score a play.");
            }

            Placement placement = board.Tiles[board.Tiles.Count - 1];
            int[] scoringValues = board.GetScoringValues(turnNumber);
            int total = 0;
            foreach (int scoringValue in scoringValues)
            {
                total += scoringValue;
            }

            int basePoints = 0;
            if (total % ScoringMultiple == 0)
            {
                basePoints = total;
            }

            int multiplier = 1;
            if (placement.Tile.SpecialPower == SpecialPower.DoublePoints)
            {
                multiplier = DoublePointsMultiplier;
            }

            PlayScore score = new PlayScore
            {
                Tile = placement.Tile,
                PlayedEnd = placement.End,
                EndValues = Array.AsReadOnly(scoringValues),
                Total = total,
                BasePoints = basePoints,
                Multiplier = multiplier,
                Points = basePoints * multiplier
            };
            SetEndExplanation(score, board, turnNumber);
            return score;
        }

        private static void SetEndExplanation(PlayScore score, Board board, int turnNumber)
        {
            List<BoardEndPosition> blockedEnds = new List<BoardEndPosition>();
            bool includesOpeningDouble = false;
            foreach (BoardEndState boardEnd in board.Ends)
            {
                if (boardEnd.BlockedTurn == turnNumber)
                {
                    blockedEnds.Add(boardEnd.End);
                }

                if (boardEnd.IsOpeningEnd && boardEnd.BlockedTurn != turnNumber)
                {
                    includesOpeningDouble = true;
                }
            }

            score.BlockedEnds = blockedEnds.AsReadOnly();
            score.IncludesOpeningDouble = includesOpeningDouble;
        }
    }
}
