using System;

using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.ViewModels;

namespace DominoAllFives.Client.WPF.Services
{
    /// <summary>
    /// Implements modal dialog management by constructing custom message view models.
    /// </summary>
    public class DialogService : IDialogService
    {
        private readonly Action<CustomMessageViewModel> _showDialogCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogService"/> class.
        /// </summary>
        /// <param name="showDialogCallback">The delegate action used to trigger 
        /// UI rendering of the dialog.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when showDialogCallback 
        /// is null.
        /// </exception>
        public DialogService(Action<CustomMessageViewModel> showDialogCallback)
        {
            _showDialogCallback = showDialogCallback ?? 
                throw new ArgumentNullException(nameof(showDialogCallback));
        }

        /// <summary>
        /// Displays a custom modal dialog based on the specified request settings.
        /// </summary>
        /// <param name="request">The dialog configuration details.</param>
        public void ShowDialog(DialogRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string title = LanguageManager.Instance[request.TitleKey] ?? request.TitleKey;
            string message = LanguageManager.Instance[request.MessageKey] ?? request.MessageKey;
            string acceptText = LanguageManager.Instance["Global_btnAccept"] ?? "Accept";
            string cancelText = LanguageManager.Instance["Global_btnCancel"] ?? "Cancel";

            string iconPath = GetIconPath(request.Type);

            string resolvedCancelText = null;
            Action resolvedCancelAction = null;

            if (request.Type == DialogType.Confirmation)
            {
                resolvedCancelText = cancelText;
                resolvedCancelAction = () =>
                {
                    CloseDialog();
                    request.OnCancel?.Invoke();
                };
            }

            var dialogVm = new CustomMessageViewModel
            {
                Title = title,
                Message = message,
                IconPath = iconPath,
                AcceptButtonText = acceptText,
                OnAcceptAction = () =>
                {
                    CloseDialog();
                    request.OnAccept?.Invoke();
                },
                CancelButtonText = resolvedCancelText,
                OnCancelAction = resolvedCancelAction
            };

            _showDialogCallback.Invoke(dialogVm);
        }

        private string GetIconPath(DialogType type)
        {
            switch (type)
            {
                case DialogType.Success:
                    return "../Assets/Icons/iconSuccess.png";
                case DialogType.Error:
                    return "../Assets/Icons/iconError.png";
                case DialogType.Warning:
                    return "../Assets/Icons/iconWarning.png";
                case DialogType.Information:
                    return "../Assets/Icons/iconInformation.png";
                case DialogType.Confirmation:
                    return "../Assets/Icons/iconQuestion.png";
                default:
                    return "../Assets/Icons/iconInformation.png";
            }
        }

        private void CloseDialog()
        {
            _showDialogCallback.Invoke(null);
        }
    }
}
