using System;
using System.ServiceModel;
using DominoAllFives.Client.WPF.Models;

namespace DominoAllFives.Client.WPF.ViewModels
{
    internal static class FailureDialogs
    {
        internal static bool IsConnectionFailure(Exception error)
        {
            return error is TimeoutException
                || (error is CommunicationException && !(error is FaultException));
        }

        internal static DialogRequest ForTechnicalFailure(bool connectionFailure)
        {
            if (connectionFailure)
            {
                return Create(
                    DialogType.Error,
                    "Global_msgConnectionErrorTitle",
                    "Global_msgConnectionError");
            }

            return Create(DialogType.Error, "Global_msgDefaultErrorTitle", "Global_msgDefaultError");
        }

        internal static DialogRequest Create(DialogType type, string titleKey, string messageKey)
        {
            return new DialogRequest
            {
                Type = type,
                TitleKey = titleKey,
                MessageKey = messageKey
            };
        }
    }
}
