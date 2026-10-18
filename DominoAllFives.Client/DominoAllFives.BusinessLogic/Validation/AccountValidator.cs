using DominoAllFives.Contracts.Domain;

namespace DominoAllFives.BusinessLogic.Validation
{
    /// <summary>
    /// Delegates account policies to the shared domain rules.
    /// </summary>
    public static class AccountValidator
    {
        /// <summary>
        /// Checks the existing username policy.
        /// </summary>
        public static bool IsUsernameValid(string username) => AccountRules.IsUsernameValid(username);

        /// <summary>
        /// Checks the existing email policy.
        /// </summary>
        public static bool IsEmailValid(string email) => AccountRules.IsEmailValid(email);

        /// <summary>
        /// Checks the existing new-password policy.
        /// </summary>
        public static bool IsPasswordValid(string password) => AccountRules.IsPasswordValid(password);
    }
}
