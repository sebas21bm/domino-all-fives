using System;
using System.Runtime.Serialization;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Domain
{
    /// <summary>
    /// Contains a successful account or an expected rejection, never a technical failure.
    /// Deserialization validates the invariant without relying on factories being called.
    /// </summary>
    [DataContract]
    public sealed class RegistrationResult
    {
        [DataMember(Order = 1, IsRequired = true)]
        public PlayerAccount Account { get; private set; }

        [DataMember(Order = 2, IsRequired = true)]
        public RegistrationStatus Status { get; private set; }

        private RegistrationResult(PlayerAccount account, RegistrationStatus reason)
        {
            Account = account;
            Status = reason;
        }

        public static RegistrationResult Success(PlayerAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            return new RegistrationResult(account, RegistrationStatus.None);
        }

        public static RegistrationResult Rejected(RegistrationStatus reason)
        {
            if (!IsRejection(reason))
            {
                throw new ArgumentOutOfRangeException(nameof(reason),
                    "Only expected business rejection reasons are allowed.");
            }
            return new RegistrationResult(null, reason);
        }

        private static bool IsRejection(RegistrationStatus reason)
        {
            return reason == RegistrationStatus.InvalidUsername ||
                reason == RegistrationStatus.InvalidEmail ||
                reason == RegistrationStatus.InvalidPassword ||
                reason == RegistrationStatus.UsernameAlreadyExists ||
                reason == RegistrationStatus.EmailAlreadyExists;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            bool isSuccess = Status == RegistrationStatus.None && Account != null;
            bool isRejection = Account == null && IsRejection(Status);
            if (!isSuccess && !isRejection)
            {
                throw new SerializationException("Contradictory operation result.");
            }
        }
    }
}
