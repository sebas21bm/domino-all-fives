using System;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.BusinessLogic.Validation
{
    /// <summary>Validates the ordered business rules for account registration.</summary>
    public static class RegistrationValidator
    {
        /// <summary>Validates username, email and password in their established order.</summary>
        /// <param name = "registrationData">The account information supplied by the player.</param>
        /// <exception cref = "ArgumentNullException">The registration data is null.</exception>
        /// <exception cref = "RegistrationRuleException">
        /// An account rule is not satisfied.
        /// </exception>
        public static void Validate(AccountRegistration registrationData)
        {
            if (registrationData == null)
            {
                throw new ArgumentNullException(nameof(registrationData));
            }

            if (!AccountValidator.IsUsernameValid(registrationData.Username))
            {
                throw new RegistrationRuleException(RegistrationIssue.InvalidUsername);
            }

            if (!AccountValidator.IsEmailValid(registrationData.Email))
            {
                throw new RegistrationRuleException(RegistrationIssue.InvalidEmail);
            }

            if (!AccountValidator.IsPasswordValid(registrationData.Password))
            {
                throw new RegistrationRuleException(RegistrationIssue.InvalidPassword);
            }
        }
    }
}
