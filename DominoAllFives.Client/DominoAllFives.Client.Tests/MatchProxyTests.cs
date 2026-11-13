using System.Linq;
using DominoAllFives.Client.WPF.Services.Wcf.Services;
using MatchService;
using NUnit.Framework;

namespace DominoAllFives.Client.Tests
{
    [TestFixture]
    public sealed class MatchProxyTests
    {
        [Test]
        public void ClientAssemblies_GeneratedProxy_DoNotReferenceServerContracts()
        {
            // Arrange
            const string ServerContractsAssembly = "DominoAllFives.Contracts";
            var application = typeof(WcfMatchClient).Assembly;
            var tests = typeof(MatchProxyTests).Assembly;
            // Act
            bool applicationReferencesServer = application.GetReferencedAssemblies()
                .Any(assembly => assembly.Name == ServerContractsAssembly);
            bool testsReferenceServer = tests.GetReferencedAssemblies()
                .Any(assembly => assembly.Name == ServerContractsAssembly);
            // Assert
            Assert.That(typeof(MatchServiceClient).Assembly, Is.EqualTo(application));
            Assert.That(typeof(DominoTile).Assembly, Is.EqualTo(application));
            Assert.That(applicationReferencesServer, Is.False);
            Assert.That(testsReferenceServer, Is.False);
        }
    }
}
