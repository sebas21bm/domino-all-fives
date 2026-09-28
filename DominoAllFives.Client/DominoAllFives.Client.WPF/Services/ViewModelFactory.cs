using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.Client.WPF.ViewModels;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Contracts.DTOs;
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
                ?? throw new ArgumentNullException(
                    nameof(navigationService));

            _dialogService = dialogService
                ?? throw new ArgumentNullException(
                    nameof(dialogService));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(
                    nameof(playerSession));
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
                        _playerSession);

                return (TViewModel)(ViewModelBase)
                    homePageViewModel;
            }

            if (typeof(TViewModel) ==
                typeof(RankingsViewModel))
            {
                RankingController rankingController =
                    new RankingController();

                RankingResultDto rankingResult = null;

                if (_playerSession.PlayerId.HasValue)
                {
                    rankingResult =
                        rankingController.GetRanking(
                            _playerSession.PlayerId.Value);
                }

                RankingsViewModel rankingsViewModel =
                    new RankingsViewModel(
                        _navigationService,
                        rankingResult);

                return (TViewModel)(ViewModelBase)
                    rankingsViewModel;
            }

            return (TViewModel)Activator.CreateInstance(
                typeof(TViewModel),
                _navigationService);
        }
    }
}