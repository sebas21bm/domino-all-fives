using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class RoundSummary
    {
        public int RoundNumber { get; }
        public int? WinnerId { get; }
        public int AwardedPoints { get; }
        public IReadOnlyDictionary<int, int> Scores { get; }
        public IReadOnlyDictionary<int, IReadOnlyList<Tile>> RemainingHands { get; }

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
    }
}
