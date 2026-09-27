namespace DominoAllFives.Contracts.DTOs
{
    /// <summary>
    /// Represents a player entry in the game ranking.
    /// </summary>
    public class RankingEntryDto
    {
        public int Rank { get; set; }

        public string Username { get; set; }

        public int GamesWon { get; set; }

        public string ProfilePicture { get; set; }
    }
}