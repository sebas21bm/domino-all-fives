using System;
using System.ServiceModel;
using DominoAllFives.Client.WPF.Models;
using DominoAllFivesService;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.ViewModels
{
    internal static class AccountFailureDialogs
    {
        internal static DialogRequest ForPasswordChange(Exception error, ILogger logger)
        {
            return FromException(error, logger, "Password change", isPasswordChange: true);
        }

        internal static DialogRequest ForAccountDeletion(Exception error, ILogger logger)
        {
            return FromException(error, logger, "Account deletion", isPasswordChange: false);
        }

        private static DialogRequest FromException(
            Exception error,
            ILogger logger,
            string operation,
            bool isPasswordChange)
        {
            if (error is FaultException<AccountIssue> accountFault)
            {
                logger.LogDebug(
                    error,
                    "{Operation} rejected. Account issue: {AccountIssue}.",
                    operation,
                    accountFault.Detail);
                return ForAccountIssue(accountFault.Detail, isPasswordChange);
            }

            if (error is FaultException<AccessIssue> accessFault)
            {
                logger.LogDebug(
                    error,
                    "{Operation} rejected. Access issue: {AccessIssue}.",
                    operation,
                    accessFault.Detail);
                if (accessFault.Detail == AccessIssue.SessionRequired)
                {
                    return AccountNotFound();
                }

                return FailureDialogs.ForTechnicalFailure(connectionFailure: false);
            }

            logger.LogError(error, "{Operation} could not complete.", operation);
            return FailureDialogs.ForTechnicalFailure(FailureDialogs.IsConnectionFailure(error));
        }

        private static DialogRequest ForAccountIssue(AccountIssue issue, bool isPasswordChange)
        {
            switch (issue)
            {
                case AccountIssue.IncorrectCurrentPassword:
                    return FailureDialogs.Create(
                        DialogType.Warning,
                        "MessageAccount_msgWrongCurrentPasswordTitle",
                        "MessageAccount_msgWrongCurrentPassword");
                case AccountIssue.PlayerNotFound:
                    return AccountNotFound();
                case AccountIssue.InvalidPassword when isPasswordChange:
                    return FailureDialogs.Create(
                        DialogType.Warning,
                        "MessageAccount_msgUnsafePasswordTitle",
                        "MessageAccount_msgUnsafePassword");
                case AccountIssue.PasswordUnchanged when isPasswordChange:
                    return FailureDialogs.Create(
                        DialogType.Warning,
                        "MessageAccount_msgSamePasswordTitle",
                        "MessageAccount_msgSamePassword");
                default:
                    return FailureDialogs.ForTechnicalFailure(connectionFailure: false);
            }
        }

        private static DialogRequest AccountNotFound()
        {
            return FailureDialogs.Create(
                DialogType.Warning,
                "MessageAccount_msgAccountNotFoundTitle",
                "MessageAccount_msgAccountNotFound");
        }
    }
}
