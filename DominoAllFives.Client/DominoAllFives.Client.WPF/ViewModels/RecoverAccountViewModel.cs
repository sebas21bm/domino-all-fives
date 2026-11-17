using System;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Presentation retained for a future remote use case implementation.</summary>
    public class RecoverAccountViewModel : ViewModelBase
    {
        public string Email { get; set; } = string.Empty;
        public RelayCommand SendCodeCommand { get; }
        public RelayCommand CancelCommand { get; }

        public RecoverAccountViewModel(Action onCancel = null)
        {
            SendCodeCommand = new RelayCommand(IgnoreUnavailableCommand, () => false);
            CancelCommand = new RelayCommand(() => onCancel?.Invoke());
        }
        private static void IgnoreUnavailableCommand()
        {
        }
    }
}
