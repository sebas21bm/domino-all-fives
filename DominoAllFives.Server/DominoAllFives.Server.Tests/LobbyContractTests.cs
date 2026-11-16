using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using DominoAllFives.Contracts.Enums;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public class LobbyContractTests
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
    }
}
