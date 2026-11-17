using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Services;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class LobbyContractTests
    {
        [TestCase(LobbyIssue.NotFound, "LobbyNotFound")]
        [TestCase(LobbyIssue.Unavailable, "LobbyUnavailable")]
        [TestCase(LobbyIssue.Full, "LobbyFull")]
        public void Serialize_RenamedLobbyIssue_PreservesPublishedValue(
            LobbyIssue issue,
            string expectedWireValue)
        {
            // Arrange
            DataContractSerializer serializer = new DataContractSerializer(typeof(LobbyIssue));
            using (MemoryStream stream = new MemoryStream())
            {
                // Act
                serializer.WriteObject(stream, issue);
                stream.Position = 0;
                XmlDocument document = new XmlDocument();
                document.Load(stream);
                stream.Position = 0;
                LobbyIssue restored = (LobbyIssue)serializer.ReadObject(stream);
                // Assert
                Assert.That(
                    new object[] { document.DocumentElement.InnerText, restored },
                    Is.EqualTo(new object[] { expectedWireValue, issue }));
            }
        }

        [Test]
        public void ServiceContract_Lobby_PreservesDuplexSessionConfiguration()
        {
            // Arrange
            ServiceContractAttribute contract = typeof(ILobbyService)
                .GetCustomAttribute<ServiceContractAttribute>();
            // Act
            object[] configuration =
            {
                contract.CallbackContract,
                contract.SessionMode
            };
            // Assert
            Assert.That(
                configuration,
                Is.EqualTo(new object[] { typeof(ILobbyCallback), SessionMode.Required }));
        }

        [Test]
        public void ServiceContract_CreateAndJoin_ReturnVoid()
        {
            // Arrange
            MethodInfo createLobby = typeof(ILobbyService).GetMethod("CreateLobby");
            MethodInfo joinLobby = typeof(ILobbyService).GetMethod("JoinLobby");
            // Act
            object[] returnTypes = { createLobby.ReturnType, joinLobby.ReturnType };
            // Assert
            Assert.That(returnTypes, Is.EqualTo(new object[] { typeof(void), typeof(void) }));
        }

        [Test]
        public void CallbackContract_AllOperations_AreOneWay()
        {
            // Arrange
            MethodInfo[] methods = typeof(ILobbyCallback).GetMethods();
            // Act
            bool allOperationsAreOneWay = methods.All(method => method
                .GetCustomAttribute<OperationContractAttribute>()
                .IsOneWay);
            // Assert
            Assert.That(allOperationsAreOneWay, Is.True);
        }
    }
}
