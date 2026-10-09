
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.BusinessLogic.Validation
{
    /// <summary>
    /// Validates business rules for player account registration.
    /// </summary>
    public static class RegistrationValidator
    {
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

            if (!AccountValidator.IsUsernameValid(registrationData.Username))
            {
                return RegistrationFailureReason.InvalidUsername;
            }

            if (!AccountValidator.IsEmailValid(registrationData.Email))
            {
                return RegistrationFailureReason.InvalidEmail;
            }

            if (!AccountValidator.IsPasswordValid(registrationData.Password))
            {
                return RegistrationFailureReason.InvalidPassword;
            }

            return RegistrationFailureReason.None;
        }
    }
}
