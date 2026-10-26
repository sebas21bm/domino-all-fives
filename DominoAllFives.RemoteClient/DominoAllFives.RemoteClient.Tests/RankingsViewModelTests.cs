using System;
using System.Collections.Generic;
using System.ServiceModel;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.ProfilePicture;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.ViewModels;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFivesService;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace DominoAllFives.RemoteClient.Tests
{
    [TestFixture]
    public class RankingsViewModelTests
    {
        [Test]
        public void Ranking_CombinesRemoteEntriesAndPlayerPosition()
        {
            List<RankingEntry> entries = new List<RankingEntry>
            {
                new RankingEntry { Rank = 1, Username = "leader", GamesWon = 20 }
            };

            Ranking ranking = new Ranking(entries, 7);

            Assert.That(ranking.TopPlayers, Is.SameAs(entries));
            Assert.That(ranking.CurrentPlayerRank, Is.EqualTo(7));
        }

        [TestCase(7, "7")]
        [TestCase(0, "-")]
        public void LoadRanking_ComposesPlayerPositionWithLeaderboard(int position, string displayedPosition)
        {
            List<RankingEntry> ranking = new List<RankingEntry>();
            RankingClient rankingClient = new RankingClient { TopRanking = ranking, Position = position };
            DialogService dialogs = new DialogService();

            RankingsViewModel viewModel = CreateViewModel(rankingClient, dialogs, true);

            Assert.That(viewModel.CurrentPlayerRank, Is.EqualTo(displayedPosition));
            Assert.That(rankingClient.Calls, Is.EqualTo(new[] { "top", "position:42" }));
            Assert.That(dialogs.Requests, Is.Empty);
        }

        [Test]
        public void LoadRanking_PositionUnavailable_DoesNotPublishPartialLeaderboard()
        {
            List<RankingEntry> ranking = new List<RankingEntry>
            {
                new RankingEntry { Rank = 1, Username = "leader", GamesWon = 20 }
            };
            RankingClient rankingClient = new RankingClient { TopRanking = ranking, PositionError = new CommunicationException() };
            DialogService dialogs = new DialogService();

            RankingsViewModel viewModel = CreateViewModel(rankingClient, dialogs, true);

            Assert.That(viewModel.Leaderboard, Is.Empty);
            Assert.That(viewModel.CurrentPlayerRank, Is.Empty);
            Assert.That(dialogs.Requests.Count, Is.EqualTo(1));
            Assert.That(dialogs.Requests[0].MessageKey, Is.EqualTo("Global_msgConnectionError"));
            Assert.That(rankingClient.Calls, Is.EqualTo(new[] { "top", "position:42" }));
        }

        [Test]
        public void LoadRanking_UnauthenticatedSession_DoesNotCallRankingService()
        {
            RankingClient rankingClient = new RankingClient();
            DialogService dialogs = new DialogService();

            RankingsViewModel viewModel = CreateViewModel(rankingClient, dialogs, false);

            Assert.That(rankingClient.Calls, Is.Empty);
            Assert.That(dialogs.Requests, Is.Empty);
            Assert.That(viewModel.Leaderboard, Is.Empty);
        }

        [Test]
        public void LoadRanking_MissingTopRanking_DoesNotRequestPosition()
        {
            RankingClient rankingClient = new RankingClient();
            DialogService dialogs = new DialogService();

            RankingsViewModel viewModel = CreateViewModel(rankingClient, dialogs, true);

            Assert.That(rankingClient.Calls, Is.EqualTo(new[] { "top" }));
            Assert.That(viewModel.Leaderboard, Is.Empty);
            Assert.That(dialogs.Requests, Is.Empty);
        }

        private static RankingsViewModel CreateViewModel(RankingClient client, DialogService dialogs, bool authenticated)
        {
            PlayerSession session = new PlayerSession();
            if (authenticated)
            {
                session.Start(42, "player");
            }

            return new RankingsViewModel(new NavigationService(), client, session,
                new ProfilePictureImageService(NullLogger<ProfilePictureImageService>.Instance), dialogs);
        }

        private sealed class RankingClient : IRankingClient
        {
            public List<RankingEntry> TopRanking { get; set; }
            public int Position { get; set; }
            public Exception PositionError { get; set; }
            public List<string> Calls { get; } = new List<string>();

            public List<RankingEntry> GetTopWinsRanking()
            {
                Calls.Add("top");
                return TopRanking;
            }

            public int GetWinsRankingPositionById(int playerId)
            {
                Calls.Add("position:" + playerId);
                if (PositionError != null)
                {
                    throw PositionError;
                }
                return Position;
            }
        }

        private sealed class DialogService : IDialogService
        {
            public List<DialogRequest> Requests { get; } = new List<DialogRequest>();
            public void ShowDialog(DialogRequest request) => Requests.Add(request);
        }

        private sealed class NavigationService : IFrameNavigationService
        {
            public bool CanGoBack => false;
            public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase { }
            public void NavigateTo<TViewModel>(object parameter) where TViewModel : ViewModelBase { }
            public void GoBackToRefresh<TViewModel>() where TViewModel : ViewModelBase { }
            public void NavigateAsRoot<TViewModel>() where TViewModel : ViewModelBase { }
            public void GoBack() { }
        }
    }
}

