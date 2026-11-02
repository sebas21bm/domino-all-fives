using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services.Wcf
{
    /// <summary>Invokes login using client types generated from the server metadata.</summary>
    public sealed class WcfLoginService : ILoginClient
    {
        public LoginResult Login(LoginCredentials credentials)
        {
            return WcfInvoker.Invoke<AuthenticationServiceClient, LoginResult>(
                () => new AuthenticationServiceClient("AuthenticationEndpoint"), service => service.Login(credentials));
        }
    }
}