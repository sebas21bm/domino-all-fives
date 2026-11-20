using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFivesService;
using RemotePicture = DominoAllFivesService.ProfilePicture;

namespace DominoAllFives.Client.WPF.Services.Wcf.Services
{
    /// <summary>Invokes profile operations through a generated proxy per request.</summary>
    public sealed class ProfileClient : IProfileClient
    {
        /// <inheritdoc/>
        public PlayerProfile GetMyProfile(string sessionKey)
        {
            return WcfInvoker.Invoke<ProfileServiceClient, PlayerProfile>(
                () => new ProfileServiceClient("PlayerProfileEndpoint"),
                proxy => proxy.GetMyProfile(sessionKey));
        }

        /// <inheritdoc/>
        public void UpdateProfileName(string sessionKey, string username)
        {
            WcfInvoker.Invoke<ProfileServiceClient>(
                () => new ProfileServiceClient("PlayerProfileEndpoint"),
                proxy => proxy.UpdateProfileName(sessionKey, username));
        }

        /// <inheritdoc/>
        public void UpdateProfileWithPicture(
            string sessionKey,
            string username,
            RemotePicture picture)
        {
            WcfInvoker.Invoke<ProfileServiceClient>(
                () => new ProfileServiceClient("PlayerProfileEndpoint"),
                proxy => proxy.UpdateProfileWithPicture(sessionKey, username, picture));
        }

        /// <inheritdoc/>
        public void SetProfilePicture(string sessionKey, RemotePicture picture)
        {
            WcfInvoker.Invoke<ProfileServiceClient>(
                () => new ProfileServiceClient("PlayerProfileEndpoint"),
                proxy => proxy.SetProfilePicture(sessionKey, picture));
        }

        /// <inheritdoc/>
        public void RemoveProfilePicture(string sessionKey)
        {
            WcfInvoker.Invoke<ProfileServiceClient>(
                () => new ProfileServiceClient("PlayerProfileEndpoint"),
                proxy => proxy.RemoveProfilePicture(sessionKey));
        }
    }
}
