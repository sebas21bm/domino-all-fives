using System;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services.ProfilePicture;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.Services.Wcf.Services;
using DominoAllFives.Client.WPF.ViewModels;
using DominoAllFives.Client.WPF.ViewModels.Base;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.Services
{
    /// <summary>
    /// Creates presentation models and injects implemented remote clients.
    /// </summary>
    public class ViewModelFactory : IDisposable
    {
        private readonly IFrameNavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ILoginClient _loginClient;
        private readonly IRankingClient _rankingClient;
        private readonly PlayerSession _playerSession;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ProfilePictureImageService _profilePictureImageService;
        private readonly ILobbyClient _lobbyService;
        public ViewModelFactory(
            IFrameNavigationService navigationService,
            IDialogService dialogService,
            ILoginClient loginClient,
            IRankingClient rankingClient,
            PlayerSession playerSession,
            ILoggerFactory loggerFactory,
            ProfilePictureImageService profilePictureImageService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(
                nameof(navigationService));
            _dialogService = dialogService ?? throw new ArgumentNullException(
                nameof(dialogService));
            _loginClient = loginClient ?? throw new ArgumentNullException(nameof(loginClient));
            _rankingClient = rankingClient ?? throw new ArgumentNullException(
                nameof(rankingClient));
            _playerSession = playerSession ?? throw new ArgumentNullException(
                nameof(playerSession));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(
                nameof(loggerFactory));
            _profilePictureImageService = profilePictureImageService
                ?? throw new ArgumentNullException(
                nameof(profilePictureImageService));
            _lobbyService = new LobbyClient(_playerSession);
        }

        public void Dispose()
        {
            _lobbyService.Dispose();
        }

        public LoginViewModel CreateLogin(
            Action onLoginSuccess,
            Action onCancel,
            Action onGoToRegister,
            Action onGoToRecover)
        {
            LoginViewModel viewModel =
                new LoginViewModel(
                _playerSession,
                onLoginSuccess,
                onCancel,
                onGoToRegister,
                onGoToRecover);
            viewModel.Initialize(
                _dialogService,
                _loginClient,
                LoggerFactoryProvider.CreateLogger<LoginViewModel>());
            return viewModel;
        }

        public RegisterAccountViewModel CreateRegisterAccount(Action onCancel)
        {
            return new RegisterAccountViewModel(onCancel);
        }

        public RecoverAccountViewModel CreateRecoverAccount(Action onCancel)
        {
            return new RecoverAccountViewModel(onCancel);
        }

        public TViewModel Create<TViewModel>()
            where TViewModel : ViewModelBase
        {
            ViewModelBase viewModel;
            switch (typeof(TViewModel).Name)
            {
                case nameof(HomePageViewModel):
                    HomePageViewModel homePage = new HomePageViewModel(_playerSession);
                    homePage.Initialize(_navigationService, this);
                    viewModel = homePage;
                    break;
                case nameof(MainMenuViewModel):
                    MainMenuViewModel mainMenu = new MainMenuViewModel(_playerSession);
                    mainMenu.Initialize(_navigationService);
                    viewModel = mainMenu;
                    break;
                case nameof(RankingsViewModel):
                    RankingsViewModel rankings = new RankingsViewModel(_playerSession);
                    rankings.Initialize(
                        _navigationService,
                        _rankingClient,
                        _profilePictureImageService,
                        _dialogService);
                    viewModel = rankings;
                    break;
                case nameof(CreateGameViewModel):
                    CreateGameViewModel createGame = new CreateGameViewModel(_playerSession);
                    createGame.Initialize(
                        _navigationService,
                        _lobbyService,
                        _dialogService,
                        LoggerFactoryProvider.CreateLogger<CreateGameViewModel>());
                    viewModel = createGame;
                    break;
                case nameof(JoinGameViewModel):
                    JoinGameViewModel joinGame = new JoinGameViewModel(_playerSession);
                    joinGame.Initialize(
                        _navigationService,
                        _lobbyService,
                        _dialogService,
                        LoggerFactoryProvider.CreateLogger<JoinGameViewModel>());
                    viewModel = joinGame;
                    break;
                case nameof(LobbyViewModel):
                    LobbyViewModel lobby = new LobbyViewModel(_playerSession);
                    lobby.ConfigureLogger(LoggerFactoryProvider.CreateLogger<LobbyViewModel>());
                    lobby.Initialize(
                        _navigationService,
                        _lobbyService,
                        _profilePictureImageService,
                        _dialogService);
                    viewModel = lobby;
                    break;
                default:
                    viewModel = CreateSecondaryViewModel<TViewModel>();
                    break;
            }

            return (TViewModel)viewModel;
        }

        private TViewModel CreateSecondaryViewModel<TViewModel>()
            where TViewModel : ViewModelBase
        {
            ViewModelBase viewModel;
            switch (typeof(TViewModel).Name)
            {
                case nameof(EditProfileViewModel):
                    EditProfileViewModel editProfile = new EditProfileViewModel();
                    editProfile.Initialize(_navigationService);
                    viewModel = editProfile;
                    break;
                case nameof(FriendsViewModel):
                    FriendsViewModel friends = new FriendsViewModel();
                    friends.Initialize(_navigationService);
                    viewModel = friends;
                    break;
                case nameof(GameSettingsViewModel):
                    GameSettingsViewModel gameSettings = new GameSettingsViewModel();
                    gameSettings.Initialize(_navigationService);
                    viewModel = gameSettings;
                    break;
                case nameof(HowToPlayViewModel):
                    HowToPlayViewModel howToPlay = new HowToPlayViewModel();
                    howToPlay.Initialize(_navigationService);
                    viewModel = howToPlay;
                    break;
                case nameof(MatchHistoryViewModel):
                    MatchHistoryViewModel matchHistory = new MatchHistoryViewModel();
                    matchHistory.Initialize(_navigationService);
                    viewModel = matchHistory;
                    break;
                case nameof(ProfileViewModel):
                    ProfileViewModel profile = new ProfileViewModel();
                    profile.Initialize(_navigationService);
                    viewModel = profile;
                    break;
                default:
                    throw new NotSupportedException(
                        $"No creation method registered for {typeof(TViewModel).FullName}.");
            }

            return (TViewModel)viewModel;
        }
    }
}
