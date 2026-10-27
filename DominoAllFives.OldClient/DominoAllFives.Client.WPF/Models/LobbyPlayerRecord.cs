namespace DominoAllFives.Client.WPF.Models
{
    /// <summary>
    /// Represents a player displayed in the lobby.
    /// </summary>
    public class LobbyPlayerRecord
    {
        public string Username { get; set; } = string.Empty;
        public string AvatarPath { get; set; } = string.Empty;
        public bool IsHost { get; set; }
        public bool IsCurrentPlayer { get; set; }
    }
}
