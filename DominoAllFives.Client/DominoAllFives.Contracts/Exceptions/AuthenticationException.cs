using System;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Exceptions
{
    public class AuthenticationException : Exception
    {
        public LoginFailureReason Reason { get; }

        public AuthenticationException(
            LoginFailureReason reason)
            : base("Authentication failed.")
        {
            Reason = reason;
        }

        public AuthenticationException(
            LoginFailureReason reason,
            Exception innerException)
            : base("Authentication failed.", innerException)
        {
            Reason = reason;
        }
    }
}
