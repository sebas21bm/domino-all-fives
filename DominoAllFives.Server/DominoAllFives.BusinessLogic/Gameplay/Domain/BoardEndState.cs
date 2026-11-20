namespace DominoAllFives.BusinessLogic.Gameplay
{
    internal sealed class BoardEndState
    {
        private const int UnblockedTurn = -1;

        public BoardEndPosition End { get; internal set; }
        public int Value { get; internal set; }
        public int ScoreValue { get; internal set; }
        public int BlockedTurn { get; internal set; } = UnblockedTurn;
        public bool IsOpeningEnd { get; internal set; }
    }
}
