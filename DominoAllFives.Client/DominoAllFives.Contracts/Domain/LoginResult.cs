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
    public sealed class LoginResult
    {
        [DataMember(Order = 1, IsRequired = true)]
        public PlayerAccount Account { get; private set; }

        [DataMember(Order = 2, IsRequired = true)]
        public LoginFailureReason Reason { get; private set; }

        private LoginResult(PlayerAccount account, LoginFailureReason reason)
        {
            Account = account;
            Reason = reason;
        }

        public static LoginResult Success(PlayerAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            return new LoginResult(account, LoginFailureReason.None);
        }

        public static LoginResult Rejected(LoginFailureReason reason)
        {
            if (!IsRejection(reason))
            {
                throw new ArgumentOutOfRangeException(nameof(reason),
                    "Only expected business rejection reasons are allowed.");
            }
            return new LoginResult(null, reason);
        }

        private static bool IsRejection(LoginFailureReason reason)
        {
            return reason == LoginFailureReason.InvalidCredentials ||
                reason == LoginFailureReason.Banned ||
                reason == LoginFailureReason.Suspended;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            bool isSuccess = Reason == LoginFailureReason.None && Account != null;
            bool isRejection = Account == null && IsRejection(Reason);
            if (!isSuccess && !isRejection)
            {
                throw new SerializationException("Contradictory operation result.");
            }
        }
    }
}
