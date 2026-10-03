namespace DominoAllFives.Client.WPF.Models
{
    /// <summary>
    /// Represents a player that is displayed in the InviteFriends interface.
    /// </summary>
    public class GameInvitationRecord
    {
        public string Username { get; set; }
        public string AvatarPath { get; set; }
        public string RoomCode { get; set; }
    }
}
