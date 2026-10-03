using System;

using DominoAllFives.Client.WPF.Models;

namespace DominoAllFives.Client.WPF.Services
{
    /// <summary>
    /// Defines operations for presenting custom modal dialogs to the user.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Displays a custom modal dialog based on the specified request settings.
        /// </summary>
        /// <param name="request">The dialog configuration details.</param>
        void ShowDialog(DialogRequest request);
    }
}
