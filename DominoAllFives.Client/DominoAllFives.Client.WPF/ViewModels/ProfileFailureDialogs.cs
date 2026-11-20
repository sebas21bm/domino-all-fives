using System;
using System.ServiceModel;
using DominoAllFives.Client.WPF.Models;
using DominoAllFivesService;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.ViewModels
{
    internal static class ProfileFailureDialogs
    {
        internal static void HandleRemoteFailure(
            Exception ex,
            ILogger logger,
            string operation,
            Action<ProfileIssue?, bool> showFailure)
        {
            if (ex is FaultException<ProfileIssue> profileFault)
            {
                logger.LogDebug(
                    ex,
                    "{Operation} rejected. Issue: {ProfileIssue}.",
                    operation,
                    profileFault.Detail);
                showFailure(profileFault.Detail, false);
                return;
            }

            if (ex is FaultException<AccessIssue>)
            {
                logger.LogDebug(ex, "{Operation} requires an authorized session.", operation);
                showFailure(ProfileIssue.PlayerNotFound, false);
                return;
            }

            logger.LogError(ex, "{Operation} could not complete.", operation);
            showFailure(null, FailureDialogs.IsConnectionFailure(ex));
        }

        internal static DialogRequest ProfileNotFound()
        {
            return FailureDialogs.Create(
                DialogType.Error,
                "Global_msgNotFoundTitle",
                "Global_msgNotFound");
        }

        internal static DialogRequest ForPictureOrTechnicalFailure(
            ProfileIssue? issue,
            bool connectionFailure)
        {
            switch (issue)
            {
                case ProfileIssue.InvalidProfilePictureFormat:
                    return FailureDialogs.Create(
                        DialogType.Warning,
                        "MessageProfile_msgPhotoInvalidFormatTitle",
                        "MessageProfile_msgPhotoInvalidFormat");
                case ProfileIssue.ProfilePictureTooLarge:
                    return FailureDialogs.Create(
                        DialogType.Warning,
                        "MessageProfile_msgPhotoTooLargeTitle",
                        "MessageProfile_msgPhotoTooLarge");
                case ProfileIssue.InvalidProfilePicture:
                    return FailureDialogs.Create(
                        DialogType.Warning,
                        "MessageProfile_msgPhotoInvalidTitle",
                        "MessageProfile_msgPhotoInvalid");
                default:
                    return FailureDialogs.ForTechnicalFailure(!issue.HasValue && connectionFailure);
            }
        }
    }
}
