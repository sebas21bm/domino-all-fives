using System;

using DominoAllFives.Client.WPF.Models;

namespace DominoAllFives.Client.WPF.Services
{

    public interface IDialogService
    {
        void ShowDialog(DialogType type, string titleKey, string messageKey, 
            Action onAccept, Action onCancel = null);
    }
}
