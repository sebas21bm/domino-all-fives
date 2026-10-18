using System;
using System.Runtime.Serialization;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Domain
{
    /// <summary>
    /// Represents account identity, preference and server-reconstructed access restrictions.
    /// </summary>
    [DataContract]
    public sealed class PlayerAccount
    {
        private const int MaximumLanguageIdLength = 10;

        [DataMember(Order = 1, IsRequired = true)]
        public int PlayerId { get; private set; }
        [DataMember(Order = 2, IsRequired = true)]
        public string Username { get; private set; }
        [DataMember(Order = 3, IsRequired = true)]
        public string PreferredLanguageId { get; private set; }
        [DataMember(Order = 4, IsRequired = true)]
        public AccountAccessStatus AccessStatus { get; private set; }

        private PlayerAccount()
        {
        }

        /// <summary>
        /// Reconstructs an account without including credentials or persistence entities.
        /// </summary>
        /// <param name="playerId">The account identifier.</param>
        /// <param name="username">The stored or requested username.</param>
        /// <param name="preferredLanguageId">The preferred language identifier associated with the account.</param>
        /// <param name="accessStatus">The restrictions reconstructed by the server.</param>
        /// <returns>The domain value produced by the operation.</returns>
        public static PlayerAccount FromStoredAccount(int playerId, string username,
            string preferredLanguageId, AccountAccessStatus accessStatus)
        {
            PlayerAccount account = new PlayerAccount
            {
                PlayerId = playerId,
                Username = username,
                AccessStatus = accessStatus ?? throw new ArgumentNullException(nameof(accessStatus))
            };
            account.SetPreferredLanguage(preferredLanguageId);
            return account;
        }

        /// <summary>
        /// Maps the preferred language stored with the account.
        /// </summary>
        /// <param name="preferredLanguageId">The preferred language identifier associated with the account.</param>
        public void SetPreferredLanguage(string preferredLanguageId)
        {
            if (!IsPreferredLanguageValid(preferredLanguageId))
            {
                throw new ArgumentException("Invalid stored language identifier.",
                    nameof(preferredLanguageId));
            }
            PreferredLanguageId = preferredLanguageId;
        }

        private static bool IsPreferredLanguageValid(string preferredLanguageId)
        {
            return preferredLanguageId != null &&
                preferredLanguageId.Length <= MaximumLanguageIdLength;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (AccessStatus == null || !IsPreferredLanguageValid(PreferredLanguageId))
            {
                throw new SerializationException("Invalid stored account state.");
            }
        }

        /// <summary>
        /// Checks server-reconstructed restrictions without authenticating a client session.
        /// </summary>
        /// <param name="currentTime">The comparison time in the timestamp convention of the stored restrictions.</param>
        public LoginFailureReason EnsureCanLogin(DateTime currentTime)
        {
            if (AccessStatus == null)
            {
                throw new InvalidOperationException("Account access restrictions are unavailable.");
            }
            return AccessStatus.EnsureCanLogin(currentTime);
        }
    }
}
