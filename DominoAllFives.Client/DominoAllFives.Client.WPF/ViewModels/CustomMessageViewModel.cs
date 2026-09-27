using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class CustomMessageViewModel : ViewModelBase
    {
        private string _title = string.Empty;
        private string _message = string.Empty;
        private string _iconPath = string.Empty;
        private string _acceptButtonText = string.Empty;
        private string _cancelButtonText = string.Empty;
        private bool _isCancelVisible;

        private readonly Action _onAcceptAction;
        private readonly Action _onCancelAction;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public string IconPath
        {
            get => _iconPath;
            set => SetProperty(ref _iconPath, value);
        }

        public string AcceptButtonText
        {
            get => _acceptButtonText;
            set => SetProperty(ref _acceptButtonText, value);
        }

        public string CancelButtonText
        {
            get => _cancelButtonText;
            set => SetProperty(ref _cancelButtonText, value);
        }

        public bool IsCancelVisible
        {
            get => _isCancelVisible;
            set => SetProperty(ref _isCancelVisible, value);
        }

        public RelayCommand AcceptCommand { get; }

        public RelayCommand CancelCommand { get; }

        public CustomMessageViewModel(string title, string message, string iconPath, string acceptButtonText,
                                    Action onAcceptAction, string cancelButtonText, Action onCancelAction)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException("Title cannot be null or empty.", nameof(title));
            }
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));
            }
            if (acceptButtonText == null)
            {
                throw new ArgumentNullException(nameof(acceptButtonText));
            }
            if (onAcceptAction == null)
            {
                throw new ArgumentNullException(nameof(onAcceptAction));
            }

            _title = title;
            _message = message;
            _iconPath = iconPath;
            _acceptButtonText = acceptButtonText;
            _onAcceptAction = onAcceptAction;

            _onCancelAction = onCancelAction;
            _cancelButtonText = cancelButtonText ?? string.Empty;
            _isCancelVisible = _onCancelAction != null && !string.IsNullOrEmpty(_cancelButtonText);

            AcceptCommand = new RelayCommand(ExecuteAccept);
            CancelCommand = new RelayCommand(ExecuteCancel, () => _isCancelVisible);
        }

        private void ExecuteAccept()
        {
            _onAcceptAction.Invoke();
        }

        private void ExecuteCancel()
        {
            _onCancelAction?.Invoke();
        }
    }
}
