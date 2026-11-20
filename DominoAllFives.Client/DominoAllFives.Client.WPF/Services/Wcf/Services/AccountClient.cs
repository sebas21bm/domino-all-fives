using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Services
{
    /// <summary>Invokes account operations through the configured WCF endpoint.</summary>
    public sealed class AccountClient : IAccountClient
    {
        public void ChangePassword(string sessionKey, string currentPassword, string newPassword)
        {
            WcfInvoker.Invoke<AccountServiceClient>(
                () => new AccountServiceClient("AccountSettingsEndpoint"),
                proxy => proxy.ChangePassword(sessionKey, currentPassword, newPassword));
        }

        public void DeleteAccount(string sessionKey, string password)
        {
            WcfInvoker.Invoke<AccountServiceClient>(
                () => new AccountServiceClient("AccountSettingsEndpoint"),
                proxy => proxy.DeleteAccount(sessionKey, password));
        }
    }
}
