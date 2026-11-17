using System.Collections.Generic;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    internal sealed class PreparedRoundEnd
    {
        public RoundSummary Summary { get; }
        public MatchPlayer Winner { get; }
        public PreparedRound NextRound { get; }
        public List<MatchPlayer> OrderedPlayers { get; }

        public PreparedRoundEnd(
            RoundSummary summary,
            MatchPlayer winner,
            PreparedRound nextRound,
            List<MatchPlayer> orderedPlayers)
        {
            Summary = summary;
            Winner = winner;
            NextRound = nextRound;
            OrderedPlayers = orderedPlayers;
        }
    }
}
