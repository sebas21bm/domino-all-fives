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

namespace DominoAllFives.Client.Tests
{
    [TestFixture]
    public class RankingsViewModelTests
    {
        [Test]
        public void Ranking_RemoteEntriesAndPosition_CombinesBothValues()
        {
            // Arrange
            List<RankingEntry> entries = new List<RankingEntry>
            {
                new RankingEntry
                {
                    Rank = 1,
                    Username = "leader",
                    GamesWon = 20
                }
            };
            // Act
            Ranking ranking = new Ranking(entries, 7);
            // Assert
            Assert.That(ranking.TopPlayers, Is.SameAs(entries));
            Assert.That(ranking.CurrentPlayerRank, Is.EqualTo(7));
        }

        [TestCase(7, "7")]
        [TestCase(0, "-")]
        public void LoadRanking_AvailablePosition_ComposesPositionWithLeaderboard(
            int position,
            string displayedPosition)
        {
            // Arrange
            List<RankingEntry> ranking = new List<RankingEntry>();
            RankingClient rankingClient = new RankingClient
            {
                TopRanking = ranking,
                Position = position
            };
            DialogService dialogs = new DialogService();
            // Act
            RankingsViewModel viewModel = CreateViewModel(rankingClient, dialogs, true);
            // Assert
            Assert.That(viewModel.CurrentPlayerRank, Is.EqualTo(displayedPosition));
            Assert.That(rankingClient.Calls, Is.EqualTo(new[] { "top", "position:42" }));
            Assert.That(dialogs.Requests, Is.Empty);
        }

        [Test]
        public void LoadRanking_PositionUnavailable_DoesNotPublishPartialLeaderboard()
        {
            // Arrange
            List<RankingEntry> ranking = new List<RankingEntry>
            {
                new RankingEntry
                {
                    Rank = 1,
                    Username = "leader",
                    GamesWon = 20
                }
            };
            RankingClient rankingClient = new RankingClient
            {
                TopRanking = ranking,
                PositionError = new CommunicationException()
            };
            DialogService dialogs = new DialogService();
            // Act
            RankingsViewModel viewModel = CreateViewModel(rankingClient, dialogs, true);
            // Assert
            Assert.That(viewModel.Leaderboard, Is.Empty);
            Assert.That(viewModel.CurrentPlayerRank, Is.Empty);
            Assert.That(dialogs.Requests.Count, Is.EqualTo(1));
            Assert.That(dialogs.Requests[0].MessageKey, Is.EqualTo("Global_msgConnectionError"));
            Assert.That(rankingClient.Calls, Is.EqualTo(new[] { "top", "position:42" }));
        }

        [Test]
        public void LoadRanking_UnauthenticatedSession_DoesNotCallRankingService()
        {
            // Arrange
            RankingClient rankingClient = new RankingClient();
            DialogService dialogs = new DialogService();
            // Act
            RankingsViewModel viewModel = CreateViewModel(rankingClient, dialogs, false);
            // Assert
            Assert.That(rankingClient.Calls, Is.Empty);
            Assert.That(dialogs.Requests, Is.Empty);
            Assert.That(viewModel.Leaderboard, Is.Empty);
        }

        [Test]
        public void LoadRanking_MissingTopRanking_DoesNotRequestPosition()
        {
            // Arrange
            RankingClient rankingClient = new RankingClient();
            DialogService dialogs = new DialogService();
            // Act
            RankingsViewModel viewModel = CreateViewModel(rankingClient, dialogs, true);
            // Assert
            Assert.That(rankingClient.Calls, Is.EqualTo(new[] { "top" }));
            Assert.That(viewModel.Leaderboard, Is.Empty);
            Assert.That(dialogs.Requests, Is.Empty);
        }

        [Test]
        public void Initialize_InvalidOrRepeatedDependencies_RejectsBeforeLoading()
        {
            // Arrange
            var session = new PlayerSession();
            session.Start(42, "player");
            var client = new RankingClient
            {
                TopRanking = new List<RankingEntry>(),
                Position = 7
            };
            var dialogs = new DialogService();
            var pictures =
                new ProfilePictureImageService(
                NullLogger<ProfilePictureImageService>.Instance);
            var navigation = new NavigationService();
            var viewModel = new RankingsViewModel(session);
            // Assert
            Assert.That(client.Calls, Is.Empty);
            // Act
            Assert.Throws<ArgumentNullException>(
                () => viewModel.Initialize(
                navigation,
                client,
                pictures,
                null));
            // Assert
            Assert.That(client.Calls, Is.Empty);
            // Act
            viewModel.Initialize(navigation, client, pictures, dialogs);
            // Assert
            Assert.Throws<InvalidOperationException>(
                () => viewModel.Initialize(
                navigation,
                client,
                pictures,
                dialogs));
            Assert.That(client.Calls, Is.EqualTo(new[] { "top", "position:42" }));
            Assert.That(viewModel.CurrentPlayerRank, Is.EqualTo("7"));
        }

