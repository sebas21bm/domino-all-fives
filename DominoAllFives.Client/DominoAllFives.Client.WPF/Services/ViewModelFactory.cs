using System;

using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services.ProfilePicture;
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
        private readonly ProfilePictureImageService _profilePictureImageService;
        private readonly ProfilePictureFileService _profilePictureFileService;
        public ViewModelFactory(
            IFrameNavigationService navigationService,
            IDialogService dialogService,
            IAccountService accountService,
            IRankingService rankingService,
            PlayerSession playerSession,
            ILoggerFactory loggerFactory,
            ProfilePictureImageService profilePictureImageService,
            ProfilePictureFileService profilePictureFileService)
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

            _profilePictureImageService = profilePictureImageService
                ?? throw new ArgumentNullException(
                    nameof(profilePictureImageService));

            _profilePictureFileService = profilePictureFileService
                ?? throw new ArgumentNullException(
                    nameof(profilePictureFileService));
        }

        public TViewModel Create<TViewModel>()
            where TViewModel : ViewModelBase
        {
            ViewModelBase viewModel = null;

            switch(typeof(TViewModel).Name)
            {
                case nameof(HomePageViewModel):
                    viewModel = new HomePageViewModel(
                        _navigationService,
                        _dialogService,
                        _playerSession,
                        _accountService,
                        _loggerFactory);
                    break;
                case nameof(MainMenuViewModel):
                    viewModel = new MainMenuViewModel(
                        _navigationService,
                        _dialogService,
                        _playerSession);
                    break;
                case nameof(GameSettingsViewModel):
                    viewModel = new GameSettingsViewModel(
                        _navigationService,
                        _dialogService,
                        _accountService,
                        _loggerFactory,
                        _playerSession);
                    break;
                case nameof(ProfileViewModel):
                    viewModel = new ProfileViewModel(
                        _navigationService,
                        _accountService,
                        _dialogService,
                        _playerSession,
                        _profilePictureImageService);
                    break;
                case nameof(EditProfileViewModel):
                    viewModel = new EditProfileViewModel(
                        _navigationService,
                        _accountService,
                        _dialogService,
                        _playerSession,
                        _profilePictureImageService,
                        _profilePictureFileService);
                    break;
                case nameof(RankingsViewModel):
                    viewModel = new RankingsViewModel(
                        _navigationService,
                        _rankingService,
                        _playerSession,
                        _profilePictureImageService);
                    break;
                default:
                    viewModel = (TViewModel)Activator.CreateInstance(
                        typeof(TViewModel),
                        _navigationService);
                    break;
            }

            return viewModel as TViewModel;
        }
    }
}