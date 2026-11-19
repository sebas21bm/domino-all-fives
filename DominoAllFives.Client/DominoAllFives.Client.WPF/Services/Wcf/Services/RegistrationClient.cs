using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Services
{
    /// <summary>
    /// Invokes registration through the generated request/response proxy.
    /// </summary>
    public sealed class RegistrationClient : IRegistrationClient
    {
        /// <inheritdoc/>
        public PlayerAccount Register(AccountRegistration registrationData)
        {
            return WcfInvoker.Invoke<RegistrationServiceClient, PlayerAccount>(
                () => new RegistrationServiceClient(
                "RegistrationEndpoint"),
                proxy => proxy.Register(registrationData));
        }
    }
}
