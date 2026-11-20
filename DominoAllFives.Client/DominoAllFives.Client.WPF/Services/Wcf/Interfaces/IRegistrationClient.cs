using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Interfaces
{
    /// <summary>Requests account creation from the authoritative server.</summary>
    public interface IRegistrationClient
    {
        /// <summary>Registers the supplied account and returns its server session.</summary>
        /// <param name = "registrationData">The captured account information.</param>
        /// <returns>The newly created account.</returns>
        PlayerAccount Register(AccountRegistration registrationData);
    }
}
