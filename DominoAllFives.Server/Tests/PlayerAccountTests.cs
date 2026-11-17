using System;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public class PlayerAccountTests
    {
        [Test]
        public void FromStoredAccount_ValidValues_PreservesAccountIdentity()
        {
            // Arrange
            AccountAccessStatus accessStatus = AccountAccessStatus.FromStoredState(
                false,
                false,
                null);
            // Act
            PlayerAccount account = PlayerAccount.FromStoredAccount(
                7,
                "player",
                "es-MX",
                accessStatus);
            // Assert
            Assert.That(
                new object[]
                {
                    account.PlayerId,
                    account.Username,
                    account.PreferredLanguageId,
                    account.AccessStatus
                },
                Is.EqualTo(new object[] { 7, "player", "es-MX", accessStatus }));
        }

        [Test]
        public void FromStoredAccount_NullAccessStatus_ThrowsArgumentNullException()
        {
            // Arrange
            AccountAccessStatus accessStatus = null;
            // Act
            TestDelegate createAccount = () => PlayerAccount.FromStoredAccount(
                7,
                "player",
                "es-MX",
                accessStatus);
            // Assert
            Assert.Throws<ArgumentNullException>(createAccount);
        }

        [Test]
        public void SetPreferredLanguage_NullLanguage_ThrowsArgumentNullException()
        {
            // Arrange
            PlayerAccount account = CreateAccount();
            // Act
            TestDelegate setLanguage = () => account.SetPreferredLanguage(null);
            // Assert
            Assert.Throws<ArgumentNullException>(setLanguage);
        }

        [Test]
        public void SetPreferredLanguage_ExcessiveLength_ThrowsArgumentException()
        {
            // Arrange
            PlayerAccount account = CreateAccount();
            // Act
            TestDelegate setLanguage = () => account.SetPreferredLanguage("language-id");
            // Assert
            Assert.Throws<ArgumentException>(setLanguage);
        }

        [Test]
        public void SetSessionKey_NullKey_ThrowsArgumentNullException()
        {
            // Arrange
            PlayerAccount account = CreateAccount();
            // Act
            TestDelegate setSessionKey = () => account.SetSessionKey(null);
            // Assert
            Assert.Throws<ArgumentNullException>(setSessionKey);
        }

        [TestCase("")]
        [TestCase("   ")]
        public void SetSessionKey_BlankKey_ThrowsArgumentException(string sessionKey)
        {
            // Arrange
            PlayerAccount account = CreateAccount();
            // Act
            TestDelegate setSessionKey = () => account.SetSessionKey(sessionKey);
            // Assert
            Assert.Throws<ArgumentException>(setSessionKey);
        }

        [Test]
        public void SetSessionKey_ValidKey_PreservesKey()
        {
            // Arrange
            PlayerAccount account = CreateAccount();
            // Act
            account.SetSessionKey("session-key");
            // Assert
            Assert.That(account.SessionKey, Is.EqualTo("session-key"));
        }

        private static PlayerAccount CreateAccount()
        {
            AccountAccessStatus accessStatus = AccountAccessStatus.FromStoredState(
                false,
                false,
                null);
            return PlayerAccount.FromStoredAccount(
                7,
                "player",
                "es-MX",
                accessStatus);
        }
    }
}
