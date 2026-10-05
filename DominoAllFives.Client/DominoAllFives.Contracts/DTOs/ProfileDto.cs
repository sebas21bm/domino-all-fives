using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.DTOs
{
    /// <summary>
    /// Represents the profile information of a player.
    /// </summary>
    public class ProfileDto
    {
        public string Username { get; set; }

        public byte[] ProfilePictureData { get; set; }

        public int Wins { get; set; }

        public int TotalPoints { get; set; }

        public int GamesPlayed { get; set; }

        public RetrieveInformationFailureReason FailureReason { get; set; }
    }
}