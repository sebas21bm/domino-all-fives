namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class Placement
    {
        public Tile Tile { get; internal set; }
        public BoardEndPosition End { get; internal set; }
        public int ConnectingValue { get; internal set; }
        public int ExposedValue { get; internal set; }
    }
}
