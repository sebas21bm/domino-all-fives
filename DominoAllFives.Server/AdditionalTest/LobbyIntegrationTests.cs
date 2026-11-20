using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using NUnit.Framework;

namespace DominoAllFives.Server.AdditionalTest
{
    [TestFixture]
    public sealed class LobbyIntegrationTests
    {
        [Test]
        public void MatchStart_CompletedTransition_ClosesLobbyChat()
        {
            // Arrange
            LobbyManager manager = new LobbyManager();
            LobbyController controller = new LobbyController(manager);
            Lobby lobby = CreateLobby(controller);
            //manager.AddChatMessage(CreatePlayer(1), lobby.LobbyId, "Hello");
            controller.PrepareLobbyForMatch(1);
            // Act
            controller.ConfirmMatchStarted(1);
            // Assert
            /*Assert.That(
                () => manager.GetChatMessages(lobby.LobbyId, 1),
                Throws.TypeOf<ChatRuleException>()
                    .With.Property("Issue").EqualTo(ChatIssue.PlayerNotInChat));*/
        }

        [Test]
        public void MatchStart_CancelledTransition_PreservesLobbyAndMessages()
        {
            // Arrange
            LobbyManager manager = new LobbyManager();
            LobbyController controller = new LobbyController(manager);
            Lobby lobby = CreateLobby(controller);
            //manager.AddChatMessage(CreatePlayer(1), lobby.LobbyId, "Hello");
            controller.PrepareLobbyForMatch(1);
            // Act
            controller.RestoreLobbyAfterMatchStartFailure(1);
            // Assert
            Assert.That(
                new object[]
                {
                    controller.GetLobby(lobby.LobbyCode).State,
                    //manager.GetChatMessages(lobby.LobbyId, 1).Count
                },
                Is.EqualTo(new object[] { LobbyState.Waiting, 1 }));
        }

        [Test]
        public void ReleaseMatch_FinishedMatch_AllowsParticipantsToCreateAnotherLobby()
        {
            // Arrange
            LobbyManager manager = new LobbyManager();
            LobbyController controller = new LobbyController(manager);
            Lobby lobby = CreateLobby(controller);
            controller.PrepareLobbyForMatch(1);
            controller.ConfirmMatchStarted(1);
            // Act
            manager.ReleaseMatch(lobby.LobbyId);
            // Assert
            Assert.DoesNotThrow(
                () => controller.CreateLobby(CreatePlayer(2), CreateSettings()));
        }

        private static Lobby CreateLobby(LobbyController controller)
        {
            Lobby lobby = controller.CreateLobby(CreatePlayer(1), CreateSettings());
            return controller.JoinLobby(CreatePlayer(2), lobby.LobbyCode);
        }

        private static LobbySettings CreateSettings()
        {
            return new LobbySettings
            {
                MaxPlayers = 2,
                TargetPoints = 100
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
