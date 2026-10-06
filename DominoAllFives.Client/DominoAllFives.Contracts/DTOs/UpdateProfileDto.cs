namespace DominoAllFives.Contracts.DTOs
{
    /// <summary>
    /// Represents the editable profile information of a player.
    /// </summary>
    public class UpdateProfileDto
    {
        public int PlayerId { get; set; }

        public string Username { get; set; }

        public bool ProfilePictureChanged { get; set; }

        public string ProfilePictureFileName { get; set; }

        public byte[] ProfilePictureData { get; set; }
    }
}