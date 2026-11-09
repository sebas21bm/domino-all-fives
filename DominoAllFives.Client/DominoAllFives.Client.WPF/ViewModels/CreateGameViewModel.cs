using System;
using System.ServiceModel;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Wcf;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class CreateGameViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigation;
        private readonly LobbySession _lobbySession;
        private bool _isBusy;
        private readonly DominoAllFives.Client.WPF.Models.PlayerSession _playerSession;

        public bool Is100PointsSelected { get; set; }
        public bool Is150PointsSelected { get; set; }
        public bool Is200PointsSelected { get; set; }
        public bool Is250PointsSelected { get; set; }
        public bool Is2PlayersSelected { get; set; }
        public bool Is3PlayersSelected { get; set; }
        public bool Is4PlayersSelected { get; set; }
        public string ErrorMessage { get; private set; }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public RelayCommand GoBackCommand { get; }
        public RelayCommand CreateGameCommand { get; }

        public CreateGameViewModel(IFrameNavigationService navigation, LobbySession lobbySession, DominoAllFives.Client.WPF.Models.PlayerSession playerSession = null)
        {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _lobbySession = lobbySession ?? throw new ArgumentNullException(nameof(lobbySession));
            _playerSession = playerSession;
            Is2PlayersSelected = true;
            Is100PointsSelected = true;
            GoBackCommand = new RelayCommand(() => _navigation.GoBack(), () => !_isBusy);
            CreateGameCommand = new RelayCommand(CreateGame, () => !_isBusy);
        }

        private void CreateGame()
        {
            if (_isBusy) return;
            int maxPlayers = Is4PlayersSelected ? 4 : Is3PlayersSelected ? 3 : 2;

            LobbySettings settings = new LobbySettings
            {
                HostDisplayName = _playerSession != null && _playerSession.IsAuthenticated ? _playerSession.Username : "Host", // Provisional: replace with verified session display name if available.
                MaximumPlayers = maxPlayers
            };

            SetError(null);
            SetBusy(true);
            UiOperationRunner.Run(
                () => _lobbySession.Create(settings),
                lobby =>
                {
                    SetBusy(false);
                    _navigation.NavigateTo<LobbyViewModel>();
                },
                error =>
                {
                    SetBusy(false);
                    SetError(UiOperationRunner.ErrorMessage(error));
                });
        }

        private void SetBusy(bool busy)
        {
            _isBusy = busy;
            CreateGameCommand.RaiseCanExecuteChanged();
            GoBackCommand.RaiseCanExecuteChanged();
        }

        private void SetError(string message)
        {
            ErrorMessage = message;
            OnPropertyChanged(nameof(ErrorMessage));
            OnPropertyChanged(nameof(HasError));
        }
    }
}
