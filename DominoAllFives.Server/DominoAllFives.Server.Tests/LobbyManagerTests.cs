using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public class LobbyManagerTests
    {
        [Test]
        public void CreateLobby_ValidSettings_ReturnsHostSnapshot()
        {
            // Arrange
            // Act
            Lobby lobby = CreateController().CreateLobby(Player(1), Settings(4, 100));
            // Assert
            Assert.That(lobby.LobbyCode, Has.Length.EqualTo(6));
            Assert.That(lobby.Participants.Count, Is.EqualTo(1));
            Assert.That(lobby.Host.PlayerId, Is.EqualTo(1));
            Assert.That(lobby.State, Is.EqualTo(LobbyState.Waiting));
        }

        [Test]
        public void CreateLobby_InvalidLimit_ReportsSpecificIssue()
        {
            // Arrange
            // Act
            LobbyRuleException error =
                Assert.Throws<LobbyRuleException>(
                () => CreateController().CreateLobby(
                Player(
                1),
                Settings(
                5,
                100)));
            // Assert
            Assert.That(error.Issue, Is.EqualTo(LobbyIssue.InvalidPlayerLimit));
        }

        [Test]
        public void JoinLobby_WhenFull_ReportsLobbyFull()
        {
            // Arrange
            LobbyController controller = CreateController();
            Lobby lobby = controller.CreateLobby(Player(1), Settings(2, 100));
            controller.JoinLobby(Player(2), lobby.LobbyCode);
            // Act
            LobbyRuleException error =
                Assert.Throws<LobbyRuleException>(
                () => controller.JoinLobby(
                Player(
                3),
                lobby.LobbyCode));
            // Assert
            Assert.That(error.Issue, Is.EqualTo(LobbyIssue.Full));
        }

        [Test]
        public void StartLobby_WithOnePlayer_ReportsInsufficientPlayers()
        {
            // Arrange
            LobbyController controller = CreateController();
            controller.CreateLobby(Player(1), Settings(4, 150));
            // Act
            LobbyRuleException error =
                Assert.Throws<LobbyRuleException>(
                () => controller.StartLobby(
                1));
            // Assert
            Assert.That(error.Issue, Is.EqualTo(LobbyIssue.InsufficientPlayers));
        }

        [Test]
        public void StartLobby_ByNonHost_IsRejected()
        {
            // Arrange
            LobbyController controller = CreateController();
            Lobby lobby = controller.CreateLobby(Player(1), Settings(4, 200));
            controller.JoinLobby(Player(2), lobby.LobbyCode);
            // Act
            TestDelegate startLobby = () => controller.StartLobby(2);
            // Assert
            Assert.Throws<System.UnauthorizedAccessException>(startLobby);
        }

        [Test]
        public void HostLeave_ActiveLobby_ClosesLobbyAndReleasesParticipants()
        {
            // Arrange
            LobbyController controller = CreateController();
            Lobby lobby = controller.CreateLobby(Player(1), Settings(4, 250));
            controller.JoinLobby(Player(2), lobby.LobbyCode);
            // Act
            Lobby closed = controller.LeaveLobby(1);
            // Assert
            Assert.That(closed.State, Is.EqualTo(LobbyState.Closed));
            Assert.DoesNotThrow(() => controller.CreateLobby(Player(2), Settings(2, 100)));
        }

        private static LobbyController CreateController() =>
            new LobbyController(
            new LobbyManager());
        private static Player Player(int id) => new Player
        {
            PlayerId = id,
            Username = "Player" + id,
            Picture = ProfilePicture.FromStoredContent(new byte[0]),
            Presence = PlayerPresence.Online
        };
        private static LobbySettings Settings(int players, int points) => new LobbySettings
        {
            MaxPlayers = players,
            TargetPoints = points
        };
    }
}
