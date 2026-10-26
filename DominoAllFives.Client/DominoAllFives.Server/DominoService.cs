using System.Collections.Generic;
using System;
using System.Data.SqlClient;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.ServiceModel;
using DominoAllFives.Server.Controllers;
using DominoAllFives.Server.Exceptions;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Wcf;
using Microsoft.Extensions.Logging.Abstractions;

namespace DominoAllFives.Server
{
    /// <summary>
    /// Server boundary for the remote use cases.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall,
        IncludeExceptionDetailInFaults = false)]
    public partial class DominoService : ISessionAccount
    {
        public PlayerAccount LoginWithAccount(
            Credentials credentials)
        {
            try
            {
                AuthenticationController controller =
                    new AuthenticationController();

                return controller.LoginWithAccount(credentials);
            }
            catch (AccountRuleException exception)
            {
                throw new FaultException<AccountIssue>(
                    exception.Issue,
                    new FaultReason("The account cannot sign in."));
            }
            catch (Exception exception) when (
                exception is SqlException ||
                exception is EntityException ||
                exception is DbUpdateException)
            {
                Console.Error.WriteLine(
                    "Login failed on server: {0}", exception);

                throw new FaultException(
                    "The service could not complete the operation.");
            }
        }
    }

    public partial class DominoService :  IRankingRemoteService
    {
        public List<RankingEntry> GetTopWinsRanking()
        {
            try
            {
                ProfilePictureStorage storage = new ProfilePictureStorage(
                    NullLogger<ProfilePictureStorage>.Instance);
                return new RankingController(storage).GetTopWinsRanking();
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

        public int GetWinsRankingPositionById(int playerId)
        {
            if (playerId <= 0)
            {
                throw new FaultException("Invalid request.");
            }

            try
            {
                ProfilePictureStorage storage = new ProfilePictureStorage(
                    NullLogger<ProfilePictureStorage>.Instance);
                return new RankingController(storage).GetWinsRankingPositionById(playerId);
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
