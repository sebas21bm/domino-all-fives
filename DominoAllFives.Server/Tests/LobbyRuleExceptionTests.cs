using System;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.Contracts.Enums;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class LobbyRuleExceptionTests
    {
        [TestCase(LobbyIssue.NotFound)]
        [TestCase(LobbyIssue.Full)]
        [TestCase(LobbyIssue.InsufficientPlayers)]
        public void Constructor_DefinedIssue_PreservesIssue(LobbyIssue issue)
        {
            // Arrange
            // Act
            LobbyRuleException exception = new LobbyRuleException(issue);
            // Assert
            Assert.That(exception.Issue, Is.EqualTo(issue));
        }

        [Test]
        public void Constructor_UndefinedIssue_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            LobbyIssue issue = (LobbyIssue)int.MaxValue;
            // Act
            TestDelegate createException = () => new LobbyRuleException(issue);
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(createException);
        }
    }
}
