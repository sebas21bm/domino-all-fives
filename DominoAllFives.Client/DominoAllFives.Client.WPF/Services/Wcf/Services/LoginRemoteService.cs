using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Services
{
    /// <summary>
    /// Invokes login using client types generated from the server metadata.
    /// </summary>
    public sealed class LoginRemoteService : ILoginClient
    {
        public PlayerAccount LoginWithAccount(Credentials credentials)
        {
            return WcfInvoker.Invoke<SessionAccountClient, PlayerAccount>(
                () => new SessionAccountClient(
                "AuthenticationEndpoint"),
                proxy => proxy.LoginWithAccount(
                credentials));
        }
    }
}
