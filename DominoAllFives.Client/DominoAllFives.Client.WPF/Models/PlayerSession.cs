using System;

namespace DominoAllFives.Client.WPF.Models
{
    public class PlayerSession
    {
        public int? PlayerId { get; private set; }
        public string Username { get; private set; }
        public string PreferredLanguageId { get; private set; }
        public string SessionKey { get; private set; }

        public bool IsAuthenticated
        {
            get
            {
                return PlayerId.HasValue;
            }
        }

        public bool HasServerSession
        {
            get
            {
                return IsAuthenticated && !string.IsNullOrWhiteSpace(SessionKey);
            }
        }

        public void Start(int playerId, string username)
        {
            Start(playerId, username, null);
        }

        public void Start(int playerId, string username, string preferredLanguageId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException(null, nameof(username));
            }

            PlayerId = playerId;
            Username = username;
            PreferredLanguageId = preferredLanguageId;
            SessionKey = null;
        }

        public void Start(
            int playerId,
            string username,
            string preferredLanguageId,
            string sessionKey)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException(null, nameof(username));
            }

            if (string.IsNullOrWhiteSpace(sessionKey))
            {
                throw new ArgumentException(null, nameof(sessionKey));
            }

            PlayerId = playerId;
            Username = username;
            PreferredLanguageId = preferredLanguageId;
            SessionKey = sessionKey;
        }

        public void UpdatePreferredLanguage(string preferredLanguageId)
        {
            PreferredLanguageId = preferredLanguageId;
        }

        public void UpdateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException(null, nameof(username));
            }

            Username = username;
        }

        public void Clear()
        {
            PlayerId = null;
            Username = null;
            PreferredLanguageId = null;
            SessionKey = null;
        }
    }
}
