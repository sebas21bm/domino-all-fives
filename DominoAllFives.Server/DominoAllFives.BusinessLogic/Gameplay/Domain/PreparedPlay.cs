namespace DominoAllFives.BusinessLogic.Gameplay
{
    internal sealed class PreparedPlay
    {
        public Board Board { get; }
        public MatchPlayer Player { get; }
        public PlayScore Score { get; }
        public MatchState Phase { get; internal set; }
        public PreparedRoundEnd RoundEnd { get; internal set; }

        public PreparedPlay(Board board, MatchPlayer player, PlayScore score)
        {
            Board = board;
            Player = player;
            Score = score;
            Phase = MatchState.InProgress;
        }
    }
}
