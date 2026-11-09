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
                () =>
                {
                    var proxy = new SessionAccountClient("AuthenticationEndpoint");

                    string actualAddress = proxy.Endpoint.Address.Uri.ToString();

                    return proxy;
                },
                proxy => proxy.LoginWithAccount(credentials));
        }
    }
}
