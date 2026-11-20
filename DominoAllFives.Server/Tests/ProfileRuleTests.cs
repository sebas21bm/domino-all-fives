using System;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class ProfileRuleTests
    {
        private const int UnstoredPlayerId = 1795301002;
        private const int MaximumPictureSize = 1024 * 1024;

        [TestCase(ProfileIssue.PlayerNotFound)]
        [TestCase(ProfileIssue.InvalidUsername)]
        [TestCase(ProfileIssue.SameUsername)]
        [TestCase(ProfileIssue.UsernameAlreadyExists)]
        [TestCase(ProfileIssue.InvalidProfilePictureFormat)]
        [TestCase(ProfileIssue.ProfilePictureTooLarge)]
        [TestCase(ProfileIssue.InvalidProfilePicture)]
        public void Constructor_DefinedProfileIssue_PreservesIssue(ProfileIssue issue)
        {
            // Arrange
            ProfileIssue expectedIssue = issue;
            // Act
            ProfileRuleException exception = new ProfileRuleException(issue);
            // Assert
            Assert.That(exception.Issue, Is.EqualTo(expectedIssue));
        }

        [TestCase(-1)]
        [TestCase(int.MaxValue)]
        public void Constructor_UndefinedProfileIssue_ThrowsArgumentOutOfRangeException(int value)
        {
            // Arrange
            ProfileIssue issue = (ProfileIssue)value;
            // Act
            TestDelegate create = CreateException;
            // Assert
            Assert.That(create, Throws.TypeOf<ArgumentOutOfRangeException>());

            void CreateException()
            {
                new ProfileRuleException(issue);
            }
        }

        [Test]
        public void Constructor_ProfileControllerWithoutStorage_ThrowsArgumentNullException()
        {
            // Arrange
            ProfilePictureStorage storage = null;
            // Act
            TestDelegate create = CreateController;
            // Assert
            Assert.That(create, Throws.TypeOf<ArgumentNullException>());

            void CreateController()
            {
                new ProfileController(storage);
            }
        }

        [Test]
        public void Constructor_PictureControllerWithoutStorage_ThrowsArgumentNullException()
        {
            // Arrange
            ProfilePictureStorage storage = null;
            // Act
            TestDelegate create = CreateController;
            // Assert
            Assert.That(create, Throws.TypeOf<ArgumentNullException>());

            void CreateController()
            {
                new ProfilePictureController(storage);
            }
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void GetProfile_InvalidPlayerId_ThrowsArgumentOutOfRangeException(int playerId)
        {
            // Arrange
            ProfileController controller = new ProfileController(CreateStorage());
            // Act
            TestDelegate getProfile = GetProfile;
            // Assert
            Assert.That(getProfile, Throws.TypeOf<ArgumentOutOfRangeException>());

            void GetProfile()
            {
                controller.GetProfile(playerId);
            }
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void UpdateProfileName_InvalidPlayerId_ThrowsArgumentOutOfRangeException(int playerId)
        {
            // Arrange
            ProfileController controller = new ProfileController(CreateStorage());
            // Act
            TestDelegate update = UpdateProfileName;
            // Assert
            Assert.That(update, Throws.TypeOf<ArgumentOutOfRangeException>());

            void UpdateProfileName()
            {
                controller.UpdateProfileName(playerId, "player");
            }
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void UpdateProfileWithPicture_InvalidPlayerId_ThrowsArgumentOutOfRangeException(
            int playerId)
        {
            // Arrange
            ProfileController controller = new ProfileController(CreateStorage());
            ProfilePicture picture = ProfilePicture.FromSelectedContent(
                "avatar.png",
                new byte[] { 1 });
            // Act
            TestDelegate update = UpdateProfileWithPicture;
            // Assert
            Assert.That(update, Throws.TypeOf<ArgumentOutOfRangeException>());

            void UpdateProfileWithPicture()
            {
                controller.UpdateProfileWithPicture(playerId, "player", picture);
            }
        }

        [Test]
        public void UpdateProfileWithPicture_NullReplacement_ThrowsArgumentNullExceptionBeforePersistence()
        {
            // Arrange
            ProfileController controller = new ProfileController(CreateStorage());
            // Act
            TestDelegate update = UpdateProfileWithPicture;
            // Assert
            Assert.That(update, Throws.TypeOf<ArgumentNullException>());

            void UpdateProfileWithPicture()
            {
                controller.UpdateProfileWithPicture(UnstoredPlayerId, "player", null);
            }
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void SetProfilePicture_InvalidPlayerId_ThrowsArgumentOutOfRangeException(
            int playerId)
        {
            // Arrange
            ProfilePictureController controller = new ProfilePictureController(CreateStorage());
            // Act
            TestDelegate setPicture = SetPicture;
            // Assert
            Assert.That(setPicture, Throws.TypeOf<ArgumentOutOfRangeException>());

            void SetPicture()
            {
                controller.SetProfilePicture(playerId, null);
            }
        }

        [Test]
        public void SetProfilePicture_NullPicture_ThrowsArgumentNullExceptionBeforePersistence()
        {
            // Arrange
            ProfilePictureController controller = new ProfilePictureController(CreateStorage());
            // Act
            TestDelegate setPicture = SetPicture;
            // Assert
            Assert.That(setPicture, Throws.TypeOf<ArgumentNullException>());

            void SetPicture()
            {
                controller.SetProfilePicture(UnstoredPlayerId, null);
            }
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void RemoveProfilePicture_InvalidPlayerId_ThrowsArgumentOutOfRangeException(
            int playerId)
        {
            // Arrange
            ProfilePictureController controller = new ProfilePictureController(CreateStorage());
            // Act
            TestDelegate removePicture = RemovePicture;
            // Assert
            Assert.That(removePicture, Throws.TypeOf<ArgumentOutOfRangeException>());

            void RemovePicture()
            {
                controller.RemoveProfilePicture(playerId);
            }
        }

        [TestCase("absent", ProfileIssue.InvalidProfilePicture)]
        [TestCase("empty", ProfileIssue.InvalidProfilePicture)]
        [TestCase("format", ProfileIssue.InvalidProfilePictureFormat)]
        [TestCase("missing-name", ProfileIssue.InvalidProfilePictureFormat)]
        [TestCase("format-before-size", ProfileIssue.InvalidProfilePictureFormat)]
        [TestCase("large", ProfileIssue.ProfilePictureTooLarge)]
        public void SetProfilePicture_InvalidContent_ThrowsProfileRuleBeforePersistence(
            string selection,
            ProfileIssue expectedIssue)
        {
            // Arrange
            ProfilePictureController controller = new ProfilePictureController(CreateStorage());
            ProfilePicture picture = CreateInvalidPicture(selection);
            // Act
            TestDelegate setPicture = SetPicture;
            // Assert
            Assert.That(setPicture, Throws.TypeOf<ProfileRuleException>()
                .With.Property("Issue").EqualTo(expectedIssue));

            void SetPicture()
            {
                controller.SetProfilePicture(UnstoredPlayerId, picture);
            }
        }

        private static ProfilePictureStorage CreateStorage()
        {
            return new ProfilePictureStorage(NullLogger<ProfilePictureStorage>.Instance);
        }

        private static ProfilePicture CreateInvalidPicture(string selection)
        {
            switch (selection)
            {
                case "absent":
                    return ProfilePicture.FromSelectedContent("avatar.png", null);
                case "empty":
                    return ProfilePicture.FromSelectedContent("avatar.png", Array.Empty<byte>());
                case "format":
                    return ProfilePicture.FromSelectedContent("avatar.txt", new byte[] { 1 });
                case "missing-name":
                    return ProfilePicture.FromSelectedContent(null, new byte[] { 1 });
                case "format-before-size":
                    return ProfilePicture.FromSelectedContent(
                        "avatar.txt",
                        new byte[MaximumPictureSize + 1]);
                case "large":
                    return ProfilePicture.FromSelectedContent(
                        "avatar.png",
                        new byte[MaximumPictureSize + 1]);
                default:
                    throw new ArgumentOutOfRangeException(nameof(selection));
            }
        }
    }
}
