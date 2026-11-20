using System.ServiceModel;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Services
{
    /// <summary>Exposes account creation over WCF.</summary>
    [ServiceContract(Namespace = "urn:domino-all-fives:services")]
    public interface IRegistrationService
    {
        /// <summary>Creates an account and opens its server session.</summary>
        /// <param name = "registrationData">The account information supplied by the player.</param>
        /// <returns>The created account with its server session key.</returns>
        /// <exception cref = "FaultException{RegistrationIssue}">
        /// The registration was rejected.
        /// </exception>
        /// <exception cref = "FaultException">The server could not complete registration.</exception>
        [OperationContract]
        [FaultContract(typeof(RegistrationIssue))]
        PlayerAccount Register(AccountRegistration registrationData);
    }
}
