using System;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public class AccountAccessStatusTests
    {
        [TestCase(true, true, LoginStatus.InvalidCredentials)]
        [TestCase(false, true, LoginStatus.Banned)]
        [TestCase(false, false, LoginStatus.None)]
        public void EnsureCanLogin_StoredFlags_ReturnsExpectedStatus(
            bool isDeleted,
            bool isBanned,
            LoginStatus expectedStatus)
        {
            // Arrange
            AccountAccessStatus status = AccountAccessStatus.FromStoredState(
                isDeleted,
                isBanned,
                null);
            // Act
            LoginStatus result = status.EnsureCanLogin(DateTime.UtcNow);
            // Assert
            Assert.That(result, Is.EqualTo(expectedStatus));
        }

        [Test]
        public void EnsureCanLogin_FutureSuspension_ReturnsSuspended()
        {
            // Arrange
            DateTime currentTime = new DateTime(2026, 11, 16, 12, 0, 0, DateTimeKind.Utc);
            AccountAccessStatus status = AccountAccessStatus.FromStoredState(
                false,
                false,
                currentTime.AddMinutes(1));
            // Act
            LoginStatus result = status.EnsureCanLogin(currentTime);
            // Assert
            Assert.That(result, Is.EqualTo(LoginStatus.Suspended));
        }

        [TestCase(-1)]
        [TestCase(0)]
        public void EnsureCanLogin_ExpiredOrBoundarySuspension_ReturnsNone(int minuteOffset)
        {
            // Arrange
            DateTime currentTime = new DateTime(2026, 11, 16, 12, 0, 0, DateTimeKind.Utc);
            AccountAccessStatus status = AccountAccessStatus.FromStoredState(
                false,
                false,
                currentTime.AddMinutes(minuteOffset));
            // Act
            LoginStatus result = status.EnsureCanLogin(currentTime);
            // Assert
            Assert.That(result, Is.EqualTo(LoginStatus.None));
        }
    }
}
