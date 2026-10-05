namespace DominoAllFives.Client.WPF.Models
{
    /// <summary>
    /// Stores information about the player authenticated in the current session.
    /// </summary>
    public class PlayerSession
    {
        public int? PlayerId { get; private set; }

        public string Username { get; private set; }

        public bool IsAuthenticated
        {
            get => PlayerId.HasValue;
        }

        public void Start(int playerId, string username)
        {
            PlayerId = playerId;
            Username = username;
        }

        public void Clear()
        {
            PlayerId = null;
            Username = null;
        }
    }
}
