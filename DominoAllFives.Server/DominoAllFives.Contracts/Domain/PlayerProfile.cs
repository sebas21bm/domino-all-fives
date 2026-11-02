using System;

namespace DominoAllFives.Contracts.Domain
{
    /// <summary>
    /// Composes the public identity, statistics and optional picture of a player.
    /// </summary>
    public sealed class PlayerProfile
    {
        public string Username { get; private set; }
        public PlayerStatistics Statistics { get; private set; }
        public ProfilePicture Picture { get; private set; }

        private PlayerProfile()
        {
        }

        /// <summary>
        /// Composes a profile from server-reconstructed domain values.
        /// </summary>
        /// <param name="username">The stored username.</param>
        /// <param name="statistics">The stored statistics snapshot.</param>
        /// <param name="picture">The picture snapshot, which may contain absent content.</param>
        /// <returns>A profile without operation failure flags.</returns>
        public static PlayerProfile FromStoredProfile(string username,
            PlayerStatistics statistics, ProfilePicture picture)
        {
            return new PlayerProfile
            {
                Username = username,
                Statistics = statistics ?? throw new ArgumentNullException(nameof(statistics)),
                Picture = picture ?? throw new ArgumentNullException(nameof(picture))
            };
        }
    }
}
