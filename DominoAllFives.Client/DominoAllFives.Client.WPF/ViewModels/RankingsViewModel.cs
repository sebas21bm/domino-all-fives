using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Contracts.DTOs;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>
    /// Provides ranking information for the rankings interface.
    /// </summary>
    public class RankingsViewModel : ViewModelBase
    {
        private const string DefaultProfilePicture =
            "/Assets/Images/ProfilePictures/defaultProfilePic.png";

        private const string ApplicationFolderName =
            "DominoAllFives";

        private const string ProfilePicturesFolderName =
            "ProfilePictures";

        private readonly IFrameNavigationService _navigationService;

        private ObservableCollection<PlayerRankingRecord> _leaderboard;
        private string _currentPlayerRank;

        public ObservableCollection<PlayerRankingRecord> Leaderboard
        {
            get => _leaderboard;
            set => SetProperty(
                ref _leaderboard,
                value);
        }

        public string CurrentPlayerRank
        {
            get => _currentPlayerRank;
            set => SetProperty(
                ref _currentPlayerRank,
                value);
        }

        public RelayCommand GoBackCommand { get; }

        public RankingsViewModel(
            IFrameNavigationService navigationService,
            RankingResultDto rankingResult)
        {
            _navigationService = navigationService
                ?? throw new ArgumentNullException(
                    nameof(navigationService));

            _leaderboard =
                new ObservableCollection<PlayerRankingRecord>();

            _currentPlayerRank = string.Empty;

            GoBackCommand =
                new RelayCommand(
                    _ => _navigationService.GoBack());

            LoadRanking(rankingResult);
        }

        private void LoadRanking(
            RankingResultDto rankingResult)
        {
            if (rankingResult == null)
            {
                return;
            }

            Leaderboard.Clear();

            foreach (RankingEntryDto entry
                in rankingResult.TopPlayers)
            {
                Leaderboard.Add(
                    new PlayerRankingRecord
                    {
                        Rank = entry.Rank,
                        Username = entry.Username,
                        StatisticValue = entry.GamesWon,
                        AvatarPath =
                            GetProfilePicturePath(
                                entry.ProfilePicture)
                    });
            }

            CurrentPlayerRank =
                rankingResult.CurrentPlayerRank > 0
                    ? rankingResult.CurrentPlayerRank.ToString()
                    : "-";
        }

        private string GetProfilePicturePath(
            string profilePicture)
        {
            if (string.IsNullOrWhiteSpace(
                profilePicture))
            {
                return DefaultProfilePicture;
            }

            string localApplicationData =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData);

            string profilePicturePath =
                Path.Combine(
                    localApplicationData,
                    ApplicationFolderName,
                    ProfilePicturesFolderName,
                    profilePicture);

            if (!File.Exists(profilePicturePath))
            {
                return DefaultProfilePicture;
            }

            return profilePicturePath;
        }
    }
}