namespace DominoAllFives.BusinessLogic.Validation
{
    /// <summary>
    /// Validates business rules for player usernames.
    /// </summary>
    public static class UsernameValidator
    {
        private const int MaximumUsernameLength = 64;

        /// <summary>
        /// Determines whether a username satisfies the established
        /// business rules.
        /// </summary>
        /// <param name="username">
        /// The username to validate.
        /// </param>
        /// <returns>
        /// True when the username is valid; otherwise, false.
        /// </returns>
        public static bool IsValid(string username)
        {
            return !string.IsNullOrWhiteSpace(username) &&
                   username.Length <= MaximumUsernameLength;
        }
    }
}