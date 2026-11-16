using System;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.Contracts.Enums;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public class AccountRuleExceptionTests
    {
        [TestCase(AccountIssue.InvalidCredentials)]
        [TestCase(AccountIssue.Banned)]
        [TestCase(AccountIssue.Suspended)]
        public void Constructor_DefinedIssue_PreservesIssue(AccountIssue issue)
        {
            // Arrange
            AccountIssue expectedIssue = issue;
            // Act
            AccountRuleException exception = new AccountRuleException(issue);
            // Assert
            Assert.That(exception.Issue, Is.EqualTo(expectedIssue));
        }

        [Test]
        public void Constructor_UndefinedIssue_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            AccountIssue issue = (AccountIssue)int.MaxValue;
            // Act
            TestDelegate createException = () => new AccountRuleException(issue);
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(createException);
        }
    }
}
