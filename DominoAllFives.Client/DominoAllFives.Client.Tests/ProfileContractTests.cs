using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.ProfilePicture;
using DominoAllFives.Client.WPF.ViewModels;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace DominoAllFives.Client.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ProfileContractTests
    {
        [Test]
        public void ProfilePicture_ExternalArrayMutation_DoesNotChangeSnapshot()
        {
            // Arrange.
            byte[] input = { 1, 2, 3 };
            ProfilePicture picture = ProfilePicture.FromSelectedContent("avatar.png", input);

            // Act.
            input[0] = 9;
            byte[] output = picture.ImageData;
            output[1] = 9;

            // Assert.
            Assert.That(picture.OriginalFileName, Is.EqualTo("avatar.png"));
            Assert.That(picture.ImageData, Is.EqualTo(new byte[] { 1, 2, 3 }));
        }

        [Test]
        public void PlayerProfile_NewAccountStatistics_AcceptsZerosAndMissingPicture()
        {
            // Arrange.
            PlayerStatistics statistics = PlayerStatistics.FromStoredStatistics(0, 0, 0);

            // Act.
            PlayerProfile profile = PlayerProfile.FromStoredProfile(
                "player", statistics, ProfilePicture.FromStoredContent(null));

            // Assert.
            Assert.That(profile.Statistics.GamesWon, Is.Zero);
            Assert.That(profile.Statistics.GamesPlayed, Is.Zero);
            Assert.That(profile.Statistics.TotalPointsScored, Is.Zero);
            Assert.That(profile.Picture.ImageData, Is.Null.Or.Empty);
        }

        [Test]
        public void Ranking_SourceCollectionMutation_PreservesOrderAndOwnPosition()
        {
            // Arrange.
            ProfilePicture picture = ProfilePicture.FromStoredContent(null);
            RankingEntry first = RankingEntry.FromRankedPlayer(1, "first", 5, picture);
            RankingEntry second = RankingEntry.FromRankedPlayer(2, "second", 5, picture);
            List<RankingEntry> source = new List<RankingEntry> { first, second };

            // Act.
            Ranking ranking = Ranking.FromOrderedEntries(source, 1);
            source.Reverse();
            source.Clear();

            // Assert.
            Assert.That(ranking.TopPlayers.Count, Is.EqualTo(2));
            Assert.That(ranking.TopPlayers[0].Username, Is.EqualTo("first"));
            Assert.That(ranking.TopPlayers[1].Rank, Is.EqualTo(2));
            Assert.That(ranking.CurrentPlayerRank, Is.EqualTo(1));
        }

        [Test]
        public void Ranking_EmptyTopAndNoRank_AreValidSnapshots()
        {
            // Arrange and act.
            Ranking ranking = Ranking.FromOrderedEntries(new RankingEntry[0], 0);

            // Assert.
            Assert.That(ranking.TopPlayers, Is.Empty);
            Assert.That(ranking.CurrentPlayerRank, Is.Zero);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void GetProfile_InvalidId_ThrowsBeforeDatabase(int playerId)
        {
            // Arrange.
            ProfileController controller = CreateProfileController();

            // Act.
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.GetProfile(playerId));

            // Assert.
        }

        [Test]
        public void UpdateProfile_InvalidIdAndPicture_RejectsArgumentFirst()
        {
            // Arrange.
            ProfileController controller = CreateProfileController();
            ProfilePicture picture = ProfilePicture.FromSelectedContent("bad.ext", null);

            // Act.
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.UpdateProfile(0, "", picture));

            // Assert.
        }

        [TestCase(1, "bad.ext", 0, ProfilePictureFailureReason.InvalidImage)]
        [TestCase(1, "bad.ext", 1048577, ProfilePictureFailureReason.InvalidFormat)]
        [TestCase(1, "avatar.PNG", 1048577, ProfilePictureFailureReason.FileTooLarge)]
        public void SetProfilePicture_InvalidData_PreservesErrorOrderBeforeDatabase(
            int playerId, string name, int byteCount, ProfilePictureFailureReason expected)
        {
            // Arrange.
            ProfilePictureController controller = new ProfilePictureController(
                CreateStorage(), NullLogger<ProfilePictureController>.Instance);

            // Act.
            ProfilePictureFailureReason reason = controller.SetProfilePicture(playerId,
                ProfilePicture.FromSelectedContent(name, new byte[byteCount]));

            // Assert.
            Assert.That(reason, Is.EqualTo(expected));
        }

        [TestCase(UpdateProfileFailureReason.InvalidUsername, "MessageAccount_msgInvalidUsername")]
        [TestCase(UpdateProfileFailureReason.UsernameAlreadyExists, "MessageAccount_msgUsernameUsed")]
        [TestCase(UpdateProfileFailureReason.InvalidProfilePicture,
            "MessageProfile_msgPhotoInvalid")]
        [TestCase(UpdateProfileFailureReason.InvalidProfilePictureFormat,
            "MessageProfile_msgPhotoInvalidFormat")]
        [TestCase(UpdateProfileFailureReason.ProfilePictureTooLarge,
            "MessageProfile_msgPhotoTooLarge")]
        [TestCase(UpdateProfileFailureReason.PlayerNotFound, "MessageProfile_msgUpdateProfileError")]
        [TestCase(UpdateProfileFailureReason.ServiceUnavailable, "MessageProfile_msgUpdateProfileError")]
        public void EditProfile_BusinessRejectionOrTechnicalFailure_PreservesDialogAndSession(
            UpdateProfileFailureReason reason, string expectedMessage)
        {
            // Arrange.
            ProfileService service = new ProfileService { UpdateFailure = reason };
            DialogRecorder dialogs = new DialogRecorder();
            PlayerSession session = CreateSession();
            EditProfileViewModel model = CreateEditor(service, dialogs, session);
            model.Username = "new-name";

            // Act.
            model.SaveCommand.Execute(null);

            // Assert.
            Assert.That(dialogs.Last.MessageKey, Is.EqualTo(expectedMessage));
            Assert.That(session.Username, Is.EqualTo("player"));
            Assert.That(dialogs.Last.OnAccept, Is.Null);
            Assert.That(service.PictureChanged, Is.False);
        }

        [Test]
        public void EditProfile_NoChange_DoesNotCallUpdate()
        {
            // Arrange.
            ProfileService service = new ProfileService();
            EditProfileViewModel model = CreateEditor(
                service, new DialogRecorder(), CreateSession());

            // Act.
            model.SaveCommand.Execute(null);

            // Assert.
            Assert.That(service.UpdateCount, Is.Zero);
        }

        [Test]
        public void EditProfile_NormalReturn_UpdatesSessionAndDefersNavigation()
        {
            // Arrange.
            ProfileService service = new ProfileService();
            DialogRecorder dialogs = new DialogRecorder();
            PlayerSession session = CreateSession();
            EditProfileViewModel model = CreateEditor(service, dialogs, session);
            model.Username = "new-name";

            // Act.
            model.SaveCommand.Execute(null);

            // Assert.
            Assert.That(session.Username, Is.EqualTo("new-name"));
            Assert.That(service.UpdateCount, Is.EqualTo(1));
            Assert.That(service.PictureChanged, Is.False);
            Assert.That(service.CurrentProfile.Picture.ImageData, Is.EqualTo(service.OriginalImage));
            Assert.That(service.CurrentProfile.Statistics.GamesWon, Is.EqualTo(4));
            Assert.That(dialogs.Last.MessageKey, Is.EqualTo("MessageProfile_msgProfileUpdated"));
            Assert.That(dialogs.Last.OnAccept, Is.Not.Null);
        }

        [TestCase(RetrieveInformationFailureReason.NotFound, "Global_msgNotFound")]
        [TestCase(RetrieveInformationFailureReason.ServiceUnavailable, "Global_msgConnectionError")]
        public void GetProfile_BusinessRejectionOrTechnicalFailure_PreservesUiMessage(
            RetrieveInformationFailureReason reason, string expectedMessage)
        {
            // Arrange.
            ProfileService service = new ProfileService { RetrievalFailure = reason };
            DialogRecorder dialogs = new DialogRecorder();

            // Act.
            ProfileViewModel model = new ProfileViewModel(
                new NavigationStub(), service, dialogs, CreateSession(), CreateImageService());

            // Assert.
            Assert.That(dialogs.Last.MessageKey, Is.EqualTo(expectedMessage));
            Assert.That(model, Is.Not.Null);
        }

        [Test]
        public void Rankings_SnapshotMapping_PreservesOrdinalAndDistinctOwnRank()
        {
            // Arrange.
            var picture = new DominoAllFivesService.ProfilePicture { ImageData = CreatePng() };
            var snapshot = new DominoAllFivesService.Ranking
            {
                TopPlayers = new[]
                {
                    new DominoAllFivesService.RankingEntry { Rank = 1, Username = "first", GamesWon = 5, Picture = picture },
                    new DominoAllFivesService.RankingEntry { Rank = 2, Username = "player", GamesWon = 5, Picture = picture }
                },
                CurrentPlayerRank = 1
            };

            // Act.
            RankingsViewModel model = new RankingsViewModel(
                new NavigationStub(), new RankingService(snapshot), CreateSession(),
                CreateImageService(), new DialogRecorder());

            // Assert.
            Assert.That(model.Leaderboard[1].Rank, Is.EqualTo(2));
            Assert.That(model.Leaderboard[1].StatisticValue, Is.EqualTo(5));
            Assert.That(model.CurrentPlayerRank, Is.EqualTo("1"));
        }

        [Test]
        public void Rankings_TechnicalFailure_ShowsLocalizedErrorAndLeavesEmptySnapshot()
        {
            DialogRecorder dialogs = new DialogRecorder();
            RankingsViewModel model = new RankingsViewModel(new NavigationStub(),
                new RankingService(null) { Failure = new System.Data.Entity.Core.EntityException("SQL details") },
                CreateSession(), CreateImageService(), dialogs);
            Assert.That(model.Leaderboard, Is.Empty);
            Assert.That(model.CurrentPlayerRank, Is.Empty);
            Assert.That(dialogs.Last.MessageKey, Is.EqualTo("Global_msgConnectionError"));
            Assert.That(dialogs.Last.Type, Is.EqualTo(DialogType.Error));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void EditProfile_WithSelectedPicture_UsesSingleCombinedOperation(bool failSave)
        {
            // Arrange. Supply selection without opening a native file dialog.
            ProfileService service = new ProfileService();
            if (failSave)
            {
                service.UpdateFailure = UpdateProfileFailureReason.ServiceUnavailable;
            }
            PlayerProfile original = service.CurrentProfile;
            DialogRecorder dialogs = new DialogRecorder();
            PlayerSession session = CreateSession();
            EditProfileViewModel model = CreateEditor(service, dialogs, session);
            model.Username = "new-name";
            byte[] selected = { 1, 2, 3 };
            typeof(EditProfileViewModel).GetField("_selectedProfilePictureData",
                BindingFlags.Instance | BindingFlags.NonPublic).SetValue(model, selected);
            typeof(EditProfileViewModel).GetField("_selectedProfilePictureFileName",
                BindingFlags.Instance | BindingFlags.NonPublic).SetValue(model, "new.png");

            // Act.
            model.SaveCommand.Execute(null);

            // Assert. This verifies WPF routing and confirmation, not SQL persistence.
            Assert.That(service.UpdateCount, Is.EqualTo(1));
            Assert.That(service.PictureChanged, Is.True);
            Assert.That(service.CurrentProfile.Statistics, Is.SameAs(original.Statistics));
            if (failSave)
            {
                Assert.That(service.CurrentProfile, Is.SameAs(original));
                Assert.That(session.Username, Is.EqualTo("player"));
                Assert.That(dialogs.Last.MessageKey, Is.EqualTo("MessageProfile_msgUpdateProfileError"));
                Assert.That(dialogs.Last.OnAccept, Is.Null);
            }
            else
            {
                Assert.That(service.CurrentProfile.Username, Is.EqualTo("new-name"));
                Assert.That(service.CurrentProfile.Picture.OriginalFileName, Is.EqualTo("new.png"));
                Assert.That(service.CurrentProfile.Picture.ImageData, Is.EqualTo(selected));
                Assert.That(session.Username, Is.EqualTo("new-name"));
            }
        }

        [Test]
        public void SetProfilePicture_NullPicture_ThrowsArgumentNull()
        {
            // Arrange.
            ProfilePictureController controller = new ProfilePictureController(
                CreateStorage(), NullLogger<ProfilePictureController>.Instance);

            // Act.
            Assert.Throws<ArgumentNullException>(() => controller.SetProfilePicture(1, null));

            // Assert.
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void RemoveProfilePicture_InvalidId_RemainsIndependentOperation(int playerId)
        {
            // Arrange.
            ProfilePictureController controller = new ProfilePictureController(
                CreateStorage(), NullLogger<ProfilePictureController>.Instance);

            // Act.
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.RemoveProfilePicture(playerId));

            // Assert. No upload content is required to invoke deletion.
        }

        [Test]
        public void ProfileEditingContract_DoesNotAcceptStatisticsOrReadSnapshot()
        {
            // Arrange.
            Type[] forbidden = { typeof(PlayerProfile), typeof(PlayerStatistics) };
            MethodInfo[] operations = typeof(IAccountService).GetMethods()
                .Where(method => method.Name == "UpdateProfile").ToArray();

            // Act and assert.
            Assert.That(operations.Length, Is.EqualTo(2));
            Assert.That(operations.SelectMany(method => method.GetParameters())
                .Any(parameter => forbidden.Contains(parameter.ParameterType)), Is.False);
            Assert.That(operations.SelectMany(method => method.GetParameters())
                .Any(parameter => parameter.ParameterType == typeof(bool)), Is.False);
            Assert.That(typeof(PlayerStatistics).GetProperties()
                .Any(property => property.GetSetMethod() != null), Is.False);
        }

        [TestCase(true, "avatar.png", 0, UpdateProfileFailureReason.InvalidProfilePicture)]
        [TestCase(false, "avatar.png", 0, UpdateProfileFailureReason.InvalidProfilePicture)]
        [TestCase(false, "bad.ext", 1048577, UpdateProfileFailureReason.InvalidProfilePictureFormat)]
        [TestCase(false, "avatar.png", 1048577, UpdateProfileFailureReason.ProfilePictureTooLarge)]
        public void ReplacementValidation_RejectsMissingEmptyOrInvalidPicture(
            bool nullPicture, string name, int byteCount, UpdateProfileFailureReason expected)
        {
            // Arrange. Isolate validation because account lookup requires SQL Server.
            ProfilePicture picture = nullPicture ? null
                : ProfilePicture.FromSelectedContent(name, new byte[byteCount]);
            MethodInfo validate = typeof(ProfileController).GetMethod("ValidateProfilePicture",
                BindingFlags.Instance | BindingFlags.NonPublic);

            // Act.
            object reason = validate.Invoke(CreateProfileController(), new object[] { picture });

            // Assert. This does not claim a database-backed replacement was executed.
            Assert.That(reason, Is.EqualTo(expected));
        }

        private static ProfilePictureStorage CreateStorage()
        {
            return new ProfilePictureStorage(NullLogger<ProfilePictureStorage>.Instance);
        }

        private static ProfileController CreateProfileController()
        {
            return new ProfileController(CreateStorage(), NullLogger<ProfileController>.Instance);
        }

        private static ProfilePictureImageService CreateImageService()
        {
            return new ProfilePictureImageService(NullLogger<ProfilePictureImageService>.Instance);
        }

        private static PlayerSession CreateSession()
        {
            PlayerSession session = new PlayerSession();
            session.Start(9, "player", "es-MX");
            return session;
        }

        private static EditProfileViewModel CreateEditor(
            ProfileService service, DialogRecorder dialogs, PlayerSession session)
        {
            return new EditProfileViewModel(
                new NavigationStub(), service, dialogs, session, CreateImageService(),
                new ProfilePictureFileService(NullLogger<ProfilePictureFileService>.Instance));
        }

        private static byte[] CreatePng()
        {
            byte[] pixels = { 0, 0, 0, 255 };
            BitmapSource source = BitmapSource.Create(
                1, 1, 96, 96, PixelFormats.Bgra32, null, pixels, 4);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));
            using (MemoryStream output = new MemoryStream())
            {
                encoder.Save(output);
                return output.ToArray();
            }
        }

        private sealed class DialogRecorder : IDialogService
        {
            public DialogRequest Last { get; private set; }

            public void ShowDialog(DialogRequest request)
            {
                Last = request;
            }
        }

        private sealed class NavigationStub : IFrameNavigationService
        {
            public bool CanGoBack => false;
            public void NavigateTo<T>() where T : ViewModelBase { }
            public void NavigateTo<T>(object parameter) where T : ViewModelBase { }
            public void GoBackToRefresh<T>() where T : ViewModelBase { }
            public void NavigateAsRoot<T>() where T : ViewModelBase { }
            public void GoBack() { }
        }

        private sealed class RankingService : IRankingClient
        {
            private readonly DominoAllFivesService.Ranking _ranking;
            public Exception Failure { get; set; }

            public RankingService(DominoAllFivesService.Ranking ranking)
            {
                _ranking = ranking;
            }

            public DominoAllFivesService.Ranking GetTopRanking(int playerId)
            {
                if (Failure != null) throw Failure;
                return _ranking;
            }
        }

        private sealed class ProfileService : IAccountService
        {
            public byte[] OriginalImage { get; } = CreatePng();
            public PlayerProfile CurrentProfile { get; private set; }

            public ProfileService()
            {
                CurrentProfile = PlayerProfile.FromStoredProfile("player",
                    PlayerStatistics.FromStoredStatistics(4, 60, 9),
                    ProfilePicture.FromStoredContent(OriginalImage));
            }

            public UpdateProfileFailureReason? UpdateFailure { get; set; }
            public RetrieveInformationFailureReason? RetrievalFailure { get; set; }
            public int UpdateCount { get; private set; }
            public bool PictureChanged { get; private set; }

            public PlayerProfile GetProfile(int playerId)
            {
                if (RetrievalFailure.HasValue)
                {
                    if (RetrievalFailure == RetrieveInformationFailureReason.NotFound) return null;
                    throw new System.Data.Entity.Core.EntityException("Synthetic retrieval failure.");
                }
                return CurrentProfile;
            }

            public UpdateProfileFailureReason UpdateProfile(int playerId, string username)
            {
                return SaveProfile(username, false, null);
            }

            public UpdateProfileFailureReason UpdateProfile(int playerId, string username,
                ProfilePicture replacementPicture)
            {
                return SaveProfile(username, true, replacementPicture);
            }

            private UpdateProfileFailureReason SaveProfile(string username, bool replacePicture, ProfilePicture picture)
            {
                UpdateCount++;
                PictureChanged = replacePicture;
                if (UpdateFailure.HasValue)
                {
                    if (UpdateFailure == UpdateProfileFailureReason.ServiceUnavailable)
                    {
                        throw new System.Data.Entity.Core.EntityException("Synthetic save failure.");
                    }
                    return UpdateFailure.Value;
                }
                CurrentProfile = PlayerProfile.FromStoredProfile(username,
                    CurrentProfile.Statistics, replacePicture ? picture : CurrentProfile.Picture);
                return UpdateProfileFailureReason.None;
            }

            public LoginResult Login(LoginCredentials credentials) =>
                throw new NotSupportedException();
            public RegistrationResult Register(AccountRegistration registration) =>
                throw new NotSupportedException();
            public LanguagePreferenceFailureReason ChangePreferredLanguage(string languageId) =>
                throw new NotSupportedException();
            public ProfilePictureFailureReason SetProfilePicture(int playerId, ProfilePicture picture) =>
                throw new NotSupportedException();
            public ProfilePictureFailureReason RemoveProfilePicture(int playerId) => throw new NotSupportedException();
            public DeleteFailureReason DeleteAccount(int playerId, string password) =>
                throw new NotSupportedException();
            public ChangePasswordFailureReason ChangePassword(int playerId, string currentPassword, string newPassword) =>
                throw new NotSupportedException();
        }
    }
}
