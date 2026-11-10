using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Threading;
using DominoAllFives.Client.WPF.Services.Wcf;
using DominoAllFives.Client.WPF.ViewModels.Base;
using LobbyService;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public sealed class JoinGameViewModel : ViewModelBase
    {
        private readonly PlayerSession _playerSession;
        private IFrameNavigationService _navigation;
        private LobbySession _lobbySession;
        private IDialogService _dialogService;
        private ILogger<JoinGameViewModel> _logger;
        private bool _isBusy;
        private string _roomCode = string.Empty;
        private bool _isJoinByCodeMode = true;
        private string _errorMessage = string.Empty;

        public bool IsJoinByCodeMode { get => _isJoinByCodeMode;
            set => SetProperty(ref _isJoinByCodeMode, value); }
        public string RoomCode
        {
            get => _roomCode;
            set { if (SetProperty(ref _roomCode, value)) JoinRoomCommand.RaiseCanExecuteChanged(); }
        }
        public string ErrorMessage { get => _errorMessage;
            private set { if (SetProperty(ref _errorMessage, value))
                    OnPropertyChanged(nameof(HasError)); } }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public ObservableCollection<GameInvitationRecord> Invitations { get; } =
            new ObservableCollection<GameInvitationRecord>();
        public RelayCommand GoBackCommand { get; }
        public RelayCommand SwitchModeCommand { get; }
        public RelayCommand JoinRoomCommand { get; }
        public RelayCommand AcceptInvitationCommand { get; }

        public JoinGameViewModel(PlayerSession playerSession)
        {
            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));
            GoBackCommand = new RelayCommand(() => _navigation.GoBack(), CanInteract);
            SwitchModeCommand = new RelayCommand(
                () => IsJoinByCodeMode = !IsJoinByCodeMode, CanInteract);
            JoinRoomCommand = new RelayCommand(JoinRoom, () => CanInteract() &&
                _playerSession.HasServerSession && !string.IsNullOrWhiteSpace(RoomCode));
            AcceptInvitationCommand = new RelayCommand(() => { }, () => false);
        }

        public void Initialize(IFrameNavigationService navigation, LobbySession lobbySession,
            IDialogService dialogService, ILogger<JoinGameViewModel> logger)
        {
            if (_lobbySession != null) throw new InvalidOperationException();
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _lobbySession = lobbySession ?? throw new ArgumentNullException(nameof(lobbySession));
            _dialogService = dialogService
                ?? throw new ArgumentNullException(nameof(dialogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            RaiseCommands();
        }

        private bool CanInteract() => _lobbySession != null && !_isBusy;

        private void JoinRoom()
        {
            if (!JoinRoomCommand.CanExecute(null)) return;
            string code = RoomCode.Trim().ToUpperInvariant();
            ErrorMessage = string.Empty;
            SetBusy(true);
            BackgroundWork.Start(new BackgroundJob<string, Lobby>(
                value => _lobbySession.Join(value), code,
                (lobby, value) => { SetBusy(false); _navigation.NavigateTo<LobbyViewModel>(); },
                (error, value) => { SetBusy(false); HandleFailure(error); }));
        }

        private void HandleFailure(Exception error)
        {
            FaultException<LobbyIssue> lobbyFault = error as FaultException<LobbyIssue>;
            if (lobbyFault != null)
            {
                _logger.LogWarning(lobbyFault, "Join lobby rejected: {Issue}.", lobbyFault.Detail);
                string key = lobbyFault.Detail == LobbyIssue.LobbyNotFound
                    ? "Lobby_msgNotFound" : lobbyFault.Detail == LobbyIssue.PlayerAlreadyInLobby
                    ? "Lobby_msgAlreadyJoined" : "Lobby_msgUnavailable";
                Show(DialogType.Warning, key);
                return;
            }
            _logger.LogError(error, "Lobby could not be joined.");
            bool connection = error is CommunicationException || error is TimeoutException;
            Show(DialogType.Error, connection ? "Global_msgConnectionError" :
                "Lobby_msgOperationFailed", connection ? "Global_msgConnectionErrorTitle" :
                "Global_msgDefaultErrorTitle");
        }

        private void Show(DialogType type, string message,
            string title = "Global_msgDefaultErrorTitle")
        {
            _dialogService.ShowDialog(new DialogRequest
            {
                Type = type,
                TitleKey = title,
                MessageKey = message
            });
        }

        private void SetBusy(bool value) { _isBusy = value; RaiseCommands(); }
        private void RaiseCommands()
        {
            GoBackCommand.RaiseCanExecuteChanged();
            SwitchModeCommand.RaiseCanExecuteChanged();
            JoinRoomCommand.RaiseCanExecuteChanged();
        }
    }
}