        [Test]
        public void Factory_AuthenticatedSession_ReturnsInitializedPagesAndModal()
        {
            // Arrange
            var navigation = new NavigationService();
            var session = new PlayerSession();
            session.Start(42, "player");
            var client = new RankingClient
            {
                TopRanking = new List<RankingEntry>(),
                Position = 7
            };
            using (var factory =
                new ViewModelFactory(
                navigation,
                new DialogService(),
                new LoginClient(),
                client,
                session,
                NullLoggerFactory.Instance,
                new ProfilePictureImageService(
                NullLogger<ProfilePictureImageService>.Instance)))
            {
                // Act
                var home = factory.Create<HomePageViewModel>();
                home.ShowLoginCommand.Execute(null);
                // Assert
                Assert.That(home.IsModalVisible, Is.True);
                Assert.That(home.CurrentModal, Is.TypeOf<LoginViewModel>());
                // Arrange
                var login = (LoginViewModel)home.CurrentModal;
                // Act
                login.LoginCommand.Execute(null);
                // Assert
                Assert.That(login.IsEmailRequiredVisible, Is.True);
                Assert.That(login.GoToRegisterCommand.CanExecute(null), Is.False);
                // Act
                login.CancelCommand.Execute(null);
                // Assert
                Assert.That(home.CurrentModal, Is.Null);
                Assert.That(home.IsModalVisible, Is.False);
                // Act
                MainMenuViewModel mainMenu = factory.Create<MainMenuViewModel>();
                // Assert
                Assert.That(mainMenu.Username, Is.EqualTo("player"));
                // Act
                RankingsViewModel rankings = factory.Create<RankingsViewModel>();
                // Assert
                Assert.That(rankings.CurrentPlayerRank, Is.EqualTo("7"));
                // Act
                factory.Create<CreateGameViewModel>().GoBackCommand.Execute(null);
                factory.Create<JoinGameViewModel>().GoBackCommand.Execute(null);
                factory.Create<EditProfileViewModel>().GoBackCommand.Execute(null);
                factory.Create<FriendsViewModel>().GoBackCommand.Execute(null);
                factory.Create<GameSettingsViewModel>().GoBackCommand.Execute(null);
                factory.Create<HowToPlayViewModel>().NavigateBackCommand.Execute(null);
                factory.Create<MatchHistoryViewModel>().GoBackCommand.Execute(null);
                factory.Create<ProfileViewModel>().GoBackCommand.Execute(null);
                // Assert
                Assert.That(navigation.BackCalls, Is.EqualTo(8));
                Assert.Throws<InvalidOperationException>(() => factory.Create<LobbyViewModel>());
            }
        }

        [Test]
        public void MainWindow_Initialize_SetsNavigationAndPreservesDialogService()
        {
            // Arrange
            var navigation = new NavigationService();
            var shell = new MainWindowViewModel();
            // Act
            shell.Initialize(navigation);
            // Assert
            Assert.That(shell.Navigation, Is.SameAs(navigation));
            Assert.That(shell.DialogService, Is.Not.Null);
            Assert.Throws<InvalidOperationException>(() => shell.Initialize(navigation));
        }

        private sealed class LoginClient : ILoginClient
        {
            public PlayerAccount LoginWithAccount(Credentials credentials) => null;
        }

        private static RankingsViewModel CreateViewModel(
            RankingClient client,
            DialogService dialogs,
            bool authenticated)
        {
            PlayerSession session = new PlayerSession();
            if (authenticated)
            {
                session.Start(42, "player");
            }

            var viewModel = new RankingsViewModel(session);
            viewModel.Initialize(
                new NavigationService(),
                client,
                new ProfilePictureImageService(
                NullLogger<ProfilePictureImageService>.Instance),
                dialogs);
            return viewModel;
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
            public int BackCalls { get; private set; }
            public bool CanGoBack => false;

            public void NavigateTo<TViewModel>()
                where TViewModel : ViewModelBase
            {
            }

            public void NavigateTo<TViewModel>(object parameter)
                where TViewModel : ViewModelBase
            {
            }

            public void GoBackToRefresh<TViewModel>()
                where TViewModel : ViewModelBase
            {
            }

            public void NavigateAsRoot<TViewModel>()
                where TViewModel : ViewModelBase
            {
            }

            public void GoBack()
            {
                BackCalls++;
            }
        }
    }
}
