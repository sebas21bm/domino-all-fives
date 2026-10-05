using System;

using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.ViewModels;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.Services
{
    /// <summary>
    /// Creates view models with their required dependencies.
    /// </summary>
    public class ViewModelFactory
    {
        private readonly IFrameNavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IAccountService _accountService;
        private readonly IRankingService _rankingService;
        private readonly PlayerSession _playerSession;
        private readonly ILoggerFactory _loggerFactory;

        public ViewModelFactory(
            IFrameNavigationService navigationService,
            IDialogService dialogService,
            IAccountService accountService,
            IRankingService rankingService,
            PlayerSession playerSession,
            ILoggerFactory loggerFactory)
        {
            _navigationService = navigationService
                ?? throw new ArgumentNullException(
                    nameof(navigationService));

            _dialogService = dialogService
                ?? throw new ArgumentNullException(
                    nameof(dialogService));

            _accountService = accountService
                ?? throw new ArgumentNullException(
                    nameof(accountService));

            _rankingService = rankingService
                ?? throw new ArgumentNullException(
                    nameof(rankingService));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(
                    nameof(playerSession));

            _loggerFactory = loggerFactory
                ?? throw new ArgumentNullException(
                    nameof(loggerFactory));
        }

        public TViewModel Create<TViewModel>()
            where TViewModel : ViewModelBase
        {
            if (typeof(TViewModel) ==
                typeof(HomePageViewModel))
            {
                HomePageViewModel homePageViewModel =
                    new HomePageViewModel(
                        _navigationService,
                        _dialogService,
                        _playerSession,
                        _accountService,
                        _loggerFactory);

                return (TViewModel)(ViewModelBase)
                    homePageViewModel;
            }

            if (typeof(TViewModel) ==
                typeof(MainMenuViewModel))
            {
                MainMenuViewModel mainMenuViewModel =
                    new MainMenuViewModel(
                        _navigationService,
                        _dialogService,
                        _playerSession);

                return (TViewModel)(ViewModelBase)
                    mainMenuViewModel;
            }

            if (typeof(TViewModel) ==
                typeof(GameSettingsViewModel))
            {
                GameSettingsViewModel gameSettingsViewModel =
                    new GameSettingsViewModel(
                        _navigationService,
                        _playerSession);

                return (TViewModel)(ViewModelBase)
                    gameSettingsViewModel;
            }

            if (typeof(TViewModel) ==
                typeof(ProfileViewModel))
            {
                ProfileViewModel profileViewModel =
                    new ProfileViewModel(
                        _navigationService,
                        _accountService,
                        _dialogService,
                        _playerSession);

                return (TViewModel)(ViewModelBase)
                    profileViewModel;
            }

            if (typeof(TViewModel) ==
                typeof(RankingsViewModel))
            {
                RankingsViewModel rankingsViewModel =
                    new RankingsViewModel(
                        _navigationService,
                        _rankingService,
                        _playerSession);

                return (TViewModel)(ViewModelBase)
                    rankingsViewModel;
            }

            return (TViewModel)Activator.CreateInstance(
                typeof(TViewModel),
                _navigationService);
        }
    }
}