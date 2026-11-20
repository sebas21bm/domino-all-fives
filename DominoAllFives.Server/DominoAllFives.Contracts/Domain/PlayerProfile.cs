using System;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Domain
{
    /// <summary>
    /// Composes the public identity, statistics and optional picture of a player.
    /// </summary>
    [DataContract]
    public sealed class PlayerProfile
    {
        [DataMember(Order = 1, IsRequired = true)]
        public string Username { get; private set; }
        [DataMember(Order = 2, IsRequired = true)]
        public PlayerStatistics Statistics { get; private set; }
        [DataMember(Order = 3, IsRequired = true)]
        public ProfilePicture Picture { get; private set; }

        private PlayerProfile()
        {
        }

        /// <summary>
        /// Composes a profile from server-reconstructed domain values.
        /// </summary>
        /// <param name = "username">The stored username.</param>
        /// <param name = "statistics">The stored statistics snapshot.</param>
        /// <param name = "picture">The picture snapshot, which may contain absent content.</param>
        /// <returns>A profile without operation failure flags.</returns>
        public static PlayerProfile FromStoredProfile(
            string username,
            PlayerStatistics statistics,
            ProfilePicture picture)
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
