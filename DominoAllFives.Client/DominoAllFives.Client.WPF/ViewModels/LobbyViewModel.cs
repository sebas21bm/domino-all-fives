using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Wcf;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class LobbyViewModel : ViewModelBase, IDisposable
    {
        private IFrameNavigationService _navigation;
        private LobbySession _session;
        private ILobbyClient _client;
        private bool _isBusy;
        private bool _disposed;
        private string _roomCode = string.Empty;
        private bool _isHost;
        private bool _isModalVisible;
        private ViewModelBase _currentModal;
        private string _statusMessage = string.Empty;

        public ViewModelBase CurrentModal
        {
            get => _currentModal;
            set => SetProperty(ref _currentModal, value);
        }
        public bool IsModalVisible
        {
            get => _isModalVisible;
            set => SetProperty(ref _isModalVisible, value);
        }
        public string RoomCode
        {
            get => _roomCode;
            private set => SetProperty(ref _roomCode, value);
        }
        public bool IsHost
        {
            get => _isHost;
            private set => SetProperty(ref _isHost, value);
        }
        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }
        public ObservableCollection<LobbyPlayerRecord> Players { get; }
            = new ObservableCollection<LobbyPlayerRecord>();
        public RelayCommand GoBackCommand { get; }
        public RelayCommand InviteFriendsCommand { get; }
        public RelayCommand StartGameCommand { get; }
        public RelayCommand ViewProfileCommand { get; }
        public RelayCommand KickPlayerCommand { get; }

        public LobbyViewModel()
        {
            GoBackCommand = new RelayCommand(Leave, () => !_isBusy);
            InviteFriendsCommand = new RelayCommand(() => { }, () => false);
            StartGameCommand = new RelayCommand(StartGame, () => !_disposed && !_isBusy && IsHost && Players.Count >= 2 && _session.CurrentLobby?.HasStarted == false);
            ViewProfileCommand = new RelayCommand(() => { }, () => false);
            KickPlayerCommand = new RelayCommand(() => { }, () => false);
        }

        public void Initialize(
            IFrameNavigationService navigation,
            LobbySession session)
        {
            if (_navigation != null)
            {
                throw new InvalidOperationException(
                    "LobbyViewModel has already been initialized.");
            }

            if (navigation == null)
                throw new ArgumentNullException(nameof(navigation));
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (!session.IsActive)
                throw new InvalidOperationException("No active lobby session.");

            _navigation = navigation;
            _session = session;
            _client = _session.Client;

            _client.LobbyChanged += OnLobbyChanged;
            _client.LobbyClosed += OnLobbyClosed;
            _client.MatchStarted += OnMatchStarted;
            ApplyLobby(_session.CurrentLobby);
            if (_session.WasClosed) OnLobbyClosed();
            else if (_session.ProvisionalMatchId > 0) OnMatchStarted(_session.ProvisionalMatchId);
        }

        private void OnLobbyChanged(Lobby lobby)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_disposed || !_session.IsActive) return;
                // LobbySession caches callbacks even before this page subscribes.
                ApplyLobby(_session.CurrentLobby);
            }));
        }

        private void OnLobbyClosed()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_disposed) return;
                StatusMessage = LanguageManager.Instance["Lobby_msgClosed"];
                UiOperationRunner.Run(() => { _session.Reset(); return true; },
                    result => { if (!_disposed) _navigation.GoBack(); },
                    error => { if (!_disposed) _navigation.GoBack(); });
            }));
        }

        private void OnMatchStarted(int matchId)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_disposed) return;
                StatusMessage = LanguageManager.Instance["Lobby_msgStarted"];
                StartGameCommand.RaiseCanExecuteChanged();
                // Do not navigate to a real match: initial server ID is provisional.
            }));
        }

        private void ApplyLobby(Lobby lobby)
        {
            if (lobby == null) return;
            RoomCode = lobby.LobbyCode;
            Players.Clear();
            if (lobby.Participants != null)
            {
                foreach (LobbyParticipant participant in lobby.Participants)
                {
                    Players.Add(new LobbyPlayerRecord
                    {
                        Username = participant.DisplayName,
                        IsHost = participant.IsHost,
                        IsCurrentPlayer = false, // No authenticated player identity in provisional contract.
                        AvatarPath = string.Empty
                    });
                }
            }
            // The initial contract does not expose the local WCF session ID to the client.
            // Until identity is integrated, do NOT infer IsHost from an arbitrary participant.
            IsHost = _session.WasCreatedByThisClient; // UI-only flag, server still authorizes.
            StartGameCommand.RaiseCanExecuteChanged();
        }

        private void StartGame()
        {
            if (_disposed || !StartGameCommand.CanExecute(null)) return;
            _isBusy = true;
            StartGameCommand.RaiseCanExecuteChanged();
            GoBackCommand.RaiseCanExecuteChanged();
            UiOperationRunner.Run(
                () =>
                {
                    _session.StartMatch();
                    return true;
                },
                result => { _isBusy = false; StartGameCommand.RaiseCanExecuteChanged(); if (!_disposed && _session.ProvisionalMatchId == 0) StatusMessage = LanguageManager.Instance["Lobby_msgStartRequested"]; },
                error => { _isBusy = false; StartGameCommand.RaiseCanExecuteChanged(); if (!_disposed) StatusMessage = UiOperationRunner.ErrorMessage(error); });
        }

        private void Leave()
        {
            if (_disposed || _isBusy) return;
            _isBusy = true;
            StartGameCommand.RaiseCanExecuteChanged();
            GoBackCommand.RaiseCanExecuteChanged();
            UiOperationRunner.Run(
                () =>
                {
                    _session.Leave();
                    return true;
                },
                result => { _isBusy = false; if (!_disposed) _navigation.GoBack(); },
                error => { _isBusy = false; if (!_disposed) { StatusMessage = UiOperationRunner.ErrorMessage(error); _navigation.GoBack(); } });
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_client == null) return;
            _client.LobbyChanged -= OnLobbyChanged;
            _client.LobbyClosed -= OnLobbyClosed;
            _client.MatchStarted -= OnMatchStarted;
            if (_session.IsActive) UiOperationRunner.Run(() => { _session.Leave(); return true; }, result => { }, error => { });
        }
    }
}
