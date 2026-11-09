using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel;
using System.Threading;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.ViewModels;
using DominoAllFivesService;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace DominoAllFives.Client.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class LoginViewModelTests
    {
        private CultureInfo _culture;
        private CultureInfo _uiCulture;
        private CultureInfo _defaultCulture;
        private CultureInfo _defaultUiCulture;
        private LoginClient _client;
        private DialogService _dialogs;
        private PlayerSession _session;
        private LanguageManager _languages;
        private TestLogger<LoginViewModel> _logger;
        private int _navigations;
        private int _savedPreferences;

        [SetUp]
        public void SetUp()
        {
            _culture = Thread.CurrentThread.CurrentCulture;
            _uiCulture = Thread.CurrentThread.CurrentUICulture;
            _defaultCulture = CultureInfo.DefaultThreadCurrentCulture;
            _defaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
            _logger = new TestLogger<LoginViewModel>();
            _client = new LoginClient();
            _dialogs = new DialogService();
            _session = new PlayerSession();
            _navigations = 0;
            _savedPreferences = 0;
            _languages = new LanguageManager(() => "es-MX", _ => _savedPreferences++);
        }

        [TearDown]
        public void RestoreCulture()
        {
            Thread.CurrentThread.CurrentCulture = _culture;
            Thread.CurrentThread.CurrentUICulture = _uiCulture;
            CultureInfo.DefaultThreadCurrentCulture = _defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = _defaultUiCulture;
        }

        [TestCase(AccountIssue.InvalidCredentials, "MessageAuthentication_msgInvalidCredentials")]
        [TestCase(AccountIssue.Banned, "MessageAuthentication_msgBannedAccount")]
        [TestCase(AccountIssue.Suspended, "MessageAuthentication_msgSuspendedAccount")]
        public void Login_BusinessFault_ShowsLocalizedWarning(AccountIssue issue, string messageKey)
        {
            // Arrange
            _client.Error = new FaultException<AccountIssue>(issue);
            // Act
            CreateViewModel().LoginCommand.Execute(null);
            // Assert
            AssertFailure(messageKey, DialogType.Warning);
            AssertLog(LogLevel.Warning);
            Assert.That(_logger.Messages[0], Does.Contain(issue.ToString()));
        }

        [Test]
        public void Login_Success_StartsSessionAndAppliesAccountLanguage()
        {
            _client.Account = new PlayerAccount { PlayerId = 42, Username = "player", PreferredLanguageId = "en-US" };
            CreateViewModel().LoginCommand.Execute(null);
            Assert.That(_session.PlayerId, Is.EqualTo(42));
            Assert.That(_session.Username, Is.EqualTo("player"));
            Assert.That(_session.PreferredLanguageId, Is.EqualTo("en-US"));
            Assert.That(_languages.CurrentLanguageCode, Is.EqualTo("en-US"));
            Assert.That(_savedPreferences, Is.Zero);
            Assert.That(_navigations, Is.EqualTo(1));
            Assert.That(_dialogs.Requests, Is.Empty);
            Assert.That(_client.Credentials.EmailOrUsername, Is.EqualTo("player"));
            Assert.That(_client.Credentials.Password, Is.EqualTo("password"));
        }

        [Test]
        public void Login_TechnicalFault_ShowsTechnicalMessage()
        {
            // Arrange
            _client.Error = new FaultException("Internal details must not be displayed");
            // Act
            CreateViewModel().LoginCommand.Execute(null);
            // Assert
            AssertFailure("Global_msgDefaultError", DialogType.Error);
            AssertLog(LogLevel.Error);
        }

        [Test]
        public void Login_Timeout_ShowsConnectionMessage()
        {
            // Arrange
            _client.Error = new TimeoutException();
            // Act
            CreateViewModel().LoginCommand.Execute(null);
            // Assert
            AssertFailure("MessageAuthentication_msgConnectionError", DialogType.Error);
            AssertLog(LogLevel.Error);
        }

        [Test]
        public void Login_EndpointUnavailable_ShowsConnectionMessage()
        {
            // Arrange
            _client.Error = new EndpointNotFoundException();
            // Act
            CreateViewModel().LoginCommand.Execute(null);
            // Assert
            AssertFailure("MessageAuthentication_msgConnectionError", DialogType.Error);
            AssertLog(LogLevel.Error);
        }

        [Test]
        public void Login_UnknownBusinessFault_ShowsTechnicalMessage()
        {
            _client.Error = new FaultException<AccountIssue>((AccountIssue)999);
            CreateViewModel().LoginCommand.Execute(null);
            AssertFailure("Global_msgDefaultError", DialogType.Error);
        }

        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("  ", "  ")]
        public void Login_BlankInput_DoesNotCallService(string email, string password)
        {
            LoginViewModel viewModel = CreateViewModel();
            viewModel.Email = email;
            viewModel.Password = password;
            viewModel.LoginCommand.Execute(null);
            Assert.That(viewModel.IsEmailRequiredVisible, Is.True);
            Assert.That(viewModel.IsPasswordRequiredVisible, Is.True);
            Assert.That(_client.CallCount, Is.Zero);
            Assert.That(_session.IsAuthenticated, Is.False);
            Assert.That(_navigations, Is.Zero);
            Assert.That(_dialogs.Requests, Is.Empty);
        }

        [TestCase(0, "player")]
        [TestCase(-1, "player")]
        [TestCase(42, " ")]
        public void Login_InvalidAccount_ShowsTechnicalMessage(int playerId, string username)
        {
            _client.Account = new PlayerAccount { PlayerId = playerId, Username = username };
            CreateViewModel().LoginCommand.Execute(null);
            AssertFailure("Global_msgDefaultError", DialogType.Error);
        }

        [Test]
        public void Login_NullAccount_ShowsTechnicalMessage()
        {
            CreateViewModel().LoginCommand.Execute(null);
            AssertFailure("Global_msgDefaultError", DialogType.Error);
        }

        [Test]
        public void Login_BeforeInitialization_DoesNotCallService()
        {
            var viewModel = new LoginViewModel(_session,
                () => _navigations++, () => { }, () => { }, () => { });

            Assert.Throws<InvalidOperationException>(() => viewModel.LoginCommand.Execute(null));
            Assert.That(_client.CallCount, Is.Zero);
        }

        [Test]
        public void Initialize_Twice_RejectsSecondInitialization()
        {
            LoginViewModel viewModel = CreateViewModel();

            Assert.Throws<InvalidOperationException>(() =>
                viewModel.Initialize(_dialogs, _client, _logger, _languages));
            Assert.That(_client.CallCount, Is.Zero);
        }

        private void AssertLog(LogLevel level)
        {
            Assert.That(_logger.Levels, Is.EqualTo(new[] { level }));
            Assert.That(_logger.Exceptions[0], Is.SameAs(_client.Error));
            Assert.That(_logger.Messages[0], Does.Not.Contain("password"));
        }

        private LoginViewModel CreateViewModel()
        {
            var viewModel = new LoginViewModel(_session,
                () => _navigations++, () => { }, () => { }, () => { })
            { Email = " player ", Password = "password" };
            viewModel.Initialize(_dialogs, _client, _logger, _languages);
            return viewModel;
        }

        private void AssertFailure(string messageKey, DialogType dialogType)
        {
            Assert.That(_client.CallCount, Is.EqualTo(1));
            Assert.That(_session.IsAuthenticated, Is.False);
            Assert.That(_navigations, Is.Zero);
            Assert.That(_languages.CurrentLanguageCode, Is.EqualTo("es-MX"));
            Assert.That(_savedPreferences, Is.Zero);
            Assert.That(_dialogs.Requests.Count, Is.EqualTo(1));
            Assert.That(_dialogs.Requests[0].MessageKey, Is.EqualTo(messageKey));
            Assert.That(_dialogs.Requests[0].Type, Is.EqualTo(dialogType));
        }

        private sealed class LoginClient : ILoginClient
        {
            public Exception Error { get; set; }
            public PlayerAccount Account { get; set; }
            public Credentials Credentials { get; private set; }
            public int CallCount { get; private set; }
            public PlayerAccount LoginWithAccount(Credentials credentials)
            {
                CallCount++;
                Credentials = credentials;
                if (Error != null) throw Error;
                return Account;
            }
        }

        private sealed class DialogService : IDialogService
        {
            public List<DialogRequest> Requests { get; } = new List<DialogRequest>();
            public void ShowDialog(DialogRequest request) => Requests.Add(request);
        }
    }
}
