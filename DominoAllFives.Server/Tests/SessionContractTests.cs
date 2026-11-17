using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Services;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public class SessionContractTests
    {
        [Test]
        public void ServiceContract_SessionAccount_PreservesPublishedIdentity()
        {
            // Arrange
            System.Type serviceType = typeof(ISessionAccountService);
            // Act
            ServiceContractAttribute contract = (ServiceContractAttribute)
                serviceType.GetCustomAttributes(typeof(ServiceContractAttribute), false)[0];
            // Assert
            Assert.That(
                new[] { contract.Name, contract.Namespace },
                Is.EqualTo(new[] { "ISessionAccount", "urn:domino-all-fives:services" }));
        }

        [Test]
        public void Serialize_Credentials_PreservesPublishedContract()
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
                Assert.That(
                    new object[]
                    {
                        document.DocumentElement.LocalName,
                        document.DocumentElement.NamespaceURI,
                        restored.EmailOrUsername,
                        restored.Password
                    },
                    Is.EqualTo(
                        new object[]
                        {
                            "Credentials",
                            "http://schemas.datacontract.org/2004/07/"
                                + "DominoAllFives.Contracts.Domain",
                            "player",
                            "  secret  "
                        }));
            }
        }
    }
}
