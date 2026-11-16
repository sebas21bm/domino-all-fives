using System;
using System.ServiceModel;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Services;
using NUnit.Framework;

namespace DominoAllFives.Server.AdditionalTest
{
    [TestFixture]
    [NonParallelizable]
    public sealed class LobbyServiceWcfTests
    {
        [Test]
        public void OpenLobbySession_InvalidKey_ReturnsTypedAccessFaultOverNamedPipe()
        {
            // Arrange
            string address = "net.pipe://localhost/DominoAllFives/LobbyTest/"
                + Guid.NewGuid().ToString("N");
            NetNamedPipeBinding binding =
                new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            using (ServiceHost host = new ServiceHost(
                typeof(LobbyService),
                new Uri(address)))
            {
                host.AddServiceEndpoint(typeof(ILobbyService), binding, string.Empty);
                host.Open();
                DuplexChannelFactory<ILobbyService> factory =
                    new DuplexChannelFactory<ILobbyService>(
                        new InstanceContext(new Callback()),
                        binding,
                        new EndpointAddress(address));
                ILobbyService channel = factory.CreateChannel();
                try
                {
                    // Act
                    TestDelegate openSession = () =>
                        channel.OpenLobbySession("invalid-session-key");
                    // Assert
                    Assert.That(
                        openSession,
                        Throws.TypeOf<FaultException<AccessIssue>>()
                            .With.Property("Detail").EqualTo(AccessIssue.SessionRequired));
                }
                finally
                {
                    ((ICommunicationObject)channel).Abort();
                    factory.Abort();
                }
            }
        }

        private sealed class Callback : ILobbyCallback
        {
            public void OnLobbyChanged(Lobby lobby)
            {
            }

            public void OnLobbyClosed()
            {
            }

            public void OnMatchStarted(int matchId)
            {
            }
        }
    }
}
