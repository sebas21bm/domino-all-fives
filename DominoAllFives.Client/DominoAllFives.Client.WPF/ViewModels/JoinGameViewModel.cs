using System;
using System.Collections.ObjectModel;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Wcf;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class JoinGameViewModel : ViewModelBase
    {
        private IFrameNavigationService _navigation;
        private LobbySession _lobbySession;
        private bool _isBusy;
        private string _roomCode = string.Empty;
        private bool _isJoinByCodeMode = true;
        private string _errorMessage = string.Empty;

        public bool IsJoinByCodeMode
        {
            get => _isJoinByCodeMode;
            set => SetProperty(ref _isJoinByCodeMode, value);
        }
        public string RoomCode
        {
            get => _roomCode;
            set { if (SetProperty(ref _roomCode, value)) JoinRoomCommand?.RaiseCanExecuteChanged(); }
        }
        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                if (SetProperty(ref _errorMessage, value))
                    OnPropertyChanged(nameof(HasError));
            }
        }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public ObservableCollection<GameInvitationRecord> Invitations { get; }
            = new ObservableCollection<GameInvitationRecord>();
        public RelayCommand GoBackCommand { get; }
        public RelayCommand SwitchModeCommand { get; }
        public RelayCommand JoinRoomCommand { get; }
        public RelayCommand AcceptInvitationCommand { get; }

        public JoinGameViewModel()
        {
            GoBackCommand = new RelayCommand(() => _navigation.GoBack(), () => !_isBusy);
            SwitchModeCommand = new RelayCommand(() => IsJoinByCodeMode = !IsJoinByCodeMode, () => !_isBusy);
            JoinRoomCommand = new RelayCommand(JoinRoom, () => !_isBusy && !string.IsNullOrWhiteSpace(RoomCode));
            AcceptInvitationCommand = new RelayCommand(() => { }, () => false);
        }

        public void Initialize(
            IFrameNavigationService navigation,
            LobbySession lobbySession)
        {
            if (_navigation != null)
            {
                throw new InvalidOperationException(
                    "JoinGameViewModel has already been initialized.");
            }

            if (navigation == null)
                throw new ArgumentNullException(nameof(navigation));
            if (lobbySession == null)
                throw new ArgumentNullException(nameof(lobbySession));

            _navigation = navigation;
            _lobbySession = lobbySession;
        }

        private void JoinRoom()
        {
            if (_isBusy || string.IsNullOrWhiteSpace(RoomCode)) return;
            string code = RoomCode.Trim();
            ErrorMessage = string.Empty;
            _isBusy = true;
            JoinRoomCommand.RaiseCanExecuteChanged();
            UiOperationRunner.Run(
                () => _lobbySession.Join(code),
                lobby =>
                {
                    _isBusy = false;
                    _navigation.NavigateTo<LobbyViewModel>();
                },
                error =>
                {
                    _isBusy = false;
                    ErrorMessage = UiOperationRunner.ErrorMessage(error);
                    JoinRoomCommand.RaiseCanExecuteChanged();
                });
        }
    }
}
