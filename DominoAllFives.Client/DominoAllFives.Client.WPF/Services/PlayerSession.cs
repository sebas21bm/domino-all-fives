namespace DominoAllFives.Client.WPF.Services
{
    /// <summary>
    /// Stores information about the player authenticated in the current session.
    /// </summary>
    public class PlayerSession
    {
        public int? PlayerId { get; private set; }

        public bool IsAuthenticated
        {
            get => PlayerId.HasValue;
        }

        public void Start(int playerId)
        {
            PlayerId = playerId;
        }

        public void Clear()
        {
            PlayerId = null;
        }
    }
}
