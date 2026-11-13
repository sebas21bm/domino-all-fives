using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Services;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public class CredentialsTests
    {
        [Test]
        public void Contract_RenamedSessionInterface_PreservesPublishedServiceName()
        {
            // Arrange
            System.Type serviceType = typeof(ISessionAccountService);

            // Act
            ServiceContractAttribute contract = (ServiceContractAttribute)
                serviceType.GetCustomAttributes(typeof(ServiceContractAttribute), false)[0];

            // Assert
            Assert.That(contract.Name, Is.EqualTo("ISessionAccount"));
            Assert.That(contract.Namespace, Is.EqualTo("urn:domino-all-fives:services"));
        }

        [TestCase(LobbyIssue.NotFound, "LobbyNotFound")]
        [TestCase(LobbyIssue.Unavailable, "LobbyUnavailable")]
        [TestCase(LobbyIssue.Full, "LobbyFull")]
        public void Serialize_RenamedLobbyIssue_PreservesPublishedValue(
            LobbyIssue issue, string expectedWireValue)
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
                Assert.That(document.DocumentElement.InnerText, Is.EqualTo(expectedWireValue));
                Assert.That(restored, Is.EqualTo(issue));
            }
        }

        [Test]
        public void Serialize_Credentials_PreservesWireNameAndPassword()
        {
            // Arrange
            Credentials credentials = Credentials.ForLogin("  player  ", "  secret  ");
            DataContractSerializer serializer = new DataContractSerializer(typeof(Credentials));
            using (MemoryStream stream = new MemoryStream())
            {
                // Act
                serializer.WriteObject(stream, credentials);
                stream.Position = 0;
                XmlDocument document = new XmlDocument();
                document.Load(stream);
                stream.Position = 0;
                Credentials restored = (Credentials)serializer.ReadObject(stream);
                // Assert
                Assert.That(document.DocumentElement.LocalName, Is.EqualTo("Credentials"));
                Assert.That(
                    document.DocumentElement.NamespaceURI,
                    Is.EqualTo(
                    "http://schemas.datacontract.org/2004/07/DominoAllFives.Contracts.Domain"));
                Assert.That(restored.EmailOrUsername, Is.EqualTo("player"));
                Assert.That(restored.Password, Is.EqualTo("  secret  "));
            }
        }
    }
}
