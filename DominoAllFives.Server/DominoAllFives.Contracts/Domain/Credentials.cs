using System.Runtime.Serialization;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Domain
{
    /// <summary>
    /// Represents the credentials required to authenticate an account.
    /// </summary>
    [DataContract]
    public sealed class Credentials
    {
        [DataMember(Order = 1, IsRequired = true)]
        public string EmailOrUsername { get; private set; }

        [DataMember(Order = 2, IsRequired = true)]
        public string Password { get; private set; }

        private Credentials()
        {
        }

        /// <summary>
        /// Creates credentials without applying new-password policies.
        /// </summary>
        /// <param name = "identifier">The username or email supplied for login.</param>
        /// <param name = "password">The password supplied by the player.</param>
        /// <returns>The domain value produced by the operation.</returns>
        public static Credentials ForLogin(string identifier, string password)
        {
            Credentials credentials = new Credentials
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
        public LoginStatus EnsureValid()
        {
            if (string.IsNullOrWhiteSpace(EmailOrUsername) || string.IsNullOrWhiteSpace(Password))
            {
                return LoginStatus.InvalidCredentials;
            }

            return LoginStatus.None;
        }
    }
}
