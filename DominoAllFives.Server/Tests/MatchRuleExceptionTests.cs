using System;
using DominoAllFives.BusinessLogic.Gameplay;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class MatchRuleExceptionTests
    {
        [TestCase(MatchIssue.MatchNotFound)]
        [TestCase(MatchIssue.StaleVersion)]
        [TestCase(MatchIssue.InvalidPlacement)]
        public void Constructor_DefinedIssue_PreservesIssue(MatchIssue issue)
        {
            // Arrange
            // Act
            MatchRuleException exception = new MatchRuleException(issue);
            // Assert
            Assert.That(exception.Issue, Is.EqualTo(issue));
        }

        [Test]
        public void Constructor_UndefinedIssue_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            MatchIssue issue = (MatchIssue)int.MaxValue;
            // Act
            ArgumentOutOfRangeException exception = null;
            try
            {
                new MatchRuleException(issue);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                exception = ex;
            }
            // Assert
            Assert.That(exception, Is.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
