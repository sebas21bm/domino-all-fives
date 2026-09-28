using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.Client.WPF.ViewModels;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using System;

namespace DominoAllFives.Client.WPF.Services
{
    /// <summary>
    /// Creates view models with their required dependencies.
    /// </summary>
    public class ViewModelFactory
    {
        private readonly IFrameNavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly PlayerSession _playerSession;

        public ViewModelFactory(
            IFrameNavigationService navigationService,
            IDialogService dialogService,
            PlayerSession playerSession)
        {
            _navigationService = navigationService
                ?? throw new ArgumentNullException(nameof(navigationService));

            _dialogService = dialogService
                ?? throw new ArgumentNullException(nameof(dialogService));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));
        }

        public TViewModel Create<TViewModel>()
            where TViewModel : ViewModelBase
        {
            if (typeof(TViewModel) == typeof(HomePageViewModel))
            {
                HomePageViewModel homePageViewModel =
                    new HomePageViewModel(
                        _navigationService,
                        _dialogService,
                        _playerSession);

                return (TViewModel)(ViewModelBase)homePageViewModel;
            }

            if (typeof(TViewModel) == typeof(MainMenuViewModel))
            {
                MainMenuViewModel mainMenuViewModel =
                    new MainMenuViewModel(
                        _navigationService,
                        _dialogService,
                        _playerSession);

                return (TViewModel)(ViewModelBase)mainMenuViewModel;
            }

            if (typeof(TViewModel) == typeof(GameSettingsViewModel))
            {
                GameSettingsViewModel gameSettingsViewModel =
                    new GameSettingsViewModel(
                        _navigationService,
                        _playerSession);

                return (TViewModel)(ViewModelBase)gameSettingsViewModel;
            }

            if (typeof(TViewModel) == typeof(RankingsViewModel))
            {
                RankingResultDto rankingResult = GetRankingResult();

                RankingsViewModel rankingsViewModel =
                    new RankingsViewModel(
                        _navigationService,
                        rankingResult);

                return (TViewModel)(ViewModelBase)rankingsViewModel;
            }

            return (TViewModel)Activator.CreateInstance(
                typeof(TViewModel),
                _navigationService);
        }

        private RankingResultDto GetRankingResult()
        {
            if (!_playerSession.PlayerId.HasValue)
            {
                return null;
            }

            using (DominoAllFivesEntities context =
                new DominoAllFivesEntities())
            {
                PlayerStatsRepository playerStatsRepository =
                    new PlayerStatsRepository(context);

                RankingController rankingController =
                    new RankingController(playerStatsRepository);

                return rankingController.GetRanking(
                    _playerSession.PlayerId.Value);
            }
        }
    }
}