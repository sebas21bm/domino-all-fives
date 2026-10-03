using System;

using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.ViewModels.Base;


namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>
    /// ViewModel that manages the presentation state and action callbacks 
    /// for custom message dialogs.
    /// </summary>
    public class CustomMessageViewModel : ViewModelBase
    {
        private string _title = string.Empty;
        private string _message = string.Empty;
        private string _iconPath = string.Empty;
        private string _acceptButtonText = string.Empty;
        private string _cancelButtonText = string.Empty;
        private bool _isCancelVisible;

        private Action _onAcceptAction;
        private Action _onCancelAction;

        /// <summary>
        /// Gets or sets the localized title displayed on the dialog.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
        public string Title
        {
            get => _title;
            set
            {
                _title = value ?? throw new ArgumentNullException(nameof(value));
                OnPropertyChanged(nameof(Title));
            }
        }

        /// <summary>
        /// Gets or sets the main message text presented to the user.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
        public string Message
        {
            get => _message;
            set
            {
                _message = value ?? throw new ArgumentNullException(nameof(value));
                OnPropertyChanged(nameof(Message));
            }
        }

        /// <summary>
        /// Gets or sets the asset path for the icon displayed on the dialog.
        /// </summary>
        public string IconPath
        {
            get => _iconPath;
            set => SetProperty(ref _iconPath, value);
        }

        /// <summary>
        /// Gets or sets the text for the primary acceptance button.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
        public string AcceptButtonText
        {
            get => _acceptButtonText;
            set
            {
                _acceptButtonText = value ?? throw new ArgumentNullException(nameof(value));
                OnPropertyChanged(nameof(AcceptButtonText));
            }
        }

        /// <summary>
        /// Gets or sets the text for the secondary cancellation button.
        /// </summary>
        public string CancelButtonText
        {
            get => _cancelButtonText;
            set => SetProperty(ref _cancelButtonText, value ?? string.Empty);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cancellation button is visible.
        /// </summary>
        public bool IsCancelVisible
        {
            get => _isCancelVisible;
            set => SetProperty(ref _isCancelVisible, value);
        }

        /// <summary>
        /// Gets or sets the callback action executed when the user confirms or accepts.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
        public Action OnAcceptAction
        {
            get => _onAcceptAction;
            set => _onAcceptAction = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the callback action executed when the user cancels or dismisses.
        /// </summary>
        public Action OnCancelAction
        {
            get => _onCancelAction;
            set
            {
                _onCancelAction = value;
                IsCancelVisible = _onCancelAction != null && !string.IsNullOrEmpty(
                    CancelButtonText);
                CancelCommand?.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets the command executed upon clicking the acceptance button.
        /// </summary>
        public RelayCommand AcceptCommand { get; }

        /// <summary>
        /// Gets the command executed upon clicking the cancellation button.
        /// </summary>
        public RelayCommand CancelCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMessageViewModel"/> class.
        /// </summary>
        public CustomMessageViewModel()
        {
            AcceptCommand = new RelayCommand(ExecuteAccept);
            CancelCommand = new RelayCommand(ExecuteCancel, () => IsCancelVisible);
        }

        private void ExecuteAccept()
        {
            _onAcceptAction?.Invoke();
        }

        private void ExecuteCancel()
        {
            _onCancelAction?.Invoke();
        }
    }
}
