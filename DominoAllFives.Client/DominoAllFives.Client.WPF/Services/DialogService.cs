using System;

using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.ViewModels;

namespace DominoAllFives.Client.WPF.Services
{
    public class DialogService : IDialogService
    {
        private readonly Action<CustomMessageViewModel> _showDialogCallback;

        public DialogService(Action<ViewModels.CustomMessageViewModel> showDialogCallback)
        {
            if (showDialogCallback == null)
            {
                throw new ArgumentNullException(nameof(showDialogCallback));
            }
            _showDialogCallback = showDialogCallback;
        }

        public void ShowDialog(DialogType type, string titleKey, string messageKey, 
            Action onAccept, Action onCancel = null)
        {
            string title = LanguageManager.Instance[titleKey] ?? titleKey;
            string message = LanguageManager.Instance[messageKey] ?? messageKey;
            string acceptText = LanguageManager.Instance["Global_btnAccept"] ?? "Accept";
            string cancelText = LanguageManager.Instance["Global_btnCancel"] ?? "Cancel";

            string iconPath = GetIconPath(type);

            string resolvedCancelText = null;
            Action resolvedCancelAction = null;

            if (type == DialogType.Confirmation)
            {
                resolvedCancelText = cancelText;
                resolvedCancelAction = () =>
                {
                    CloseDialog();
                    onCancel?.Invoke();
                };
            }

            var dialogVm = new CustomMessageViewModel(
                title,
                message,
                iconPath,
                acceptText,
                () =>
                {
                    CloseDialog();
                    onAccept?.Invoke();
                },
                resolvedCancelText,
                resolvedCancelAction
            );

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
