namespace DominoAllFives.Client.WPF.Models
{
    /// <summary>
    /// Represents the different types of domino tiles available in the game.
    /// </summary>
    public enum DominoTileType
    {
        /// <summary>
        /// Standard domino tile.
        /// </summary>
        Normal,

        /// <summary>
        /// Domino tile that awards double points.
        /// </summary>
        DoublePoints,

        /// <summary>
        /// Domino tile used to block the side where it was played and disable the points it sums
        /// for one turn.
        /// </summary>
        Block
    }
}
