using System.ServiceModel;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Services;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public class LobbyServiceWcfTests
    {
        [Test]
        public void OpenLobbySession_InvalidKey_ReturnsTypedAccessFaultOverNetTcp()
        {
            const string address = "net.tcp://localhost:18735/LobbyTest";
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            using (ServiceHost host = new ServiceHost(typeof(LobbyService)))
            {
                host.AddServiceEndpoint(typeof(ILobbyService), binding, address);
                host.Open();

                DuplexChannelFactory<ILobbyService> factory =
                    new DuplexChannelFactory<ILobbyService>(new InstanceContext(new Callback()),
                        binding, new EndpointAddress(address));
                ILobbyService channel = factory.CreateChannel();
                try
                {
                    FaultException<AccessIssue> error =
                        Assert.Throws<FaultException<AccessIssue>>(() =>
                            channel.OpenLobbySession("invalid-session-key"));
                    Assert.That(error.Detail, Is.EqualTo(AccessIssue.SessionRequired));
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
            public void OnLobbyChanged(Lobby lobby) { }
            public void OnLobbyClosed() { }
            public void OnMatchStarted(int matchId) { }
        }
    }
}
