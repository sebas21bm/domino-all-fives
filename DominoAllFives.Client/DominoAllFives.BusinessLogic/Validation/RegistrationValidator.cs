using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using DominoAllFives.Contracts.DTOs;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.BusinessLogic.Validation
{
    /// <summary>
    /// Validates business rules for player account registration.
    /// </summary>
    public static class RegistrationValidator
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
        /// Validates the registration data against the account
        /// registration business rules.
        /// </summary>
        /// <param name="registrationData">
        /// The registration information to validate.
        /// </param>
        /// <returns>
        /// None when the data is valid; otherwise, the reason
        /// why validation failed.
        /// </returns>
        public static RegistrationFailureReason Validate(
            RegisterAccountDto registrationData)
        {
            if (registrationData == null)
            {
                return RegistrationFailureReason.InvalidRegistrationData;
            }

            if (!IsUsernameValid(registrationData.Username))
            {
                return RegistrationFailureReason.InvalidUsername;
            }

            if (!IsEmailValid(registrationData.Email))
            {
                return RegistrationFailureReason.InvalidEmail;
            }

            if (!IsPasswordValid(registrationData.Password))
            {
                return RegistrationFailureReason.InvalidPassword;
            }

            return RegistrationFailureReason.None;
        }

        private static bool IsUsernameValid(string username)
        {
            return !string.IsNullOrWhiteSpace(username) &&
                   username.Length <= MaximumUsernameLength;
        }

        private static bool IsEmailValid(string email)
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

        private static bool IsPasswordValid(string password)
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