using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Services;
using NUnit.Framework;
using Proxy = global::DominoAllFivesService;

namespace DominoAllFives.Server.AdditionalTest
{
    [TestFixture]
    public sealed class RegistrationServiceWcfTests
    {
        [Test]
        public void Register_NullRequest_ReturnsTypedInvalidDataFault()
        {
            // Arrange
            Proxy.AccountRegistration request = null;
            // Act
            Proxy.RegistrationIssue issue = InvokeRegistration(request);
            // Assert
            Assert.That(issue, Is.EqualTo(Proxy.RegistrationIssue.InvalidRegistrationData));
        }

        [TestCase(null, "player@gmail.com", "StrongPass1!",
            Proxy.RegistrationIssue.InvalidUsername)]
        [TestCase("   ", "invalid-email", "weak", Proxy.RegistrationIssue.InvalidUsername)]
        [TestCase("player", null, "StrongPass1!", Proxy.RegistrationIssue.InvalidEmail)]
        [TestCase("player", "invalid-email", "weak", Proxy.RegistrationIssue.InvalidEmail)]
        [TestCase("player", "player@example.com", "StrongPass1!",
            Proxy.RegistrationIssue.InvalidEmail)]
        [TestCase("player", "player@gmail.com", null, Proxy.RegistrationIssue.InvalidPassword)]
        [TestCase("player", "player@gmail.com", "weak", Proxy.RegistrationIssue.InvalidPassword)]
        public void Register_InvalidInput_ReturnsTypedFaultBeforePersistence(
            string username,
            string email,
            string password,
            Proxy.RegistrationIssue expectedIssue)
        {
            // Arrange
            Proxy.AccountRegistration request = new Proxy.AccountRegistration
            {
                Username = username,
                Email = email,
                Password = password,
                PreferredLanguageId = "es-MX"
            };
            // Act
            Proxy.RegistrationIssue issue = InvokeRegistration(request);
            // Assert
            Assert.That(issue, Is.EqualTo(expectedIssue));
        }

        [Test]
        public void Serialize_RegistrationRequest_PreservesFieldsAcrossGeneratedProxy()
        {
            // Arrange
            AccountRegistration request = AccountRegistration.Create(
                "player",
                "player@gmail.com",
                "  StrongPass1!  ",
                "pt-BR");
            DataContractSerializer serverSerializer =
                new DataContractSerializer(typeof(AccountRegistration));
            DataContractSerializer proxySerializer =
                new DataContractSerializer(typeof(Proxy.AccountRegistration));
            using (MemoryStream stream = new MemoryStream())
            {
                // Act
                serverSerializer.WriteObject(stream, request);
                stream.Position = 0;
                Proxy.AccountRegistration generatedRequest =
                    (Proxy.AccountRegistration)proxySerializer.ReadObject(stream);
                stream.SetLength(0);
                stream.Position = 0;
                proxySerializer.WriteObject(stream, generatedRequest);
                stream.Position = 0;
                AccountRegistration restored =
                    (AccountRegistration)serverSerializer.ReadObject(stream);
                // Assert
                Assert.That(
                    typeof(Proxy.AccountRegistration).Assembly,
                    Is.Not.EqualTo(typeof(AccountRegistration).Assembly));
                Assert.That(
                    new[]
                    {
                        restored.Username,
                        restored.Email,
                        restored.Password,
                        restored.PreferredLanguageId
                    },
                    Is.EqualTo(new[]
                    {
                        "player",
                        "player@gmail.com",
                        "  StrongPass1!  ",
                        "pt-BR"
                    }));
            }
        }

        private static Proxy.RegistrationIssue InvokeRegistration(Proxy.AccountRegistration request)
        {
            string addressText = "net.pipe://localhost/registration/"
                + Guid.NewGuid().ToString("N");
            Uri address = new Uri(addressText);
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            ServiceHost host = new ServiceHost(typeof(DominoService), address);
            host.AddServiceEndpoint(typeof(IRegistrationService), binding, string.Empty);
            Proxy.RegistrationServiceClient client = new Proxy.RegistrationServiceClient(
                binding,
                new EndpointAddress(address));
            try
            {
                host.Open();
                try
                {
                    client.Register(request);
                }
                catch (FaultException<Proxy.RegistrationIssue> ex)
                {
                    return ex.Detail;
                }

                Assert.Fail("The invalid registration must return a typed rejection fault.");
                return default(Proxy.RegistrationIssue);
            }
            finally
            {
                client.Abort();
                host.Abort();
            }
        }
    }
}
