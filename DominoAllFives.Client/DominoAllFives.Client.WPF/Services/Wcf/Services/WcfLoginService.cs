using DominoAllFivesService;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;

namespace DominoAllFives.Client.WPF.Services.Wcf.Services
{
    /// <summary>
    /// Invokes login using client types generated from the server metadata.
    /// </summary>
    public sealed class WcfLoginService : ILoginClient
    {
        public PlayerAccount LoginWithAccount(Credentials credentials)
        {
            return WcfInvoker.Invoke<SessionAccountClient, PlayerAccount>(
                () => new SessionAccountClient("AuthenticationEndpoint"),
                proxy => proxy.LoginWithAccount(credentials));
        }
    }
}
