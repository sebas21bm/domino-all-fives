using DominoAllFives.Client.WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoAllFives.Client.WPF.Services
{
    public interface IDialogService
    {
        void ShowDialog(DialogType type, string titleKey, string messageKey, Action onAccept, Action onCancel = null);
    }
}
