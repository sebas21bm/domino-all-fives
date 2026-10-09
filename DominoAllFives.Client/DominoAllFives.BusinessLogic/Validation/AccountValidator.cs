
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DominoAllFives.BusinessLogic.Validation
{
    /// <summary>
    /// Validates business rules for player account data.
    /// </summary>
    public static class AccountValidator
    {
        private const int MaximumUsernameLength = 64;

        private const string EmailPattern =
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        private const string PasswordPattern =
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$";

        private static readonly TimeSpan regexTimeout =
            TimeSpan.FromMilliseconds(250);

        private static readonly HashSet<string> validEmailDomains =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "gmail.com",
                "outlook.com",
                "hotmail.com",
                "yahoo.com",
                "icloud.com"
            };

        /// <summary>
        /// Determines whether a username satisfies the established
        /// business rules.
        /// </summary>
        public static bool IsUsernameValid(string username)
        {
            return !string.IsNullOrWhiteSpace(username) &&
                   username.Length <= MaximumUsernameLength;
        }

        /// <summary>
        /// Determines whether an email satisfies the established
        /// business rules.
        /// </summary>
        public static bool IsEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                !Regex.IsMatch(
                    email,
                    EmailPattern,
                    RegexOptions.None,
                    regexTimeout))
            {
                return false;
            }

            int atIndex = email.LastIndexOf('@');

            if (atIndex < 0 || atIndex == email.Length - 1)
            {
                return false;
            }

            string domain = email.Substring(atIndex + 1);

            return validEmailDomains.Contains(domain);
        }

        /// <summary>
        /// Determines whether a password satisfies the established
        /// business rules.
        /// </summary>
        public static bool IsPasswordValid(string password)
        {
            return !string.IsNullOrWhiteSpace(password) &&
                   Regex.IsMatch(
                       password,
                       PasswordPattern,
                       RegexOptions.None,
                       regexTimeout);
        }
    }
}
