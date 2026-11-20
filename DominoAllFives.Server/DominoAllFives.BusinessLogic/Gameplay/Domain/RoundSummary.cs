using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    internal sealed class RoundSummary
    {
        public int RoundNumber { get; }
        public int? WinnerId { get; }
        public int AwardedPoints { get; }
        public IReadOnlyDictionary<int, int> Scores { get; }
        public IReadOnlyDictionary<int, IReadOnlyList<Tile>> RemainingHands { get; }
        internal Board ClosedBoard { get; private set; }
        internal int ClosingTurn { get; private set; }
        internal long Version { get; private set; }

        internal RoundSummary(
            int roundNumber,
            int? winnerId,
            int awardedPoints,
            IReadOnlyList<MatchPlayer> players)
        {
            RoundNumber = roundNumber;
            WinnerId = winnerId;
            AwardedPoints = awardedPoints;
            Dictionary<int, int> scores = new Dictionary<int, int>();
            Dictionary<int, IReadOnlyList<Tile>> remainingHands =
                new Dictionary<int, IReadOnlyList<Tile>>();
            foreach (MatchPlayer player in players)
            {
                int points = player.Points;
                if (player.PlayerId == winnerId)
                {
                    points += awardedPoints;
                }

                scores.Add(player.PlayerId, points);
                List<Tile> hand = new List<Tile>(player.Tiles);
                remainingHands.Add(player.PlayerId, hand.AsReadOnly());
            }

            Scores = new ReadOnlyDictionary<int, int>(scores);
            RemainingHands = new ReadOnlyDictionary<int, IReadOnlyList<Tile>>(remainingHands);
        }

        internal void CaptureClosingBoard(Board board, int turnNumber, long version)
        {
            ClosedBoard = board.Copy();
            ClosingTurn = turnNumber;
            Version = version;
        }
    }
}
