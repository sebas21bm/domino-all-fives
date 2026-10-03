using DominoAllFives.Contracts.DTOs;

namespace DominoAllFives.Contracts.Services
{
    /// <summary>
    /// Represents the service responsible for handling account-related operations,
    /// sucha as login, registration, and profile picture management.
    /// </summary>
    public interface IAccountService
    {
        LoginResultDto Login(LoginRequestDto loginRequest);
        
        RegistrationResultDto Register(RegisterAccountDto registrationData);

        ProfilePictureResultDto SetProfilePicture(ProfilePictureDto profilePicture);
    }
}
