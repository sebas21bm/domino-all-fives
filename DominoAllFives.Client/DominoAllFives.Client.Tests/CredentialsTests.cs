using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using NUnit.Framework;
using DominoAllFivesService;

namespace DominoAllFives.Client.Tests
{
    [TestFixture]
    public class CredentialsTests
    {
        [Test]
        public void Serialize_Credentials_PreservesWireNameAndPassword()
        {
            // Arrange
            Credentials credentials = new Credentials { EmailOrUsername = "player", Password = "  secret  " };
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
                Assert.That(document.DocumentElement.NamespaceURI,
                    Is.EqualTo("http://schemas.datacontract.org/2004/07/DominoAllFives.Contracts.Domain"));
                Assert.That(restored.EmailOrUsername, Is.EqualTo("player"));
                Assert.That(restored.Password, Is.EqualTo("  secret  "));
            }
        }
    }
}
