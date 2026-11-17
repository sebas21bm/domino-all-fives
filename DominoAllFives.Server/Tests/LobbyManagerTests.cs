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
    public sealed class LobbyManagerTests
    {
        [Test]
        public void CreateLobby_ValidSettings_ReturnsHostSnapshot()
        {
            // Arrange
            LobbyController controller = CreateController();
            // Act
            Lobby lobby = controller.CreateLobby(CreatePlayer(1), CreateSettings(4, 100));
            // Assert
            Assert.That(
                new object[]
                {
                    lobby.LobbyCode.Length,
                    lobby.Participants.Count,
                    lobby.Host.PlayerId,
                    lobby.State
                },
                Is.EqualTo(new object[] { 6, 1, 1, LobbyState.Waiting }));
        }

        [Test]
        public void CreateLobby_PlayerAlreadyInLobby_ThrowsSpecificIssue()
        {
            // Arrange
            LobbyController controller = CreateController();
            controller.CreateLobby(CreatePlayer(1), CreateSettings(4, 100));
            // Act
            TestDelegate createLobby = () => controller.CreateLobby(
                CreatePlayer(1),
                CreateSettings(4, 100));
            // Assert
            Assert.That(
                createLobby,
                Throws.TypeOf<LobbyRuleException>()
                    .With.Property("Issue").EqualTo(LobbyIssue.PlayerAlreadyInLobby));
        }

        [Test]
        public void JoinLobby_AvailableLobby_AddsPlayer()
        {
            // Arrange
            LobbyController controller = CreateController();
            Lobby lobby = controller.CreateLobby(CreatePlayer(1), CreateSettings(4, 100));
            // Act
            Lobby joined = controller.JoinLobby(CreatePlayer(2), lobby.LobbyCode.ToLowerInvariant());
            // Assert
            Assert.That(joined.Participants.Count, Is.EqualTo(2));
        }

        [Test]
        public void JoinLobby_FullLobby_ThrowsSpecificIssue()
        {
            // Arrange
            LobbyController controller = CreateController();
            Lobby lobby = controller.CreateLobby(CreatePlayer(1), CreateSettings(2, 100));
            controller.JoinLobby(CreatePlayer(2), lobby.LobbyCode);
            // Act
            TestDelegate joinLobby = () => controller.JoinLobby(CreatePlayer(3), lobby.LobbyCode);
            // Assert
            Assert.That(
                joinLobby,
                Throws.TypeOf<LobbyRuleException>()
                    .With.Property("Issue").EqualTo(LobbyIssue.Full));
        }

        [Test]
        public void PrepareLobbyForMatch_InsufficientPlayers_ThrowsSpecificIssue()
        {
            // Arrange
            LobbyController controller = CreateController();
            controller.CreateLobby(CreatePlayer(1), CreateSettings(4, 150));
            // Act
            TestDelegate prepareLobby = () => controller.PrepareLobbyForMatch(1);
            // Assert
            Assert.That(
                prepareLobby,
                Throws.TypeOf<LobbyRuleException>()
                    .With.Property("Issue").EqualTo(LobbyIssue.InsufficientPlayers));
        }

        [Test]
        public void PrepareLobbyForMatch_NonHost_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            LobbyController controller = CreateController();
            Lobby lobby = CreateLobbyWithTwoPlayers(controller);
            // Act
            TestDelegate prepareLobby = () => controller.PrepareLobbyForMatch(
                lobby.Participants[1].PlayerId);
            // Assert
            Assert.Throws<UnauthorizedAccessException>(prepareLobby);
        }

        [Test]
        public void PrepareLobbyForMatch_ValidLobby_ChangesStateToStarting()
        {
            // Arrange
            LobbyController controller = CreateController();
            CreateLobbyWithTwoPlayers(controller);
            // Act
            Lobby lobby = controller.PrepareLobbyForMatch(1);
            // Assert
            Assert.That(lobby.State, Is.EqualTo(LobbyState.Starting));
        }

        [Test]
        public void ConfirmMatchStarted_StartingLobby_ChangesStateToInProgress()
        {
            // Arrange
            LobbyController controller = CreateController();
            CreateLobbyWithTwoPlayers(controller);
            controller.PrepareLobbyForMatch(1);
            // Act
            Lobby lobby = controller.ConfirmMatchStarted(1);
            // Assert
            Assert.That(lobby.State, Is.EqualTo(LobbyState.InProgress));
        }

        [Test]
        public void RestoreLobbyAfterMatchStartFailure_StartingLobby_RestoresWaitingState()
        {
            // Arrange
            LobbyController controller = CreateController();
            Lobby lobby = CreateLobbyWithTwoPlayers(controller);
            controller.PrepareLobbyForMatch(1);
            // Act
            controller.RestoreLobbyAfterMatchStartFailure(1);
            // Assert
            Assert.That(
                controller.GetLobby(lobby.LobbyCode).State,
                Is.EqualTo(LobbyState.Waiting));
        }

        [Test]
        public void LeaveLobby_HostLeaves_ClosesLobby()
        {
            // Arrange
            LobbyController controller = CreateController();
            CreateLobbyWithTwoPlayers(controller);
            // Act
            Lobby lobby = controller.LeaveLobby(1);
            // Assert
            Assert.That(lobby.State, Is.EqualTo(LobbyState.Closed));
        }

        [Test]
        public void GetLobby_ReturnedSnapshotChanges_PreservesStoredLobby()
        {
            // Arrange
            LobbyController controller = CreateController();
            Lobby lobby = controller.CreateLobby(CreatePlayer(1), CreateSettings(4, 100));
            Lobby snapshot = controller.GetLobby(lobby.LobbyCode);
            snapshot.Participants.Clear();
            // Act
            Lobby storedLobby = controller.GetLobby(lobby.LobbyCode);
            // Assert
            Assert.That(storedLobby.Participants.Count, Is.EqualTo(1));
        }

        private static Lobby CreateLobbyWithTwoPlayers(LobbyController controller)
        {
            Lobby lobby = controller.CreateLobby(CreatePlayer(1), CreateSettings(4, 100));
            return controller.JoinLobby(CreatePlayer(2), lobby.LobbyCode);
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
