using System.Reflection;
using System.ServiceModel;
using DominoAllFives.Contracts.Services;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class MatchContractTests
    {
        [Test]
        public void ServiceContract_Match_PreservesDuplexSessionConfiguration()
        {
            // Arrange
            ServiceContractAttribute contract = typeof(IMatchService)
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
                Is.EqualTo(new object[] { typeof(IMatchCallback), SessionMode.Required }));
        }

        [Test]
        public void ServiceContract_OpenSession_IsOnlyInitiatingOperation()
        {
            // Arrange
            MethodInfo[] methods = typeof(IMatchService).GetMethods();
            // Act
            bool preservesSessionLifecycle = true;
            foreach (MethodInfo method in methods)
            {
                OperationContractAttribute operation = method
                    .GetCustomAttribute<OperationContractAttribute>();
                bool isOpenSession = method.Name == nameof(IMatchService.OpenMatchSession);
                if (operation.IsInitiating != isOpenSession)
                {
                    preservesSessionLifecycle = false;
                    break;
                }
            }
            // Assert
            Assert.That(preservesSessionLifecycle, Is.True);
        }

        [Test]
        public void CallbackContract_AllOperations_AreOneWay()
        {
            // Arrange
            MethodInfo[] methods = typeof(IMatchCallback).GetMethods();
            // Act
            bool allOperationsAreOneWay = true;
            foreach (MethodInfo method in methods)
            {
                OperationContractAttribute operation = method
                    .GetCustomAttribute<OperationContractAttribute>();
                if (!operation.IsOneWay)
                {
                    allOperationsAreOneWay = false;
                    break;
                }
            }
            // Assert
            Assert.That(allOperationsAreOneWay, Is.True);
        }
    }
}
