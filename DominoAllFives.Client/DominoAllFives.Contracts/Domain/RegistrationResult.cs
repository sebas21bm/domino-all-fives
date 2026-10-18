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
        public RegistrationFailureReason Reason { get; private set; }

        private RegistrationResult(PlayerAccount account, RegistrationFailureReason reason)
        {
            Account = account;
            Reason = reason;
        }

        public static RegistrationResult Success(PlayerAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            return new RegistrationResult(account, RegistrationFailureReason.None);
        }

        public static RegistrationResult Rejected(RegistrationFailureReason reason)
        {
            if (!IsRejection(reason))
            {
                throw new ArgumentOutOfRangeException(nameof(reason),
                    "Only expected business rejection reasons are allowed.");
            }
            return new RegistrationResult(null, reason);
        }

        private static bool IsRejection(RegistrationFailureReason reason)
        {
            return reason == RegistrationFailureReason.InvalidUsername ||
                reason == RegistrationFailureReason.InvalidEmail ||
                reason == RegistrationFailureReason.InvalidPassword ||
                reason == RegistrationFailureReason.UsernameAlreadyExists ||
                reason == RegistrationFailureReason.EmailAlreadyExists;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            bool isSuccess = Reason == RegistrationFailureReason.None && Account != null;
            bool isRejection = Account == null && IsRejection(Reason);
            if (!isSuccess && !isRejection)
            {
                throw new SerializationException("Contradictory operation result.");
            }
        }
    }
}
