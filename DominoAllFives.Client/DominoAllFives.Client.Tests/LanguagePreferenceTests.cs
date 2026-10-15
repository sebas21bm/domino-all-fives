using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core;
using System.Globalization;
using System.Threading;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Local;
using DominoAllFives.Client.WPF.ViewModels;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Exceptions;
using DominoAllFives.Contracts.Services;
using DominoAllFives.DataAccess.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace DominoAllFives.Client.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    public class LanguagePreferenceTests
    {
        private CultureInfo _culture;
        private CultureInfo _uiCulture;
        private CultureInfo _defaultCulture;
        private CultureInfo _defaultUiCulture;

        [SetUp]
        public void SaveCulture()
        {
            _culture = Thread.CurrentThread.CurrentCulture;
            _uiCulture = Thread.CurrentThread.CurrentUICulture;
            _defaultCulture = CultureInfo.DefaultThreadCurrentCulture;
            _defaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
        }

        [TearDown]
        public void RestoreCulture()
        {
            Thread.CurrentThread.CurrentCulture = _culture;
            Thread.CurrentThread.CurrentUICulture = _uiCulture;
            CultureInfo.DefaultThreadCurrentCulture = _defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = _defaultUiCulture;
        }

        [TestCase("es-MX")]
        [TestCase("en-US")]
        [TestCase("pt-BR")]
        public void Login_ConfirmedLanguage_AppliesWithoutChangingDevice(string language)
        {
            // Arrange.
            DevicePreference device = new DevicePreference("es-MX");
            LanguageManager manager = device.CreateManager();
            PlayerSession session = new PlayerSession();
            AccountServiceStub service = new AccountServiceStub();
            service.Account = CreateAccount(11, language);
            bool navigated = false;
            LoginViewModel model = new LoginViewModel(
                new DialogRecorder(), service, session, NullLogger<LoginViewModel>.Instance,
                () => navigated = true, () => { }, () => { }, () => { }, manager);
            model.Email = "player";
            model.Password = "test-password";

            // Act.
            model.LoginCommand.Execute(null);

            // Assert.
            Assert.That(session.PlayerId, Is.EqualTo(11));
            Assert.That(session.PreferredLanguageId, Is.EqualTo(language));
            Assert.That(manager.CurrentLanguageCode, Is.EqualTo(language));
            Assert.That(navigated, Is.True);
            Assert.That(device.Value, Is.EqualTo("es-MX"));
            Assert.That(device.SaveCount, Is.Zero);
        }

        [TestCase("es-MX")]
        [TestCase("en-US")]
        [TestCase("pt-BR")]
        public void ChangeLanguage_RecreatedSession_RetainsStoredPreference(string language)
        {
            // Arrange.
            StoreData database = new StoreData();
            PlayerSession session = new PlayerSession();
            session.Start(11, "player", "es-MX");
            LocalAccountService service = CreateLocalService(session, database);

            // Act.
            service.ChangePreferredLanguage(language);
            session.Clear();
            PlayerSession restartedSession = new PlayerSession();
            restartedSession.Start(11, "player", database.Languages[11]);
            LocalAccountService restartedService = CreateLocalService(restartedSession, database);

            // Assert.
            Assert.That(database.Languages[11], Is.EqualTo(language));
            Assert.That(restartedSession.PreferredLanguageId, Is.EqualTo(language));
            Assert.That(database.SaveCount, Is.EqualTo(1));
            Assert.That(restartedService, Is.Not.Null);
        }

        [Test]
        public void SaveLanguage_AccountAndLogout_RestoresDeviceAfterCommit()
        {
            // Arrange.
            StoreData database = new StoreData();
            PlayerSession session = new PlayerSession();
            session.Start(11, "player", "es-MX");
            DevicePreference device = new DevicePreference("pt-BR");
            LanguageManager manager = device.CreateManager();
            manager.ApplyAccountLanguage("es-MX");
            database.BeforeSave = () =>
            {
                Assert.That(manager.CurrentLanguageCode, Is.EqualTo("es-MX"));
                Assert.That(session.PreferredLanguageId, Is.EqualTo("es-MX"));
            };
            GameSettingsViewModel model = CreateSettings(
                session, CreateLocalService(session, database), manager, new DialogRecorder());
            model.SelectedLanguageCode = "en-US";

            // Act.
            model.SaveLanguageCommand.Execute(null);

            // Assert.
            Assert.That(database.Languages[11], Is.EqualTo("en-US"));
            Assert.That(session.PreferredLanguageId, Is.EqualTo("en-US"));
            Assert.That(manager.CurrentLanguageCode, Is.EqualTo("en-US"));
            Assert.That(device.Value, Is.EqualTo("pt-BR"));
            model.LogoutCommand.Execute(null);
            Assert.That(session.IsAuthenticated, Is.False);
            Assert.That(manager.CurrentLanguageCode, Is.EqualTo("pt-BR"));
            Assert.That(device.SaveCount, Is.Zero);
        }

        [Test]
        public void ChangeLanguage_AlternatingAccounts_KeepsPreferencesSeparate()
        {
            // Arrange.
            StoreData database = new StoreData();
            database.Languages[22] = "pt-BR";
            PlayerSession session = new PlayerSession();
            DevicePreference device = new DevicePreference("es-MX");
            LanguageManager manager = device.CreateManager();
            LocalAccountService service = CreateLocalService(session, database);
            // Act.
            session.Start(11, "first", "es-MX");
            service.ChangePreferredLanguage("en-US");
            session.Clear();
            session.Start(22, "second", database.Languages[22]);
            manager.ApplyAccountLanguage(session.PreferredLanguageId);
            service.ChangePreferredLanguage("pt-BR");

            // Assert.
            Assert.That(database.Languages[11], Is.EqualTo("en-US"));
            Assert.That(database.Languages[22], Is.EqualTo("pt-BR"));
            Assert.That(device.Value, Is.EqualTo("es-MX"));
        }

        [Test]
        public void SaveLanguage_GuestAndManagerRestart_PersistsOnlyDevice()
        {
            // Arrange.
            StoreData database = new StoreData();
            PlayerSession session = new PlayerSession();
            DevicePreference device = new DevicePreference("es-MX");
            LanguageManager manager = device.CreateManager();
            GameSettingsViewModel model = CreateSettings(
                session, CreateLocalService(session, database), manager, new DialogRecorder());
            model.SelectedLanguageCode = "pt-BR";

            // Act.
            model.SaveLanguageCommand.Execute(null);
            LanguageManager restarted = device.CreateManager();

            // Assert.
            Assert.That(restarted.CurrentLanguageCode, Is.EqualTo("pt-BR"));
            Assert.That(database.FactoryCount, Is.Zero);
            Assert.That(database.Languages[11], Is.EqualTo("es-MX"));
            Assert.That(device.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void SaveLanguage_PersistenceFailure_LeavesConfirmedStateUnchanged()
        {
            // Arrange.
            StoreData database = new StoreData();
            database.FailSave = true;
            PlayerSession session = new PlayerSession();
            session.Start(11, "player", "es-MX");
            DevicePreference device = new DevicePreference("pt-BR");
            LanguageManager manager = device.CreateManager();
            manager.ApplyAccountLanguage("es-MX");
            DialogRecorder dialogs = new DialogRecorder();
            GameSettingsViewModel model = CreateSettings(
                session, CreateLocalService(session, database), manager, dialogs);
            model.SelectedLanguageCode = "en-US";

            // Act.
            model.SaveLanguageCommand.Execute(null);

            // Assert.
            Assert.That(database.Languages[11], Is.EqualTo("es-MX"));
            Assert.That(session.PreferredLanguageId, Is.EqualTo("es-MX"));
            Assert.That(manager.CurrentLanguageCode, Is.EqualTo("es-MX"));
            Assert.That(device.Value, Is.EqualTo("pt-BR"));
            Assert.That(dialogs.Last, Is.Not.Null);
            Assert.That(dialogs.Last.Type, Is.EqualTo(DialogType.Error));
        }

        [Test]
        public void ChangeLanguage_LoggedOut_RejectsBeforeOpeningStore()
        {
            // Arrange.
            StoreData database = new StoreData();
            PlayerSession session = new PlayerSession();
            session.Start(11, "player", "es-MX");
            LocalAccountService service = CreateLocalService(session, database);
            session.Clear();

            // Act.
            LanguagePreferenceException failure = Assert.Throws<LanguagePreferenceException>(
                () => service.ChangePreferredLanguage("en-US"));

            // Assert.
            Assert.That(failure.Reason, Is.EqualTo(LanguagePreferenceFailureReason.NotSignedIn));
            Assert.That(database.FactoryCount, Is.Zero);
            Assert.That(database.Languages[11], Is.EqualTo("es-MX"));
        }

        [TestCase("fr-FR")]
        [TestCase("")]
        [TestCase(null)]
        public void ChangeLanguage_UnsupportedCode_RejectsBeforeOpeningStore(string language)
        {
            // Arrange.
            StoreData database = new StoreData();
            AccountLanguageController controller = CreateController(database);

            // Act.
            LanguagePreferenceException failure = Assert.Throws<LanguagePreferenceException>(
                () => controller.ChangePreferredLanguage(11, language));

            // Assert.
            Assert.That(failure.Reason, Is.EqualTo(LanguagePreferenceFailureReason.InvalidLanguage));
            Assert.That(database.FactoryCount, Is.Zero);
        }

        [Test]
        public void ChangeLanguage_MissingCatalogCode_DoesNotSave()
        {
            // Arrange.
            StoreData database = new StoreData();
            database.Catalog.Remove("en-US");
            AccountLanguageController controller = CreateController(database);

            // Act.
            LanguagePreferenceException failure = Assert.Throws<LanguagePreferenceException>(
                () => controller.ChangePreferredLanguage(11, "en-US"));

            // Assert.
            Assert.That(failure.Reason, Is.EqualTo(LanguagePreferenceFailureReason.InvalidLanguage));
            Assert.That(database.SaveCount, Is.Zero);
            Assert.That(database.Languages[11], Is.EqualTo("es-MX"));
        }

        [Test]
        public void ChangeLanguage_MissingAccount_DoesNotSave()
        {
            // Arrange.
            StoreData database = new StoreData();
            database.Languages.Remove(11);

            // Act.
            LanguagePreferenceException failure = Assert.Throws<LanguagePreferenceException>(
                () => CreateController(database).ChangePreferredLanguage(11, "en-US"));

            // Assert.
            Assert.That(failure.Reason, Is.EqualTo(LanguagePreferenceFailureReason.AccountNotFound));
            Assert.That(database.SaveCount, Is.Zero);
        }

        [Test]
        public void ChangeLanguage_SessionChangesDuringCommit_DoesNotUpdateNewSession()
        {
            // Arrange.
            StoreData database = new StoreData();
            PlayerSession session = new PlayerSession();
            session.Start(11, "first", "es-MX");
            database.BeforeSave = () => session.Start(22, "second", "pt-BR");
            LocalAccountService service = CreateLocalService(session, database);

            // Act.
            Assert.Throws<LanguagePreferenceException>(
                () => service.ChangePreferredLanguage("en-US"));

            // Assert.
            Assert.That(session.PlayerId, Is.EqualTo(22));
            Assert.That(session.PreferredLanguageId, Is.EqualTo("pt-BR"));
            Assert.That(database.Languages[11], Is.EqualTo("en-US"));
        }

        [TestCase("fr-FR")]
        [TestCase("")]
        public void Login_UnsupportedStoredPreference_UsesFallbackWithoutRewriting(string language)
        {
            // Arrange: a persisted value unsupported by the current client.
            DevicePreference device = new DevicePreference("pt-BR");
            LanguageManager manager = device.CreateManager();
            PlayerSession session = new PlayerSession();
            AccountServiceStub service = new AccountServiceStub();
            service.Account = CreateAccount(11, language);
            LoginViewModel model = new LoginViewModel(
                new DialogRecorder(), service, session, NullLogger<LoginViewModel>.Instance,
                () => { }, () => { }, () => { }, () => { }, manager);
            model.Email = "player";
            model.Password = "test-password";

            // Act.
            model.LoginCommand.Execute(null);

            // Assert: fallback is presentation only.
            Assert.That(session.IsAuthenticated, Is.True);
            Assert.That(session.PreferredLanguageId, Is.EqualTo(language));
            Assert.That(manager.CurrentLanguageCode, Is.EqualTo("es-MX"));
            Assert.That(device.Value, Is.EqualTo("pt-BR"));
            Assert.That(device.SaveCount, Is.Zero);
        }

        [Test]
        public void ChangeLanguage_DeviceSaveFails_DoesNotApplyCulture()
        {
            // Arrange.
            LanguageManager manager = new LanguageManager(
                () => "es-MX",
                language => { throw new ConfigurationErrorsException("Synthetic failure."); });

            // Act and assert.
            Assert.Throws<ConfigurationErrorsException>(() => manager.ChangeLanguage("en-US"));
            Assert.That(manager.CurrentLanguageCode, Is.EqualTo("es-MX"));
            Assert.That(Thread.CurrentThread.CurrentUICulture.Name, Is.EqualTo("es-MX"));
        }

        private static PlayerAccount CreateAccount(int playerId, string language)
        {
            PlayerAccount account = PlayerAccount.FromStoredAccount(
                playerId, "player", false, false, null);
            account.SetPreferredLanguage(language);
            return account;
        }

        private static AccountLanguageController CreateController(StoreData database)
        {
            return new AccountLanguageController(
                () => database.Open(), NullLogger<AccountLanguageController>.Instance);
        }

        private static LocalAccountService CreateLocalService(
            PlayerSession session, StoreData database)
        {
            ProfilePictureStorage storage = new ProfilePictureStorage(
                NullLogger<ProfilePictureStorage>.Instance);
            return new LocalAccountService(
                new AuthenticationController(NullLogger<AuthenticationController>.Instance),
                new RegistrationController(NullLogger<RegistrationController>.Instance),
                new ProfilePictureController(storage, NullLogger<ProfilePictureController>.Instance),
                new ProfileController(storage, NullLogger<ProfileController>.Instance),
                new AccountController(NullLogger<AccountController>.Instance),
                CreateController(database), session);
        }

        private static GameSettingsViewModel CreateSettings(
            PlayerSession session, IAccountService service,
            LanguageManager manager, DialogRecorder dialogs)
        {
            return new GameSettingsViewModel(
                new NavigationStub(), dialogs, service,
                NullLoggerFactory.Instance, session, manager);
        }

        private sealed class DevicePreference
        {
            public string Value { get; private set; }
            public int SaveCount { get; private set; }

            public DevicePreference(string language)
            {
                Value = language;
            }

            public LanguageManager CreateManager()
            {
                return new LanguageManager(() => Value, language =>
                {
                    Value = language;
                    SaveCount++;
                });
            }
        }

        private sealed class StoreData
        {
            public Dictionary<int, string> Languages { get; } =
                new Dictionary<int, string> { { 11, "es-MX" } };
            public HashSet<string> Catalog { get; } =
                new HashSet<string> { "es-MX", "en-US", "pt-BR" };
            public bool FailSave { get; set; }
            public int SaveCount { get; set; }
            public int FactoryCount { get; private set; }
            public Action BeforeSave { get; set; }

            public ILanguagePreferenceStore Open()
            {
                FactoryCount++;
                return new StoreStub(this);
            }
        }

        private sealed class StoreStub : ILanguagePreferenceStore
        {
            private readonly StoreData _database;
            private int _pendingPlayerId;
            private string _pendingLanguage;

            public StoreStub(StoreData database)
            {
                _database = database;
            }

            public bool LanguageExists(string languageId)
            {
                return _database.Catalog.Contains(languageId);
            }

            public bool UpdatePreferredLanguage(int playerId, string languageId)
            {
                if (!_database.Languages.ContainsKey(playerId))
                {
                    return false;
                }
                _pendingPlayerId = playerId;
                _pendingLanguage = languageId;
                return true;
            }

            public void SaveChanges()
            {
                _database.BeforeSave?.Invoke();
                if (_database.FailSave)
                {
                    throw new EntityException("Synthetic persistence failure.");
                }
                _database.Languages[_pendingPlayerId] = _pendingLanguage;
                _database.SaveCount++;
            }

            public void Dispose()
            {
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

        private sealed class AccountServiceStub : IAccountService
        {
            public PlayerAccount Account { get; set; }
            public PlayerAccount Login(PlayerAccount account) => Account;
            public void ChangePreferredLanguage(string languageId) =>
                throw new NotSupportedException();
            public RegistrationResultDto Register(RegisterAccountDto data) =>
                throw new NotSupportedException();
            public ProfilePictureResultDto SetProfilePicture(ProfilePictureDto picture) =>
                throw new NotSupportedException();
            public ProfilePictureResultDto RemoveProfilePicture(int playerId) =>
                throw new NotSupportedException();
            public ProfileDto GetProfile(int playerId) => throw new NotSupportedException();
            public UpdateProfileResultDto UpdateProfile(UpdateProfileDto data) =>
                throw new NotSupportedException();
            public DeleteAccountResultDto DeleteAccount(DeleteAccountRequestDto request) =>
                throw new NotSupportedException();
            public ChangePasswordResultDto ChangePassword(ChangePasswordRequestDto request) =>
                throw new NotSupportedException();
        }
    }
}
