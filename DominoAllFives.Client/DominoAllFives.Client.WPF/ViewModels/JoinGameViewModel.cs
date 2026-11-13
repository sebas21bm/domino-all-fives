using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Threading;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.ViewModels.Base;
using LobbyService;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public sealed class JoinGameViewModel : ViewModelBase
    {
        private readonly PlayerSession _playerSession;
        private IFrameNavigationService _navigation;
        private ILobbyClient _lobbyService;
        private IDialogService _dialogService;
        private ILogger<JoinGameViewModel> _logger;
        private bool _isBusy;
        private string _roomCode = string.Empty;
        private bool _isJoinByCodeMode = true;
        private string _errorMessage = string.Empty;
        public bool IsJoinByCodeMode
        {
            get
            {
                return _isJoinByCodeMode;
            }

            set
            {
                SetProperty(ref _isJoinByCodeMode, value);
            }
        }

        public string RoomCode
        {
            get
            {
                return _roomCode;
            }

            set
            {
                if (SetProperty(ref _roomCode, value))
                {
                    JoinRoomCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }

            private set
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    OnPropertyChanged(nameof(HasError));
                }
            }
        }

        public bool HasError
        {
            get
            {
                return !string.IsNullOrEmpty(ErrorMessage);
            }
        }

        public ObservableCollection<GameInvitationRecord> Invitations { get; } =
            new ObservableCollection<GameInvitationRecord>();
        public RelayCommand GoBackCommand { get; }
        public RelayCommand SwitchModeCommand { get; }
        public RelayCommand JoinRoomCommand { get; }
        public RelayCommand AcceptInvitationCommand { get; }

        public JoinGameViewModel(PlayerSession playerSession)
        {
            _playerSession = playerSession ?? throw new ArgumentNullException(
                nameof(
                playerSession));
            GoBackCommand = new RelayCommand(GoBack, CanInteract);
            SwitchModeCommand = new RelayCommand(SwitchMode, CanInteract);
            JoinRoomCommand = new RelayCommand(JoinRoom, CanJoinRoom);
            AcceptInvitationCommand = new RelayCommand(DoNothing, CannotExecute);
        }

        public void Initialize(
            IFrameNavigationService navigation,
            ILobbyClient lobbyService,
            IDialogService dialogService,
            ILogger<JoinGameViewModel> logger)
        {
            if (_lobbyService != null)
            {
                throw new InvalidOperationException();
            }

            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _lobbyService = lobbyService ?? throw new ArgumentNullException(nameof(lobbyService));
            _dialogService = dialogService ?? throw new ArgumentNullException(
                nameof(
                dialogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            RaiseCommands();
        }

        private void GoBack()
        {
            _navigation.GoBack();
        }

        private void SwitchMode()
        {
            IsJoinByCodeMode = !IsJoinByCodeMode;
        }

        private void DoNothing()
        {
        }

        private bool CannotExecute()
        {
            return false;
        }

        private bool CanInteract()
        {
            return _lobbyService != null && !_isBusy;
        }

        private bool CanJoinRoom()
        {
            return CanInteract()
                && _playerSession.HasServerSession
                && !string.IsNullOrWhiteSpace(
                RoomCode);
        }

        private void JoinRoom()
        {
            if (!JoinRoomCommand.CanExecute(null))
            {
                return;
            }

            string code = RoomCode.Trim().ToUpperInvariant();
            ErrorMessage = string.Empty;
            SetBusy(true);
            BackgroundJob<string, Lobby> job =
                new BackgroundJob<string, Lobby>(
                value => _lobbyService.JoinLobby(
                value),
                code,
                OnLobbyReady,
                OnLobbyFailed);
            BackgroundWork.Start(job);
        }

        private void HandleFailure(Exception error)
        {
            FaultException<LobbyIssue> lobbyFault = error as FaultException<LobbyIssue>;
            if (lobbyFault != null)
            {
                _logger.LogWarning(lobbyFault, "Join lobby rejected: {Issue}.", lobbyFault.Detail);
                string key = GetLobbyFailureKey(lobbyFault.Detail);
                Show(DialogType.Warning, key);
                return;
            }

            _logger.LogError(error, "Lobby could not be joined.");
            bool connection = error is CommunicationException || error is TimeoutException;
            Show(
                DialogType.Error,
                connection
                ? "Global_msgConnectionError" : "Lobby_msgOperationFailed",
                connection
                ? "Global_msgConnectionErrorTitle" : "Global_msgDefaultErrorTitle");
        }

        private void Show(
            DialogType type,
            string message,
            string title =
            "Global_msgDefaultErrorTitle")
        {
            _dialogService.ShowDialog(
                new DialogRequest { Type = type, TitleKey = title, MessageKey = message });
        }

        private void SetBusy(bool value)
        {
            _isBusy = value;
            RaiseCommands();
        }

        private void RaiseCommands()
        {
            GoBackCommand.RaiseCanExecuteChanged();
            SwitchModeCommand.RaiseCanExecuteChanged();
            JoinRoomCommand.RaiseCanExecuteChanged();
        }

        private void OnLobbyReady(Lobby lobby, string state)
        {
            SetBusy(false);
            _navigation.NavigateTo<LobbyViewModel>();
        }

        private void OnLobbyFailed(Exception error, string state)
        {
            SetBusy(false);
            HandleFailure(error);
        }

        private static string GetLobbyFailureKey(LobbyIssue issue)
        {
            if (issue == LobbyIssue.LobbyNotFound)
            {
                return "Lobby_msgNotFound";
            }

            return issue == LobbyIssue.PlayerAlreadyInLobby
                ? "Lobby_msgAlreadyJoined" : "Lobby_msgUnavailable";
        }
    }
}
