using System;
using DominoAllFives.Contracts.Enums;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Domain
{
    /// <summary>
    /// Applies access restrictions reconstructed from server persistence.
    /// </summary>
    [DataContract]
    public sealed class AccountAccessStatus
    {
        [DataMember(Order = 1, IsRequired = true)]
        public bool IsDeleted { get; private set; }
        [DataMember(Order = 2, IsRequired = true)]
        public bool IsBanned { get; private set; }
        [DataMember(Order = 3, IsRequired = true)]
        public DateTime? SuspensionUntil { get; private set; }

        private AccountAccessStatus()
        {
        }

        /// <summary>
        /// Reconstructs restrictions without reinterpreting historical timestamps.
        /// </summary>
        /// <param name="isDeleted">The persisted logical deletion flag.</param>
        /// <param name="isBanned">The persisted ban flag.</param>
        /// <param name="suspensionUntil">The persisted suspension boundary without time-zone reinterpretation.</param>
        /// <returns>The domain value produced by the operation.</returns>
        public static AccountAccessStatus FromStoredState(
            bool isDeleted, bool isBanned, DateTime? suspensionUntil)
        {
            return new AccountAccessStatus
            {
                IsDeleted = isDeleted,
                IsBanned = isBanned,
                SuspensionUntil = suspensionUntil
            };
        }

        /// <summary>
        /// Enforces restrictions using a time in the stored timestamp convention.
        /// </summary>
        /// <param name="currentTime">The comparison time in the timestamp convention of the stored restrictions.</param>
        public LoginFailureReason EnsureCanLogin(DateTime currentTime)
        {
            if (IsDeleted)
            {
                return LoginFailureReason.InvalidCredentials;
            }
            if (IsBanned)
            {
                return LoginFailureReason.Banned;
            }
            if (SuspensionUntil.HasValue && SuspensionUntil.Value > currentTime)
            {
                return LoginFailureReason.Suspended;
            }
            return LoginFailureReason.None;
        }
    }
}
