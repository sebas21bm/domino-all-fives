using System;
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
    public sealed class CreateGameViewModel : ViewModelBase
    {
        private readonly PlayerSession _playerSession;
        private IFrameNavigationService _navigation;
        private ILobbyClient _lobbyService;
        private IDialogService _dialogService;
        private ILogger<CreateGameViewModel> _logger;
        private bool _isBusy;
        private bool _is100PointsSelected = true;
        private bool _is150PointsSelected;
        private bool _is200PointsSelected;
        private bool _is250PointsSelected;
        private bool _is2PlayersSelected = true;
        private bool _is3PlayersSelected;
        private bool _is4PlayersSelected;
        public bool Is100PointsSelected
        {
            get
            {
                return _is100PointsSelected;
            }

            set
            {
                SetProperty(ref _is100PointsSelected, value);
            }
        }

        public bool Is150PointsSelected
        {
            get
            {
                return _is150PointsSelected;
            }

            set
            {
                SetProperty(ref _is150PointsSelected, value);
            }
        }

        public bool Is200PointsSelected
        {
            get
            {
                return _is200PointsSelected;
            }

            set
            {
                SetProperty(ref _is200PointsSelected, value);
            }
        }

        public bool Is250PointsSelected
        {
            get
            {
                return _is250PointsSelected;
            }

            set
            {
                SetProperty(ref _is250PointsSelected, value);
            }
        }

        public bool Is2PlayersSelected
        {
            get
            {
                return _is2PlayersSelected;
            }

            set
            {
                SetProperty(ref _is2PlayersSelected, value);
            }
        }

        public bool Is3PlayersSelected
        {
            get
            {
                return _is3PlayersSelected;
            }

            set
            {
                SetProperty(ref _is3PlayersSelected, value);
            }
        }

        public bool Is4PlayersSelected
        {
            get
            {
                return _is4PlayersSelected;
            }

            set
            {
                SetProperty(ref _is4PlayersSelected, value);
            }
        }

        public RelayCommand GoBackCommand { get; }
        public RelayCommand CreateGameCommand { get; }

        public CreateGameViewModel(PlayerSession playerSession)
        {
            _playerSession = playerSession ?? throw new ArgumentNullException(
                nameof(
                playerSession));
            GoBackCommand = new RelayCommand(GoBack, CanInteract);
            CreateGameCommand = new RelayCommand(CreateGame, CanCreateGame);
        }

        public void Initialize(
            IFrameNavigationService navigation,
            ILobbyClient lobbyService,
            IDialogService dialogService,
            ILogger<CreateGameViewModel> logger)
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

        private bool CanInteract()
        {
            return _lobbyService != null && !_isBusy;
        }

        private bool CanCreateGame()
        {
            return CanInteract() && _playerSession.HasServerSession;
        }

        private void CreateGame()
        {
            if (!CanCreateGame())
            {
                return;
            }

            LobbySettings settings = new LobbySettings
            {
                MaxPlayers = GetMaximumPlayers(),
                TargetPoints = GetTargetPoints()
            };
            SetBusy(true);
            BackgroundJob<LobbySettings, Lobby> job =
                new BackgroundJob<LobbySettings, Lobby>(
                value => _lobbyService.CreateLobby(
                value),
                settings,
                OnLobbyReady,
                OnLobbyFailed);
            BackgroundWork.Start(job);
        }

        private void HandleFailure(Exception error)
        {
            FaultException<LobbyIssue> lobbyFault = error as FaultException<LobbyIssue>;
            if (lobbyFault != null)
            {
                _logger.LogWarning(
                    lobbyFault,
                    "Create lobby rejected: {Issue}.",
                    lobbyFault.Detail);
                string key =
                    lobbyFault.Detail == LobbyIssue.PlayerAlreadyInLobby
                    ? "Lobby_msgAlreadyJoined" : "Lobby_msgInvalidSettings";
                Show(DialogType.Warning, key);
                return;
            }

            _logger.LogError(error, "Lobby could not be created.");
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
            CreateGameCommand.RaiseCanExecuteChanged();
        }

        private void OnLobbyReady(Lobby lobby, LobbySettings state)
        {
            SetBusy(false);
            _navigation.NavigateTo<LobbyViewModel>();
        }

        private void OnLobbyFailed(Exception error, LobbySettings state)
        {
            SetBusy(false);
            HandleFailure(error);
        }

        private int GetMaximumPlayers()
        {
            if (Is4PlayersSelected)
            {
                return 4;
            }

            return Is3PlayersSelected ? 3 : 2;
        }

        private int GetTargetPoints()
        {
            if (Is250PointsSelected)
            {
                return 250;
            }

            if (Is200PointsSelected)
            {
                return 200;
            }

            return Is150PointsSelected ? 150 : 100;
        }
    }
}
