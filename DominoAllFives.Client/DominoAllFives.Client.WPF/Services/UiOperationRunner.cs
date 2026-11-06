using System;
using System.ComponentModel;
using System.Windows;
using System.ServiceModel;
using DominoAllFives.Client.WPF.Localization;

namespace DominoAllFives.Client.WPF.Services
{
    internal static class UiOperationRunner
    {
        public static string ErrorMessage(Exception error)
        {
            string key = "Lobby_msgOperationFailed";
            if (error is FaultException)
            {
                switch (error.Message)
                {
                    case "Lobby not found.": key = "Lobby_msgNotFound"; break;
                    case "Lobby is unavailable or full.": key = "Lobby_msgUnavailable"; break;
                    case "Connection is already in a lobby.": key = "Lobby_msgAlreadyJoined"; break;
                    case "Only the host may perform this operation.": key = "Lobby_msgHostOnly"; break;
                    case "Lobby cannot start.": key = "Lobby_msgCannotStart"; break;
                    case "Invalid lobby settings.": key = "Lobby_msgInvalidSettings"; break;
                }
            }
            return LanguageManager.Instance[key];
        }

        public static void Run<TResult>(
            Func<TResult> work,
            Action<TResult> completed,
            Action<Exception> failed)
        {
            if (work == null) throw new ArgumentNullException(nameof(work));
            if (completed == null) throw new ArgumentNullException(nameof(completed));
            if (failed == null) throw new ArgumentNullException(nameof(failed));

            // Create on the UI thread: RunWorkerCompleted returns to its context.
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, args) =>
            {
                args.Result = work();
            };
            worker.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null) failed(args.Error);
                else completed((TResult)args.Result);
                worker.Dispose();
            };
            worker.RunWorkerAsync();
        }
    }
}
