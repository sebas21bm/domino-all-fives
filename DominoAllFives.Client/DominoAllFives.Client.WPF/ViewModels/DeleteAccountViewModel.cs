using System;

using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class DeleteAccountViewModel : ViewModelBase
    {
        private readonly Action _onAccountDeletedSuccess;
        private readonly Action _onCancel;

        public RelayCommand ConfirmDeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        public DeleteAccountViewModel(
            Action onAccountDeletedSuccess,
            Action onCancel)
        {
            _onAccountDeletedSuccess = onAccountDeletedSuccess ?? 
                throw new ArgumentNullException(nameof(onAccountDeletedSuccess));
            _onCancel = onCancel ?? throw new ArgumentNullException(nameof(onCancel));

            ConfirmDeleteCommand = new RelayCommand(ExecuteConfirmDelete);
            CancelCommand = new RelayCommand(_onCancel);
        }

        private void ExecuteConfirmDelete(object parameter)
        {
            _onAccountDeletedSuccess?.Invoke();
        }
    }
}