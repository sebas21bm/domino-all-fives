using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.ProfilePicture;
using DominoAllFives.Client.WPF.Services.Threading;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.ViewModels.Base;
using LobbyService;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public sealed class LobbyViewModel : ViewModelBase, IDisposable
    {
        private const int MinimumPlayersToStart = 2;
        private readonly PlayerSession _playerSession;
        private IFrameNavigationService _navigation;
        private ILobbyClient _client;
        private ProfilePictureImageService _pictureService;
        private IDialogService _dialogService;
        private ILogger<LobbyViewModel> _logger;
        private bool _isBusy;
        private bool _disposed;
        private string _roomCode = string.Empty;
        private bool _isHost;
        private bool _isModalVisible;
        private ViewModelBase _currentModal;
        private string _statusMessage = string.Empty;
        public ViewModelBase CurrentModal
        {
            get
            {
                return _currentModal;
            }

            set
            {
                SetProperty(ref _currentModal, value);
            }
        }

        public bool IsModalVisible
        {
            get
            {
                return _isModalVisible;
            }

            set
            {
                SetProperty(ref _isModalVisible, value);
            }
        }

        public string RoomCode
        {
            get
            {
                return _roomCode;
            }

            private set
            {
                SetProperty(ref _roomCode, value);
            }
        }

        public bool IsHost
        {
            get
            {
                return _isHost;
            }

            private set
            {
                SetProperty(ref _isHost, value);
            }
        }

        public string StatusMessage
        {
            get
            {
                return _statusMessage;
            }

            private set
            {
                SetProperty(ref _statusMessage, value);
            }
        }

        public ObservableCollection<LobbyPlayerRecord> Players { get; } =
            new ObservableCollection<LobbyPlayerRecord>();
        public RelayCommand GoBackCommand { get; }
        public RelayCommand InviteFriendsCommand { get; }
        public RelayCommand StartGameCommand { get; }
        public RelayCommand ViewProfileCommand { get; }
        public RelayCommand KickPlayerCommand { get; }

        public LobbyViewModel(PlayerSession playerSession)
        {
            _logger = LoggerFactoryProvider.CreateLogger<LobbyViewModel>();
            _playerSession = playerSession ?? throw new ArgumentNullException(nameof(
                playerSession));
            GoBackCommand = new RelayCommand(Leave, CanLeave);
            InviteFriendsCommand = new RelayCommand(DoNothing, CannotExecute);
            StartGameCommand = new RelayCommand(StartGame, CanStartGame);
            ViewProfileCommand = new RelayCommand(DoNothing, CannotExecute);
            KickPlayerCommand = new RelayCommand(DoNothing, CannotExecute);
        }

        public void Initialize(IFrameNavigationService navigation, ILobbyClient lobbyService,
            ProfilePictureImageService pictureService, IDialogService dialogService)
        {
            if (_client != null)
            {
                throw new InvalidOperationException();
            }

            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _client = lobbyService ?? throw new ArgumentNullException(nameof(lobbyService));
            _pictureService = pictureService ?? throw new ArgumentNullException(nameof(
                pictureService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(
                dialogService));
            if (!_client.IsActive)
            {
                throw new InvalidOperationException("No active lobby session.");
            }

            _client.LobbyChanged += OnLobbyChanged;
            _client.LobbyClosed += OnLobbyClosed;
            _client.MatchStarted += OnMatchStarted;
            ApplyLobby(_client.CurrentLobby);
            if (_client.WasClosed)
            {
                OnLobbyClosed();
            }
            else if (_client.ProvisionalMatchId > 0)
            {
                OnMatchStarted(_client.ProvisionalMatchId);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_client != null)
            {
                _client.LobbyChanged -= OnLobbyChanged;
                _client.LobbyClosed -= OnLobbyClosed;
                _client.MatchStarted -= OnMatchStarted;
            }
        }

        public void ConfigureLogger(ILogger<LobbyViewModel> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private void DoNothing()
        {
        }

        private bool CannotExecute()
        {
            return false;
        }

        private bool CanLeave()
        {
            return !_isBusy && !_disposed;
        }

        private bool CanStartGame()
        {
            return !_isBusy && !_disposed && IsHost && Players.Count >= MinimumPlayersToStart;
        }

        private void OnLobbyChanged(Lobby lobby)
        {
            Dispatch(() => ApplyLobbyUpdate(lobby));
        }

        private void OnLobbyClosed()
        {
            Dispatch(HandleLobbyClosedOnUi);
        }

        private void OnMatchStarted(int matchId)
        {
            Dispatch(() => HandleMatchStartedOnUi(matchId));
        }

        private void ApplyLobbyUpdate(Lobby lobby)
        {
            if (_disposed)
            {
                return;
            }

            ApplyLobby(_client.CurrentLobby ?? lobby);
        }

        private void HandleLobbyClosedOnUi()
        {
            if (_disposed)
            {
                return;
            }

            StatusMessage = string.Empty;
            _dialogService.ShowDialog(new DialogRequest { Type = DialogType.Information,
                TitleKey = "Global_msgDefaultErrorTitle", MessageKey = "Lobby_msgClosed" });
            _client.Reset();
            _navigation.GoBack();
        }

        private void HandleMatchStartedOnUi(int matchId)
        {
            if (_disposed)
            {
                return;
            }

            MatchNavigation context = new MatchNavigation
            {
                MatchId = matchId,
                PlayerSession = _playerSession,
                TargetPoints = _client.CurrentLobby.Settings.TargetPoints,
                PlayerNames = _client.CurrentLobby.Participants.ToDictionary(participant
                    => participant.PlayerId, participant => participant.Username)
            };
            Dispose();
            _client.Reset();
            _navigation.NavigateTo<MatchViewModel>(context);
        }

        private void ApplyLobby(Lobby lobby)
        {
            if (lobby == null)
            {
                return;
            }

            RoomCode = lobby.LobbyCode;
            Players.Clear();
            if (lobby.Participants != null)
            {
                foreach (Player player in lobby.Participants)
                {
                    Players.Add(new LobbyPlayerRecord(player, _pictureService.GetProfilePicture(
                        player.Picture?.ImageData), lobby.Host?.PlayerId, _playerSession.PlayerId));
                }
            }

            IsHost = lobby.Host != null && _playerSession.PlayerId == lobby.Host.PlayerId;
            RaiseCommands();
        }

        private void StartGame()
        {
            if (!StartGameCommand.CanExecute(null))
            {
                return;
            }

            SetBusy(true);
            BackgroundJob<object, bool> job = new BackgroundJob<object, bool>(
                StartMatchOnWorker, null, OnStartMatchCompleted, OnStartMatchFailed);
            BackgroundWork.Start(job);
        }

        private void Leave()
        {
            if (_isBusy || _disposed)
            {
                return;
            }

            SetBusy(true);
            BackgroundJob<object, bool> job = new BackgroundJob<object, bool>(
                LeaveLobbyOnWorker, null, OnLeaveLobbyCompleted, OnLeaveLobbyFailed);
            BackgroundWork.Start(job);
        }

        private bool StartMatchOnWorker(object state)
        {
            _client.StartMatch();
            return true;
        }

        private void OnStartMatchCompleted(bool result, object state)
        {
            SetBusy(false);
        }

        private void OnStartMatchFailed(Exception error, object state)
        {
            SetBusy(false);
            HandleOperationFailure(error);
        }

        private bool LeaveLobbyOnWorker(object state)
        {
            _client.LeaveLobby();
            return true;
        }

        private void OnLeaveLobbyCompleted(bool result, object state)
        {
            SetBusy(false);
            NavigateBackIfAvailable();
        }

        private void OnLeaveLobbyFailed(Exception error, object state)
        {
            SetBusy(false);
            _logger.LogError(error, "Lobby could not be left cleanly.");
            NavigateBackIfAvailable();
        }

        private void NavigateBackIfAvailable()
        {
            if (!_disposed)
            {
                _navigation.GoBack();
            }
        }

        private void HandleOperationFailure(Exception error)
        {
            _logger.LogError(error, "Lobby operation failed.");
            string key = error is FaultException<LobbyIssue>
                ? "Lobby_msgCannotStart" : "Lobby_msgOperationFailed";
            _dialogService.ShowDialog(new DialogRequest { Type = DialogType.Warning, TitleKey =
                "Global_msgDefaultErrorTitle", MessageKey = key });
        }

        private static void Dispatch(Action action)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(action);
            }
        }

        private void SetBusy(bool value)
        {
            _isBusy = value;
            RaiseCommands();
        }

        private void RaiseCommands()
        {
            GoBackCommand.RaiseCanExecuteChanged();
            StartGameCommand.RaiseCanExecuteChanged();
        }
    }
}
