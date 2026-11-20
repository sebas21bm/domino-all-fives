using DominoAllFivesService;
using RemotePicture = DominoAllFivesService.ProfilePicture;

namespace DominoAllFives.Client.WPF.Services.Wcf.Interfaces
{
    /// <summary>Requests profile operations for an authenticated player.</summary>
    public interface IProfileClient
    {
        /// <summary>Gets the profile owned by the supplied session.</summary>
        PlayerProfile GetMyProfile(string sessionKey);

        /// <summary>Updates the username while preserving the stored picture.</summary>
        void UpdateProfileName(string sessionKey, string username);

        /// <summary>Updates the username and replaces its picture in one operation.</summary>
        void UpdateProfileWithPicture(string sessionKey, string username, RemotePicture picture);

        /// <summary>Sets or replaces the profile picture.</summary>
        void SetProfilePicture(string sessionKey, RemotePicture picture);

        /// <summary>Removes the stored profile picture.</summary>
        void RemoveProfilePicture(string sessionKey);
    }
}
