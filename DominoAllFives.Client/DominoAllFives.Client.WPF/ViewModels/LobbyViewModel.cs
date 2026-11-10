using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Windows;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.ProfilePicture;
using DominoAllFives.Client.WPF.Services.Threading;
using DominoAllFives.Client.WPF.Services.Wcf;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.ViewModels.Base;
using LobbyService;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public sealed class LobbyViewModel : ViewModelBase, IDisposable
    {
        private readonly PlayerSession _playerSession;
        private IFrameNavigationService _navigation;
        private LobbySession _session;
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

        public ViewModelBase CurrentModal { get => _currentModal;
            set => SetProperty(ref _currentModal, value); }
        public bool IsModalVisible { get => _isModalVisible;
            set => SetProperty(ref _isModalVisible, value); }
        public string RoomCode { get => _roomCode;
            private set => SetProperty(ref _roomCode, value); }
        public bool IsHost { get => _isHost;
            private set => SetProperty(ref _isHost, value); }
        public string StatusMessage { get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value); }
        public ObservableCollection<LobbyPlayerRecord> Players { get; } =
            new ObservableCollection<LobbyPlayerRecord>();
        public RelayCommand GoBackCommand { get; }
        public RelayCommand InviteFriendsCommand { get; }
        public RelayCommand StartGameCommand { get; }
        public RelayCommand ViewProfileCommand { get; }
        public RelayCommand KickPlayerCommand { get; }

        public LobbyViewModel(PlayerSession playerSession)
        {
            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));
            GoBackCommand = new RelayCommand(Leave, () => !_isBusy && !_disposed);
            InviteFriendsCommand = new RelayCommand(() => { }, () => false);
            StartGameCommand = new RelayCommand(StartGame,
                () => !_isBusy && !_disposed && IsHost && Players.Count >= 2);
            ViewProfileCommand = new RelayCommand(() => { }, () => false);
            KickPlayerCommand = new RelayCommand(() => { }, () => false);
        }

        public void Initialize(IFrameNavigationService navigation, LobbySession session,
            ProfilePictureImageService pictureService, IDialogService dialogService,
            ILogger<LobbyViewModel> logger)
        {
            if (_session != null) throw new InvalidOperationException();
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _pictureService = pictureService
                ?? throw new ArgumentNullException(nameof(pictureService));
            _dialogService = dialogService
                ?? throw new ArgumentNullException(nameof(dialogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (!session.IsActive) throw new InvalidOperationException("No active lobby session.");
            _client = session.Client;
            _client.LobbyChanged += OnLobbyChanged;
            _client.LobbyClosed += OnLobbyClosed;
            _client.MatchStarted += OnMatchStarted;
            ApplyLobby(session.CurrentLobby);
            if (session.WasClosed) OnLobbyClosed();
            else if (session.ProvisionalMatchId > 0) OnMatchStarted(session.ProvisionalMatchId);
        }

        private void OnLobbyChanged(Lobby lobby)
        {
            Dispatch(() => { if (!_disposed) ApplyLobby(_session.CurrentLobby ?? lobby); });
        }

        private void OnLobbyClosed()
        {
            Dispatch(() =>
            {
                if (_disposed) return;
                StatusMessage = "";
                _dialogService.ShowDialog(new DialogRequest
                {
                    Type = DialogType.Information,
                    TitleKey = "Global_msgDefaultErrorTitle",
                    MessageKey = "Lobby_msgClosed"
                });
                _session.Reset();
                _navigation.GoBack();
            });
        }

        private void OnMatchStarted(int matchId)
        {
            Dispatch(() =>
            {
                if (_disposed) return;
                StatusMessage = string.Empty;
                _dialogService.ShowDialog(new DialogRequest
                {
                    Type = DialogType.Information,
                    TitleKey = "Global_msgDefaultErrorTitle",
                    MessageKey = "Lobby_msgStarted"
                });
                RaiseCommands();
            });
        }

        private void ApplyLobby(Lobby lobby)
        {
            if (lobby == null) return;
            RoomCode = lobby.LobbyCode;
            Players.Clear();
            if (lobby.Participants != null)
            {
                foreach (Player player in lobby.Participants)
                {
                    Players.Add(new LobbyPlayerRecord(player.PlayerId, player.Username,
                        _pictureService.GetProfilePicture(player.Picture?.ImageData),
                        lobby.Host != null && player.PlayerId == lobby.Host.PlayerId,
                        _playerSession.PlayerId == player.PlayerId));
                }
            }
            IsHost = lobby.Host != null && _playerSession.PlayerId == lobby.Host.PlayerId;
            RaiseCommands();
        }

        private void StartGame()
        {
            if (!StartGameCommand.CanExecute(null)) return;
            SetBusy(true);
            BackgroundWork.Start(new BackgroundJob<object, bool>(
                state => { _session.StartMatch(); return true; }, null,
                (result, state) => SetBusy(false),
                (error, state) => { SetBusy(false); HandleOperationFailure(error); }));
        }

        private void Leave()
        {
            if (_isBusy || _disposed) return;
            SetBusy(true);
            BackgroundWork.Start(new BackgroundJob<object, bool>(
                state => { _session.Leave(); return true; }, null,
                (result, state) => { SetBusy(false); if (!_disposed) _navigation.GoBack(); },
                (error, state) =>
                {
                    SetBusy(false);
                    _logger.LogError(error, "Lobby could not be left cleanly.");
                    if (!_disposed) _navigation.GoBack();
                }));
        }

        private void HandleOperationFailure(Exception error)
        {
            _logger.LogError(error, "Lobby operation failed.");
            string key = error is FaultException<LobbyIssue> ? "Lobby_msgCannotStart" :
                "Lobby_msgOperationFailed";
            _dialogService.ShowDialog(new DialogRequest
            {
                Type = DialogType.Warning,
                TitleKey = "Global_msgDefaultErrorTitle",
                MessageKey = key
            });
        }

        private static void Dispatch(Action action)
        {
            if (Application.Current.Dispatcher.CheckAccess()) action();
            else Application.Current.Dispatcher.BeginInvoke(action);
        }

        private void SetBusy(bool value) { _isBusy = value; RaiseCommands(); }
        private void RaiseCommands()
        {
            GoBackCommand.RaiseCanExecuteChanged();
            StartGameCommand.RaiseCanExecuteChanged();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_client != null)
            {
                _client.LobbyChanged -= OnLobbyChanged;
                _client.LobbyClosed -= OnLobbyClosed;
                _client.MatchStarted -= OnMatchStarted;
            }
        }
    }
}
