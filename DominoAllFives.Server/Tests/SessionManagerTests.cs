using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DominoAllFives.BusinessLogic.Managers;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class SessionManagerTests
    {
        [Test]
        public void CreateSession_ValidPlayer_ResolvesPlayerId()
        {
            // Arrange
            const int PlayerId = 91001;
            string sessionKey = null;
            try
            {
                // Act
                sessionKey = SessionManager.CreateSession(PlayerId);
                int resolvedPlayerId = SessionManager.GetPlayerId(sessionKey);
                // Assert
                Assert.That(resolvedPlayerId, Is.EqualTo(PlayerId));
            }
            finally
            {
                RemoveSessionWhenPresent(sessionKey);
            }
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void CreateSession_InvalidPlayerId_ThrowsArgumentOutOfRangeException(int playerId)
        {
            // Arrange
            int invalidPlayerId = playerId;
            // Act
            TestDelegate createSession = () => SessionManager.CreateSession(invalidPlayerId);
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(createSession);
        }

        [Test]
        public void CreateSession_SamePlayerTwice_InvalidatesPreviousKey()
        {
            // Arrange
            const int PlayerId = 91002;
            string firstKey = SessionManager.CreateSession(PlayerId);
            string secondKey = null;
            try
            {
                // Act
                secondKey = SessionManager.CreateSession(PlayerId);
                TestDelegate resolveFirstKey = () => SessionManager.GetPlayerId(firstKey);
                // Assert
                Assert.Throws<KeyNotFoundException>(resolveFirstKey);
            }
            finally
            {
                RemoveSessionWhenPresent(firstKey);
                RemoveSessionWhenPresent(secondKey);
            }
        }

        [Test]
        public void CreateSession_DifferentPlayers_ProducesDifferentKeys()
        {
            // Arrange
            string firstKey = null;
            string secondKey = null;
            try
            {
                // Act
                firstKey = SessionManager.CreateSession(91003);
                secondKey = SessionManager.CreateSession(91004);
                // Assert
                Assert.That(firstKey, Is.Not.EqualTo(secondKey));
            }
            finally
            {
                RemoveSessionWhenPresent(firstKey);
                RemoveSessionWhenPresent(secondKey);
            }
        }

        [Test]
        public void RemoveSession_ExistingSession_ReturnsTrue()
        {
            // Arrange
            string sessionKey = SessionManager.CreateSession(91005);
            // Act
            bool removed = SessionManager.RemoveSession(sessionKey);
            // Assert
            Assert.That(removed, Is.True);
        }

        [Test]
        public void GetPlayerId_NullKey_ThrowsArgumentNullException()
        {
            // Arrange
            string sessionKey = null;
            // Act
            TestDelegate getPlayerId = () => SessionManager.GetPlayerId(sessionKey);
            // Assert
            Assert.Throws<ArgumentNullException>(getPlayerId);
        }

        [TestCase("")]
        [TestCase("   ")]
        public void GetPlayerId_BlankKey_ThrowsArgumentException(string sessionKey)
        {
            // Arrange
            string invalidSessionKey = sessionKey;
            // Act
            TestDelegate getPlayerId = () => SessionManager.GetPlayerId(invalidSessionKey);
            // Assert
            Assert.Throws<ArgumentException>(getPlayerId);
        }

        [Test]
        public void CreateSession_ConcurrentPlayers_PreservesEveryMapping()
        {
            // Arrange
            const int FirstPlayerId = 91100;
            const int PlayerCount = 12;
            ConcurrentDictionary<int, string> sessions =
                new ConcurrentDictionary<int, string>();
            try
            {
                // Act
                Parallel.For(
                    FirstPlayerId,
                    FirstPlayerId + PlayerCount,
                    playerId => sessions[playerId] = SessionManager.CreateSession(playerId));
                bool allMappingsAreValid = sessions.All(
                    session => SessionManager.GetPlayerId(session.Value) == session.Key);
                // Assert
                Assert.That(allMappingsAreValid, Is.True);
            }
            finally
            {
                foreach (string sessionKey in sessions.Values)
                {
                    RemoveSessionWhenPresent(sessionKey);
                }
            }
        }

        private static void RemoveSessionWhenPresent(string sessionKey)
        {
            if (!string.IsNullOrWhiteSpace(sessionKey))
            {
                SessionManager.RemoveSession(sessionKey);
            }
        }
    }
}
