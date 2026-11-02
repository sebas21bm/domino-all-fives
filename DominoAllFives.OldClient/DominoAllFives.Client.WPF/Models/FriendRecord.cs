namespace DominoAllFives.Client.WPF.Models
{
    /// <summary>
    /// Represents a player that is displayed in the Friends view
    /// </summary>
    public class FriendRecord
    {
        public string Username { get; set; }
        public string AvatarPath { get; set; }
        public bool IsInvited { get; set; }
        public bool CanInvite => !IsInvited;
    }
}
