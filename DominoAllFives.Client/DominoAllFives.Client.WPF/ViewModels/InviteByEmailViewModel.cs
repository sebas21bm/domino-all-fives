using System;
using System.Net.Mail;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public sealed class InviteByEmailViewModel : ViewModelBase
    {
        private readonly Action<string> _sendInvitation;
        private string _recipientEmail = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isBusy;

        public RelayCommand CloseCommand { get; }
        public RelayCommand SendCommand { get; }

        public string RecipientEmail
        {
            get => _recipientEmail;
            set
            {
                SetProperty(ref _recipientEmail, value);
                SendCommand.RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public bool IsBusy => _isBusy;

        public InviteByEmailViewModel(Action close, Action<string> sendInvitation)
        {
            _sendInvitation = sendInvitation;
            CloseCommand = new RelayCommand(close, CanClose);
            SendCommand = new RelayCommand(SendInvitation, CanSend);
        }

        public void SetStatus(string key)
        {
            StatusMessage = LanguageManager.Instance[key];
        }

        public void SetBusy(bool isBusy)
        {
            SetProperty(ref _isBusy, isBusy, nameof(IsBusy));
            SendCommand.RaiseCanExecuteChanged();
            CloseCommand.RaiseCanExecuteChanged();
        }

        private bool CanClose()
        {
            return !IsBusy;
        }

        private bool CanSend()
        {
            return !IsBusy && !string.IsNullOrWhiteSpace(RecipientEmail);
        }

        private void SendInvitation()
        {
            if (!CanSend())
            {
                return;
            }

            string address = RecipientEmail.Trim();
            try
            {
                MailAddress parsed = new MailAddress(address);
                if (!string.Equals(parsed.Address, address, StringComparison.Ordinal))
                {
                    SetStatus("InviteByEmail_msgInvalidAddress");
                    return;
                }
            }
            catch (FormatException)
            {
                SetStatus("InviteByEmail_msgInvalidAddress");
                return;
            }

            _sendInvitation(address);
        }
    }
}
