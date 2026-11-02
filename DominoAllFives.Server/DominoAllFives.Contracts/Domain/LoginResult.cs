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
        public LoginStatus Status { get; private set; }

        private LoginResult(PlayerAccount account, LoginStatus reason)
        {
            Account = account;
            Status = reason;
        }

        public static LoginResult Success(PlayerAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            return new LoginResult(account, LoginStatus.None);
        }

        public static LoginResult Rejected(LoginStatus reason)
        {
            if (!IsRejection(reason))
            {
                throw new ArgumentOutOfRangeException(nameof(reason),
                    "Only expected business rejection reasons are allowed.");
            }
            return new LoginResult(null, reason);
        }

        private static bool IsRejection(LoginStatus reason)
        {
            return reason == LoginStatus.InvalidCredentials ||
                reason == LoginStatus.Banned ||
                reason == LoginStatus.Suspended;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            bool isSuccess = Status == LoginStatus.None && Account != null;
            bool isRejection = Account == null && IsRejection(Status);
            if (!isSuccess && !isRejection)
            {
                throw new SerializationException("Contradictory operation result.");
            }
        }
    }
}
