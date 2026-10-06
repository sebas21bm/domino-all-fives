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

        /// <summary>
        /// Starts a new player session with the provided player ID and username.
        /// </summary>
        public void Start(int playerId, string username)
        {
            PlayerId = playerId;
            Username = username;
        }


        /// <summary>
        /// Updates the username of the player in the current session.
        /// </summary>
        public void UpdateUsername(string username)
        {
            Username = username;
        }

        /// <summary>
        /// Clears the player session, effectively logging out the player.
        /// </summary>
        public void Clear()
        {
            PlayerId = null;
            Username = null;
        }
    }
}
