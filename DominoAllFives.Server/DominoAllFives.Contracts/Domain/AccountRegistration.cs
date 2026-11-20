using System.Runtime.Serialization;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Domain
{
    /// <summary>
    /// Encapsulates account creation data and the existing ordered validation policy.
    /// </summary>
    [DataContract]
    public sealed class AccountRegistration
    {
        [DataMember(Order = 1)]
        public string Username { get; private set; }
        [DataMember(Order = 2)]
        public string Email { get; private set; }
        [DataMember(Order = 3)]
        public string Password { get; private set; }
        [DataMember(Order = 4)]
        public string PreferredLanguageId { get; private set; }

        private AccountRegistration()
        {
        }

        /// <summary>
        /// Captures registration data; EnsureValid evaluates username, email and password in order.
        /// </summary>
        public static AccountRegistration Create(
            string username,
            string email,
            string password,
            string preferredLanguageId)
        {
            AccountRegistration registration = new AccountRegistration
            {
                Username = username,
                Email = email,
                Password = password,
                PreferredLanguageId = preferredLanguageId
            };
            return registration;
        }

        /// <summary>
        /// Preserves the registration failure precedence without querying persistence.
        /// </summary>
        public RegistrationStatus EnsureValid()
        {
            if (!AccountRules.IsUsernameValid(Username))
            {
                return RegistrationStatus.InvalidUsername;
            }

            if (!AccountRules.IsEmailValid(Email))
            {
                return RegistrationStatus.InvalidEmail;
            }

            if (!AccountRules.IsPasswordValid(Password))
            {
                return RegistrationStatus.InvalidPassword;
            }

            return RegistrationStatus.None;
        }
    }
}
