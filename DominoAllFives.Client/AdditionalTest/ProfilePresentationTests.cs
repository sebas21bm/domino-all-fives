using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.ProfilePicture;
using DominoAllFives.Client.WPF.Services.Wcf.Services;
using DominoAllFives.Client.WPF.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace DominoAllFives.Client.AdditionalTest
{
    [TestFixture]
    [NonParallelizable]
    [Apartment(ApartmentState.STA)]
    public sealed class ProfilePresentationTests
    {
        private const int PlayerId = 42;
        private const string Username = "player";
        private const string Language = "es-MX";
        private const string SessionKey = "local-presentation-session";
        private Application _createdApplication;
        private PlayerSession _session;
        private MainWindowViewModel _window;
        private FrameNavigationService _navigation;
        private ProfilePictureImageService _imageService;
        private ProfilePictureFileService _fileService;
        private ProfileClient _client;
        private int _finished;
        private int _cancelled;

        [OneTimeSetUp]
        public void StartWpfResources()
        {
            Application.ResourceAssembly = typeof(WPF.App).Assembly;
            if (Application.Current == null)
            {
                _createdApplication = new Application
                {
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };
            }
        }

        [OneTimeTearDown]
        public void StopWpfResources()
        {
            if (_createdApplication != null)
            {
                _createdApplication.Shutdown();
            }
        }

        [SetUp]
        public void SetUp()
        {
            _session = new PlayerSession();
            _window = new MainWindowViewModel();
            _navigation = new FrameNavigationService(new Frame());
            _imageService = new ProfilePictureImageService(
                NullLogger<ProfilePictureImageService>.Instance);
            _fileService = new ProfilePictureFileService(
                NullLogger<ProfilePictureFileService>.Instance);
            _client = new ProfileClient();
            _finished = 0;
            _cancelled = 0;
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Profile_WithoutServerSession_ShowsNotFoundAndDisablesProfileActions(
            bool hasLocalIdentity)
        {
            // Arrange
            StartLocalIdentityIfRequested(hasLocalIdentity);
            ProfileViewModel viewModel = new ProfileViewModel(_session);
            viewModel.ConfigureLogger(NullLogger<ProfileViewModel>.Instance);
            // Act
            viewModel.Initialize(
                _navigation,
                _client,
                _imageService,
                _window.DialogService);
            // Assert
            Assert.That(
                new object[]
                {
                    _window.IsDialogVisible,
                    _window.CurrentDialogViewModel.Message,
                    viewModel.GoToEditProfileCommand.CanExecute(null),
                    viewModel.GoToMatchHistoryCommand.CanExecute(null),
                    viewModel.GoBackCommand.CanExecute(null),
                    _session.HasServerSession
                },
                Is.EqualTo(new object[]
                {
                    true,
                    LanguageManager.Instance["Global_msgNotFound"],
                    false,
                    false,
                    true,
                    false
                }));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void EditProfile_WithoutServerSession_ShowsNotFoundAndDisablesChanges(
            bool hasLocalIdentity)
        {
            // Arrange
            StartLocalIdentityIfRequested(hasLocalIdentity);
            EditProfileViewModel viewModel = new EditProfileViewModel(_session);
            viewModel.ConfigurePictures(_imageService, _fileService);
            // Act
            viewModel.Initialize(
                _navigation,
                _client,
                _window.DialogService,
                NullLogger<EditProfileViewModel>.Instance);
            // Assert
            Assert.That(
                new object[]
                {
                    _window.IsDialogVisible,
                    _window.CurrentDialogViewModel.Message,
                    viewModel.ChangePictureCommand.CanExecute(null),
                    viewModel.RemovePictureCommand.CanExecute(null),
                    viewModel.SaveCommand.CanExecute(null),
                    viewModel.DiscardCommand.CanExecute(null),
                    _session.HasServerSession
                },
                Is.EqualTo(new object[]
                {
                    true,
                    LanguageManager.Instance["Global_msgNotFound"],
                    false,
                    false,
                    false,
                    true,
                    false
                }));
        }

        [Test]
        public void AddPhoto_WithoutSelection_ShowsInvalidImageAndPreservesSession()
        {
            // Arrange
            StartServerSession();
            UploadProfilePictureViewModel viewModel = CreateUpload();
            // Act
            viewModel.AddPhotoCommand.Execute(null);
            // Assert
            Assert.That(
                new object[]
                {
                    _window.CurrentDialogViewModel.Message,
                    _session.PlayerId,
                    _session.Username,
                    _session.PreferredLanguageId,
                    _session.SessionKey,
                    _finished,
                    _cancelled,
                    viewModel.AddPhotoCommand.CanExecute(null)
                },
                Is.EqualTo(new object[]
                {
                    LanguageManager.Instance["MessageProfile_msgPhotoInvalid"],
                    PlayerId,
                    Username,
                    Language,
                    SessionKey,
                    0,
                    0,
                    true
                }));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SkipOrCancel_IdleUpload_InvokesCallbackAndPreservesCreatedAccount(bool cancel)
        {
            // Arrange
            StartServerSession();
            UploadProfilePictureViewModel viewModel = CreateUpload();
            int expectedFinished = 1;
            int expectedCancelled = 0;
            // Act
            if (cancel)
            {
                expectedFinished = 0;
                expectedCancelled = 1;
                viewModel.CancelCommand.Execute(null);
            }
            else
            {
                viewModel.SkipPhotoCommand.Execute(null);
            }

            // Assert
            Assert.That(
                new object[]
                {
                    _finished,
                    _cancelled,
                    _session.PlayerId,
                    _session.Username,
                    _session.PreferredLanguageId,
                    _session.SessionKey,
                    _window.IsDialogVisible
                },
                Is.EqualTo(new object[]
                {
                    expectedFinished,
                    expectedCancelled,
                    PlayerId,
                    Username,
                    Language,
                    SessionKey,
                    false
                }));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void GetProfilePicture_AbsentContent_ReturnsFrozenDefaultImage(bool empty)
        {
            // Arrange
            byte[] bytes = null;
            if (empty)
            {
                bytes = Array.Empty<byte>();
            }

            // Act
            BitmapImage image = _imageService.GetProfilePicture(bytes);
            // Assert
            AssertDefaultImage(image);
        }

        [Test]
        public void GetProfilePicture_CorruptedContent_ReturnsFrozenDefaultImage()
        {
            // Arrange
            byte[] bytes = { 0, 1, 2, 3 };
            // Act
            BitmapImage image = _imageService.GetProfilePicture(bytes);
            // Assert
            AssertDefaultImage(image);
        }

        [Test]
        public void Constructor_ProfileWithActiveSession_DoesNotLoadAndDisablesCommands()
        {
            // Arrange
            StartServerSession();
            // Act
            ProfileViewModel viewModel = new ProfileViewModel(_session);
            // Assert
            Assert.That(
                new object[]
                {
                    _window.IsDialogVisible,
                    viewModel.GoBackCommand.CanExecute(null),
                    viewModel.GoToEditProfileCommand.CanExecute(null),
                    viewModel.GoToMatchHistoryCommand.CanExecute(null),
                    _session.SessionKey
                },
                Is.EqualTo(new object[] { false, false, false, false, SessionKey }));
        }

        [Test]
        public void Constructor_EditWithActiveSession_DoesNotLoadAndDisablesCommands()
        {
            // Arrange
            StartServerSession();
            // Act
            EditProfileViewModel viewModel = new EditProfileViewModel(_session);
            // Assert
            Assert.That(
                new object[]
                {
                    _window.IsDialogVisible,
                    viewModel.GoBackCommand.CanExecute(null),
                    viewModel.ChangePictureCommand.CanExecute(null),
                    viewModel.RemovePictureCommand.CanExecute(null),
                    viewModel.SaveCommand.CanExecute(null),
                    viewModel.DiscardCommand.CanExecute(null),
                    _session.SessionKey
                },
                Is.EqualTo(new object[] { false, false, false, false, false, false, SessionKey }));
        }

        [Test]
        public void Constructor_UploadWithActiveSession_DoesNotLoadAndDisablesCommands()
        {
            // Arrange
            StartServerSession();
            // Act
            UploadProfilePictureViewModel viewModel = new UploadProfilePictureViewModel(
                _session,
                OnFinished,
                OnCancelled);
            // Assert
            Assert.That(
                new object[]
                {
                    _window.IsDialogVisible,
                    viewModel.SelectPhotoCommand.CanExecute(null),
                    viewModel.AddPhotoCommand.CanExecute(null),
                    viewModel.SkipPhotoCommand.CanExecute(null),
                    viewModel.CancelCommand.CanExecute(null),
                    _finished,
                    _cancelled,
                    _session.SessionKey
                },
                Is.EqualTo(new object[]
                {
                    false, false, false, false, false, 0, 0, SessionKey
                }));
        }

        [Test]
        public void Initialize_ProfileWithoutLogger_ThrowsBeforeLoading()
        {
            // Arrange
            ProfileViewModel viewModel = new ProfileViewModel(_session);
            // Act
            TestDelegate initialize = InitializeProfile;
            // Assert
            Assert.That(initialize, Throws.TypeOf<InvalidOperationException>());

            void InitializeProfile()
            {
                viewModel.Initialize(_navigation, _client, _imageService, _window.DialogService);
            }
        }

        [Test]
        public void Initialize_EditWithoutPictures_ThrowsBeforeLoading()
        {
            // Arrange
            EditProfileViewModel viewModel = new EditProfileViewModel(_session);
            // Act
            TestDelegate initialize = InitializeEditor;
            // Assert
            Assert.That(initialize, Throws.TypeOf<InvalidOperationException>());

            void InitializeEditor()
            {
                viewModel.Initialize(
                    _navigation,
                    _client,
                    _window.DialogService,
                    NullLogger<EditProfileViewModel>.Instance);
            }
        }

        [Test]
        public void ConfigureLogger_NullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            ProfileViewModel viewModel = new ProfileViewModel(_session);
            // Act
            TestDelegate configure = ConfigureLogger;
            // Assert
            Assert.That(configure, Throws.TypeOf<ArgumentNullException>());

            void ConfigureLogger()
            {
                viewModel.ConfigureLogger(null);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ConfigurePictures_MissingDependency_ThrowsArgumentNullException(
            bool missingImage)
        {
            // Arrange
            EditProfileViewModel viewModel = new EditProfileViewModel(_session);
            ProfilePictureImageService imageService = _imageService;
            ProfilePictureFileService fileService = null;
            if (missingImage)
            {
                imageService = null;
                fileService = _fileService;
            }

            // Act
            TestDelegate configure = ConfigurePictures;
            // Assert
            Assert.That(configure, Throws.TypeOf<ArgumentNullException>());

            void ConfigurePictures()
            {
                viewModel.ConfigurePictures(imageService, fileService);
            }
        }

        [Test]
        public void Initialize_ProfileTwice_RejectsSecondInitialization()
        {
            // Arrange
            ProfileViewModel viewModel = new ProfileViewModel(_session);
            viewModel.ConfigureLogger(NullLogger<ProfileViewModel>.Instance);
            viewModel.Initialize(_navigation, _client, _imageService, _window.DialogService);
            // Act
            TestDelegate initialize = InitializeProfile;
            // Assert
            Assert.That(initialize, Throws.TypeOf<InvalidOperationException>());

            void InitializeProfile()
            {
                viewModel.Initialize(_navigation, _client, _imageService, _window.DialogService);
            }
        }

        [Test]
        public void Initialize_EditTwice_RejectsSecondInitialization()
        {
            // Arrange
            EditProfileViewModel viewModel = new EditProfileViewModel(_session);
            viewModel.ConfigurePictures(_imageService, _fileService);
            viewModel.Initialize(
                _navigation,
                _client,
                _window.DialogService,
                NullLogger<EditProfileViewModel>.Instance);
            // Act
            TestDelegate initialize = InitializeEditor;
            // Assert
            Assert.That(initialize, Throws.TypeOf<InvalidOperationException>());

            void InitializeEditor()
            {
                viewModel.Initialize(
                    _navigation,
                    _client,
                    _window.DialogService,
                    NullLogger<EditProfileViewModel>.Instance);
            }
        }

        [Test]
        public void Initialize_UploadTwice_RejectsSecondInitialization()
        {
            // Arrange
            UploadProfilePictureViewModel viewModel = CreateUpload();
            // Act
            TestDelegate initialize = InitializeUpload;
            // Assert
            Assert.That(initialize, Throws.TypeOf<InvalidOperationException>());

            void InitializeUpload()
            {
                viewModel.Initialize(
                    _window.DialogService,
                    _client,
                    _fileService,
                    NullLogger<UploadProfilePictureViewModel>.Instance);
            }
        }

        private void StartLocalIdentityIfRequested(bool requested)
        {
            if (requested)
            {
                _session.Start(PlayerId, Username, Language);
            }
        }

        private void StartServerSession()
        {
            _session.Start(PlayerId, Username, Language, SessionKey);
        }

        private UploadProfilePictureViewModel CreateUpload()
        {
            UploadProfilePictureViewModel viewModel = new UploadProfilePictureViewModel(
                _session,
                OnFinished,
                OnCancelled);
            viewModel.Initialize(
                _window.DialogService,
                _client,
                _fileService,
                NullLogger<UploadProfilePictureViewModel>.Instance);
            return viewModel;
        }

        private void OnFinished()
        {
            _finished++;
        }

        private void OnCancelled()
        {
            _cancelled++;
        }

        private static void AssertDefaultImage(BitmapImage image)
        {
            Assert.That(
                new object[]
                {
                    image.UriSource?.AbsoluteUri,
                    image.IsFrozen,
                    image.PixelWidth > 0,
                    image.PixelHeight > 0
                },
                Is.EqualTo(new object[]
                {
                    "pack://application:,,,/Assets/Images/ProfilePictures/defaultProfilePic.png",
                    true,
                    true,
                    true
                }));
        }
    }
}
