using System;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class LobbyControllerTests
    {
        [Test]
        public void Constructor_NullManager_ThrowsArgumentNullException()
        {
            // Arrange
            LobbyManager lobbyManager = null;
            // Act
            TestDelegate createController = () => new LobbyController(lobbyManager);
            // Assert
            Assert.Throws<ArgumentNullException>(createController);
        }

        [TestCase(1)]
        [TestCase(5)]
        public void CreateLobby_InvalidPlayerLimit_ThrowsSpecificIssue(int maximumPlayers)
        {
            // Arrange
            LobbyController controller = CreateController();
            LobbySettings settings = CreateSettings(maximumPlayers, 100);
            // Act
            TestDelegate createLobby = () => controller.CreateLobby(CreatePlayer(1), settings);
            // Assert
            Assert.That(
                createLobby,
                Throws.TypeOf<LobbyRuleException>()
                    .With.Property("Issue").EqualTo(LobbyIssue.InvalidPlayerLimit));
        }

        [Test]
        public void CreateLobby_InvalidTargetPoints_ThrowsSpecificIssue()
        {
            // Arrange
            LobbyController controller = CreateController();
            LobbySettings settings = CreateSettings(4, 300);
            // Act
            TestDelegate createLobby = () => controller.CreateLobby(CreatePlayer(1), settings);
            // Assert
            Assert.That(
                createLobby,
                Throws.TypeOf<LobbyRuleException>()
                    .With.Property("Issue").EqualTo(LobbyIssue.InvalidTargetPoints));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void JoinLobby_InvalidCode_ThrowsArgumentException(string lobbyCode)
        {
            // Arrange
            LobbyController controller = CreateController();
            // Act
            TestDelegate joinLobby = () => controller.JoinLobby(CreatePlayer(1), lobbyCode);
            // Assert
            Assert.That(joinLobby, Throws.InstanceOf<ArgumentException>());
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void LeaveLobby_InvalidPlayerId_ThrowsArgumentOutOfRangeException(int playerId)
        {
            // Arrange
            LobbyController controller = CreateController();
            // Act
            TestDelegate leaveLobby = () => controller.LeaveLobby(playerId);
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(leaveLobby);
        }

        [Test]
        public void CreateLobby_BlankUsername_ThrowsArgumentException()
        {
            // Arrange
            LobbyController controller = CreateController();
            Player player = CreatePlayer(1);
            player.Username = "   ";
            // Act
            TestDelegate createLobby = () => controller.CreateLobby(
                player,
                CreateSettings(4, 100));
            // Assert
            Assert.Throws<ArgumentException>(createLobby);
        }

        private static LobbyController CreateController()
        {
            return new LobbyController(new LobbyManager());
        }

        private static LobbySettings CreateSettings(int maximumPlayers, int targetPoints)
        {
            return new LobbySettings
            {
                MaxPlayers = maximumPlayers,
                TargetPoints = targetPoints
            };
        }

        private static Player CreatePlayer(int playerId)
        {
            return new Player
            {
                PlayerId = playerId,
                Username = "Player" + playerId,
                Picture = ProfilePicture.FromStoredContent(new byte[0]),
                Presence = PlayerPresence.Online
            };
        }
    }
}
