using System;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services.ProfilePicture;
using DominoAllFives.Client.WPF.Services.Wcf;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
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
        private readonly LobbySession _lobbySession;

        public ViewModelFactory(
            IFrameNavigationService navigationService,
            IDialogService dialogService,
            ILoginClient loginClient,
            IRankingClient rankingClient,
            PlayerSession playerSession,
            ILoggerFactory loggerFactory,
            ProfilePictureImageService profilePictureImageService)
        {
            _navigationService = navigationService
                ?? throw new ArgumentNullException(nameof(navigationService));

            _dialogService = dialogService
                ?? throw new ArgumentNullException(nameof(dialogService));

            _loginClient = loginClient
                ?? throw new ArgumentNullException(nameof(loginClient));

            _rankingClient = rankingClient
                ?? throw new ArgumentNullException(nameof(rankingClient));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));

            _loggerFactory = loggerFactory
                ?? throw new ArgumentNullException(nameof(loggerFactory));

            _profilePictureImageService = profilePictureImageService
                ?? throw new ArgumentNullException(nameof(profilePictureImageService));

            _lobbySession = new LobbySession();
        }

        public void Dispose()
        {
            _lobbySession.Dispose();
        }

        public LoginViewModel CreateLogin(
            Action onLoginSuccess,
            Action onCancel,
            Action onGoToRegister,
            Action onGoToRecover)
        {
            var viewModel = new LoginViewModel(
                _playerSession,
                onLoginSuccess,
                onCancel,
                onGoToRegister,
                onGoToRecover);

            viewModel.Initialize(
                _dialogService,
                _loginClient,
                _loggerFactory.CreateLogger<LoginViewModel>());

            return viewModel;
        }

        public RegisterAccountViewModel CreateRegisterAccount(
            Action onCancel)
        {
            return new RegisterAccountViewModel(onCancel);
        }

        public RecoverAccountViewModel CreateRecoverAccount(
            Action onCancel)
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
                    viewModel = new HomePageViewModel(
                        _navigationService,
                        _playerSession,
                        this);
                    break;

                case nameof(MainMenuViewModel):
                    viewModel = new MainMenuViewModel(
                        _navigationService,
                        _dialogService,
                        _playerSession);
                    break;

                case nameof(RankingsViewModel):
                    viewModel = new RankingsViewModel(
                        _navigationService,
                        _rankingClient,
                        _playerSession,
                        _profilePictureImageService,
                        _dialogService);
                    break;

                case nameof(CreateGameViewModel):
                    viewModel = new CreateGameViewModel(
                        _navigationService,
                        _lobbySession,
                        _playerSession);
                    break;

                case nameof(JoinGameViewModel):
                    viewModel = new JoinGameViewModel(
                        _navigationService,
                        _lobbySession);
                    break;

                case nameof(LobbyViewModel):
                    viewModel = new LobbyViewModel(
                        _navigationService,
                        _lobbySession);
                    break;

                default:
                    viewModel = (TViewModel)Activator.CreateInstance(
                        typeof(TViewModel),
                        _navigationService);
                    break;
            }

            return (TViewModel)viewModel;
        }
    }
}

