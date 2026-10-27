using System;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Presentation retained for a future remote use case implementation.</summary>
    public class DeleteAccountViewModel : ViewModelBase
    {
        public bool IsPasswordRequiredVisible { get; set; }
        public RelayCommand ConfirmDeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        public DeleteAccountViewModel(Action onCancel = null)
        {
            ConfirmDeleteCommand = new RelayCommand(() => { }, () => false);
            CancelCommand = new RelayCommand(() => onCancel?.Invoke());
        }
    }
}
