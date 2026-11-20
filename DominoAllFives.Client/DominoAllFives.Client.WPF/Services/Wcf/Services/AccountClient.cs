using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Services
{
    /// <summary>Invokes account operations through the configured WCF endpoint.</summary>
    public sealed class AccountClient : IAccountClient
    {
        private const string EndpointName = "AccountSettingsEndpoint";

        /// <inheritdoc/>
        public void ChangePassword(string sessionKey, string currentPassword, string newPassword)
        {
            WcfInvoker.Invoke<AccountServiceClient>(
                () => new AccountServiceClient(EndpointName),
                proxy => proxy.ChangePassword(sessionKey, currentPassword, newPassword));
        }

        /// <inheritdoc/>
        public void DeleteAccount(string sessionKey, string password)
        {
            WcfInvoker.Invoke<AccountServiceClient>(
                () => new AccountServiceClient(EndpointName),
                proxy => proxy.DeleteAccount(sessionKey, password));
        }

        /// <inheritdoc/>
        public void ChangePreferredLanguage(string sessionKey, string preferredLanguageId)
        {
            WcfInvoker.Invoke<AccountServiceClient>(
                () => new AccountServiceClient(EndpointName),
                proxy => proxy.ChangePreferredLanguage(sessionKey, preferredLanguageId));
        }
    }
}
