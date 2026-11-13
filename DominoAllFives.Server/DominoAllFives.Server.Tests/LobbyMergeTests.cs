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
    public sealed class LobbyMergeTests
    {
        [Test]
        public void StartLobby_WithInitializer_StartsMatchAndClosesLobbyChat()
        {
            // Arrange
            LobbyManager manager = new LobbyManager();
            LobbyController controller = new LobbyController(manager);
            Lobby lobby = CreateLobby(controller);
            manager.AddChatMessage(CreatePlayer(1), lobby.LobbyId, "Hello");
            Lobby initialized = null;
            // Act
            Lobby started = controller.StartLobby(1, snapshot => initialized = snapshot);
            // Assert
            Assert.That(initialized.LobbyId, Is.EqualTo(lobby.LobbyId));
            Assert.That(initialized.State, Is.EqualTo(LobbyState.Waiting));
            Assert.That(started.State, Is.EqualTo(LobbyState.InProgress));
            Assert.Throws<ChatRuleException>(() => manager.GetChatMessages(lobby.LobbyId, 1));
        }

        [Test]
        public void StartLobby_FailedInitialization_PreservesLobbyAndMessages()
        {
            // Arrange
            LobbyManager manager = new LobbyManager();
            LobbyController controller = new LobbyController(manager);
            Lobby lobby = CreateLobby(controller);
            manager.AddChatMessage(CreatePlayer(1), lobby.LobbyId, "Hello");
            // Act
            TestDelegate start = () => controller.StartLobby(1,
                snapshot => throw new InvalidOperationException("Test initialization failure."));
            // Assert
            Assert.Throws<InvalidOperationException>(start);
            Assert.That(controller.GetLobby(lobby.LobbyCode).State, Is.EqualTo(LobbyState.Waiting));
            Assert.That(manager.GetChatMessages(lobby.LobbyId, 1).Count, Is.EqualTo(1));
        }

        [Test]
        public void ReleaseMatch_FinishedMatch_AllowsParticipantsToCreateAnotherLobby()
        {
            // Arrange
            LobbyManager manager = new LobbyManager();
            LobbyController controller = new LobbyController(manager);
            Lobby lobby = CreateLobby(controller);
            controller.StartLobby(1);
            // Act
            manager.ReleaseMatch(lobby.LobbyId);
            // Assert
            Assert.DoesNotThrow(() => controller.CreateLobby(CreatePlayer(1), CreateSettings()));
            Assert.DoesNotThrow(() => controller.CreateLobby(CreatePlayer(2), CreateSettings()));
            Assert.DoesNotThrow(() => manager.ReleaseMatch(lobby.LobbyId));
        }

        private static Lobby CreateLobby(LobbyController controller)
        {
            Lobby lobby = controller.CreateLobby(CreatePlayer(1), CreateSettings());
            controller.JoinLobby(CreatePlayer(2), lobby.LobbyCode);
            return lobby;
        }

        private static LobbySettings CreateSettings()
        {
            return new LobbySettings { MaxPlayers = 2, TargetPoints = 100 };
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
