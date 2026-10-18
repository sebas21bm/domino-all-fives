using System;
using DominoAllFives.Server.Controllers;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public class AuthenticationControllerTests
    {
        [Test]
        public void Login_InvalidCredentials_RejectsWithoutOpeningDatabase()
        {
            AuthenticationController controller = new AuthenticationController(
                NullLogger<AuthenticationController>.Instance);
            LoginResult result = controller.Login(LoginCredentials.ForLogin("", ""));
            Assert.That(result.Account, Is.Null);
            Assert.That(result.Reason, Is.EqualTo(LoginFailureReason.InvalidCredentials));
            Assert.Throws<ArgumentNullException>(() => controller.Login(null));
        }

        [Test]
        public void RemoteControllers_AreOwnedOnlyByServerAssembly()
        {
            var serverAssembly = typeof(DominoAllFives.Server.DominoService).Assembly;
            var businessAssembly = typeof(DominoAllFives.BusinessLogic.Controllers.RegistrationController).Assembly;
            Assert.That(typeof(AuthenticationController).Assembly, Is.SameAs(serverAssembly));
            Assert.That(typeof(RankingController).Assembly, Is.SameAs(serverAssembly));
            Assert.That(businessAssembly.GetType("DominoAllFives.BusinessLogic.Controllers.AuthenticationController"), Is.Null);
            Assert.That(businessAssembly.GetType("DominoAllFives.BusinessLogic.Controllers.RankingController"), Is.Null);
        }
    }
}