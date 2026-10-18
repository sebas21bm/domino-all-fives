using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Domain
{
    /// <summary>
    /// Encapsulates account creation data and the existing ordered validation policy.
    /// </summary>
    public sealed class AccountRegistration
    {
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }
        public string PreferredLanguageId { get; private set; }

        private AccountRegistration()
        {
        }

        /// <summary>
        /// Captures registration data; EnsureValid evaluates username, email and password in order.
        /// </summary>
        public static AccountRegistration Create(string username, string email,
            string password, string preferredLanguageId)
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
        public RegistrationFailureReason EnsureValid()
        {
            if (!AccountRules.IsUsernameValid(Username))
            {
                return RegistrationFailureReason.InvalidUsername;
            }
            if (!AccountRules.IsEmailValid(Email))
            {
                return RegistrationFailureReason.InvalidEmail;
            }
            if (!AccountRules.IsPasswordValid(Password))
            {
                return RegistrationFailureReason.InvalidPassword;
            }
            return RegistrationFailureReason.None;
        }
    }
}
