using System;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public class PlayerRepositoryTests
    {
        [Test]
        public void Constructor_NullContext_ThrowsArgumentNullException()
        {
            // Arrange
            DominoAllFivesEntities context = null;
            // Act
            TestDelegate createRepository = () => new PlayerRepository(context);
            // Assert
            Assert.Throws<ArgumentNullException>(createRepository);
        }
    }
}
