namespace DominoAllFives.Client.WPF.Models
{
    /// <summary>
    /// Stores information about the player authenticated in the current session.
    /// </summary>
    public class PlayerSession
    {
        public int? PlayerId { get; private set; }

        public string Username { get; private set; }

        public string PreferredLanguageId { get; private set; }

        public bool IsAuthenticated
        {
            get => PlayerId.HasValue;
        }

        /// <summary>
        /// Starts a new player session with the provided player ID and username.
        /// </summary>
        public void Start(int playerId, string username)
        {
            Start(playerId, username, null);
        }

        /// <summary>Starts the single local session with the account preference.</summary>
        public void Start(int playerId, string username, string preferredLanguageId)
        {
            PlayerId = playerId;
            Username = username;
            PreferredLanguageId = preferredLanguageId;
        }

        /// <summary>Updates the session preference after persistence succeeds.</summary>
        public void UpdatePreferredLanguage(string preferredLanguageId)
        {
            PreferredLanguageId = preferredLanguageId;
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
            PreferredLanguageId = null;
        }
    }
}
