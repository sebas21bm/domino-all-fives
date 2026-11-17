using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.Services.Wcf.Services;
using MatchService;
using NUnit.Framework;

namespace DominoAllFives.Client.Tests
{
    [TestFixture]
    public sealed class MatchProxyTests
    {
        [TestCase("PlayTile")]
        [TestCase("DrawTiles")]
        [TestCase("PassTurn")]
        [TestCase("LeaveMatch")]
        public void GeneratedContract_PublishedOperation_ReturnsVoidWithFaultReply(string name)
        {
            // Arrange
            MethodInfo method = typeof(IMatchService).GetMethod(name);
            // Act
            OperationContractAttribute operation = method
                .GetCustomAttribute<OperationContractAttribute>();
            Type[] faults = method.GetCustomAttributes<FaultContractAttribute>()
                .Select(fault => fault.DetailType)
                .ToArray();
            object[] signature = { method.ReturnType, operation.IsOneWay, faults };
            // Assert
            Assert.That(signature, Is.EqualTo(new object[]
            {
                typeof(void), false, new[] { typeof(AccessIssue), typeof(MatchIssue) }
            }));
        }

        [TestCase("Play")]
        [TestCase("Draw")]
        [TestCase("Pass")]
        public void ClientCommand_PublicSignature_ExposesNoSnapshot(string name)
        {
            // Arrange
            MethodInfo contract = typeof(IMatchClient).GetMethod(name);
            MethodInfo implementation = typeof(MatchClient).GetMethod(name);
            // Act
            Type[] returnTypes = { contract.ReturnType, implementation.ReturnType };
            // Assert
            Assert.That(returnTypes, Is.EqualTo(new[] { typeof(void), typeof(void) }));
        }

        [Test]
        public void ClientAssemblies_GeneratedProxy_DoNotReferenceServerContracts()
        {
            // Arrange
            const string ServerContractsAssembly = "DominoAllFives.Contracts";
            System.Reflection.Assembly application = typeof(MatchClient).Assembly;
            System.Reflection.Assembly tests = typeof(MatchProxyTests).Assembly;
            // Act
            bool applicationReferencesServer = application.GetReferencedAssemblies().Any(
                assembly => assembly.Name == ServerContractsAssembly);
            bool testsReferenceServer = tests.GetReferencedAssemblies().Any(
                assembly => assembly.Name == ServerContractsAssembly);
            // Assert
            Assert.That(new object[]
            {
                typeof(MatchServiceClient).Assembly, typeof(DominoTile).Assembly,
                applicationReferencesServer, testsReferenceServer
            }, Is.EqualTo(new object[] { application, application, false, false }));
        }
    }
}
