using System.Runtime.Serialization;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Domain
{
    /// <summary>
    /// Represents the credentials required to authenticate an account.
    /// </summary>
    [DataContract]
    public sealed class LoginCredentials
    {
        [DataMember(Order = 1, IsRequired = true)]
        public string EmailOrUsername { get; private set; }
        [DataMember(Order = 2, IsRequired = true)]
        public string Password { get; private set; }

        private LoginCredentials()
        {
        }

        /// <summary>
        /// Creates credentials without applying new-password policies.
        /// </summary>
        /// <param name="identifier">The username or email supplied for login.</param>
        /// <param name="password">The password supplied by the player.</param>
        /// <returns>The domain value produced by the operation.</returns>
        public static LoginCredentials ForLogin(string identifier, string password)
        {
            LoginCredentials credentials = new LoginCredentials
            {
                EmailOrUsername = identifier,
                Password = password
            };
            credentials.EmailOrUsername = identifier?.Trim();
            return credentials;
        }

        /// <summary>
        /// Returns the rejection reason for missing credentials before accessing persistence.
        /// </summary>
        public LoginFailureReason EnsureValid()
        {
            if (string.IsNullOrWhiteSpace(EmailOrUsername) || string.IsNullOrWhiteSpace(Password))
            {
                return LoginFailureReason.InvalidCredentials;
            }
            return LoginFailureReason.None;
        }
    }
}
