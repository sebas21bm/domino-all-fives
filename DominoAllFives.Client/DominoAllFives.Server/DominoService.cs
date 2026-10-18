using System;
using System.Data.SqlClient;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.ServiceModel;
using DominoAllFives.Server.Controllers;
using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Wcf;
using Microsoft.Extensions.Logging.Abstractions;

namespace DominoAllFives.Server
{
    /// <summary>Server boundary for the two remote use cases.</summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall,
        IncludeExceptionDetailInFaults = false)]
    public partial class DominoService : IAuthenticationService, IRankingRemoteService
    {
        public LoginResult Login(LoginCredentials credentials)
        {
            if (credentials == null)
            {
                throw new FaultException("Invalid request.");
            }

            try
            {
                return new AuthenticationController(
                    NullLogger<AuthenticationController>.Instance).Login(credentials);
            }
            catch (Exception ex) when (
                            ex is SqlException ||
                            ex is EntityException ||
                            ex is DbUpdateException)
            {
                Console.Error.WriteLine("Login failed on server ({0}).", ex.GetType().Name);
                throw new FaultException("The service could not complete the operation.");
            }
        }

        public Ranking GetTopRanking(int playerId)
        {
            if (playerId <= 0)
            {
                throw new FaultException("Invalid request.");
            }

            try
            {
                ProfilePictureStorage storage = new ProfilePictureStorage(
                    NullLogger<ProfilePictureStorage>.Instance);
                return new RankingController(storage).GetRanking(playerId);
            }
            catch (Exception ex) when (
                            ex is SqlException ||
                            ex is EntityException ||
                            ex is DbUpdateException)
            {
                Console.Error.WriteLine("Ranking failed on server ({0}).", ex.GetType().Name);
                throw new FaultException("The service could not complete the operation.");
            }
        }
    }
}
