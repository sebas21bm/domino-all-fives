using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Gameplay;
using DominoAllFives.Contracts.Services;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class MatchContractTests
    {
        [TestCase(typeof(DominoTile))]
        [TestCase(typeof(MatchView))]
        [TestCase(typeof(MatchResult))]
        [TestCase(typeof(PlacedTile))]
        [TestCase(typeof(PlayerHand))]
        [TestCase(typeof(PlayerScore))]
        [TestCase(typeof(PlayScore))]
        [TestCase(typeof(PublicBoard))]
        [TestCase(typeof(PublicEnd))]
        [TestCase(typeof(RoundResult))]
        public void DataContract_Model_DeclaresSimpleMembers(Type modelType)
        {
            // Arrange
            PropertyInfo[] properties = modelType.GetProperties();
            // Act
            bool explicitContract = modelType.GetCustomAttribute<DataContractAttribute>() != null;
            foreach (PropertyInfo property in properties)
            {
                DataMemberAttribute member = property.GetCustomAttribute<DataMemberAttribute>();
                if (member == null || member.Order != -1 || member.IsRequired)
                {
                    explicitContract = false;
                    break;
                }
            }
            // Assert
            Assert.That(explicitContract, Is.True);
        }

        [TestCase(typeof(BoardEnd))]
        [TestCase(typeof(MatchIssue))]
        [TestCase(typeof(MatchStatus))]
        [TestCase(typeof(SpecialPower))]
        public void DataContract_Enum_MarksEveryValue(Type enumType)
        {
            // Arrange
            string[] names = Enum.GetNames(enumType);
            // Act
            bool explicitContract = enumType.GetCustomAttribute<DataContractAttribute>() != null;
            foreach (string name in names)
            {
                FieldInfo field = enumType.GetField(name);
                if (field.GetCustomAttribute<EnumMemberAttribute>() == null)
                {
                    explicitContract = false;
                    break;
                }
            }
            // Assert
            Assert.That(explicitContract, Is.True);
        }

        [TestCase("PlayTile")]
        [TestCase("DrawTiles")]
        [TestCase("PassTurn")]
        [TestCase("LeaveMatch")]
        public void ServiceContract_PublishedOperation_ReturnsVoidWithFaultReply(
            string operationName)
        {
            // Arrange
            MethodInfo method = typeof(IMatchService).GetMethod(operationName);
            // Act
            OperationContractAttribute operation = method
                .GetCustomAttribute<OperationContractAttribute>();
            object[] signature = { method.ReturnType, operation.IsOneWay, operation.IsInitiating };
            // Assert
            Assert.That(signature, Is.EqualTo(new object[] { typeof(void), false, false }));
        }

        [TestCase("OpenMatchSession", typeof(MatchView))]
        [TestCase("GetMatch", typeof(MatchView))]
        [TestCase("GetHand", typeof(PlayerHand))]
        public void ServiceContract_QueryOperation_ReturnsSnapshot(
            string operationName,
            Type expectedType)
        {
            // Arrange
            MethodInfo method = typeof(IMatchService).GetMethod(operationName);
            // Act
            Type returnType = method.ReturnType;
            // Assert
            Assert.That(returnType, Is.EqualTo(expectedType));
        }

        [TestCase("OpenMatchSession")]
        [TestCase("GetMatch")]
        [TestCase("GetHand")]
        [TestCase("PlayTile")]
        [TestCase("DrawTiles")]
        [TestCase("PassTurn")]
        [TestCase("LeaveMatch")]
        public void ServiceContract_Operation_DeclaresAccessAndMatchFaults(string operationName)
        {
            // Arrange
            MethodInfo method = typeof(IMatchService).GetMethod(operationName);
            // Act
            Type[] details = method.GetCustomAttributes<FaultContractAttribute>()
                .Select(fault => fault.DetailType)
                .ToArray();
            // Assert
            Assert.That(
                details,
                Is.EquivalentTo(new[] { typeof(AccessIssue), typeof(MatchIssue) }));
        }

        [TestCase("OnMatchChanged")]
        [TestCase("OnPrivateHandChanged")]
        [TestCase("OnRoundFinished")]
        [TestCase("OnMatchFinished")]
        public void CallbackContract_Operation_ReturnsVoidOneWay(string operationName)
        {
            // Arrange
            MethodInfo method = typeof(IMatchCallback).GetMethod(operationName);
            // Act
            OperationContractAttribute operation = method
                .GetCustomAttribute<OperationContractAttribute>();
            object[] signature = { method.ReturnType, operation.IsOneWay };
            // Assert
            Assert.That(signature, Is.EqualTo(new object[] { typeof(void), true }));
        }

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
