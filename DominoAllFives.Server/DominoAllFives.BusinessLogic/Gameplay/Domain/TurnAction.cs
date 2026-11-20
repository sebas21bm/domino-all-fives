namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class TurnAction
    {
        public int PlayerId { get; }
        public bool IsPass { get; }
        public long Version { get; }

        internal TurnAction(int playerId, bool isPass, long version)
        {
            PlayerId = playerId;
            IsPass = isPass;
            Version = version;
        }
    }
}
