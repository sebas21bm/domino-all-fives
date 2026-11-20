using System.Threading;
using System.Windows.Controls;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services.Wcf.Services;
using DominoAllFives.Client.WPF.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace DominoAllFives.Client.Tests
{
    [TestFixture]
    [NonParallelizable]
    [Apartment(ApartmentState.STA)]
    public sealed class RegistrationPresentationTests
    {
        private PlayerSession _session;
        private MainWindowViewModel _window;
        private int _successes;
        private int _cancellations;

        [SetUp]
        public void SetUp()
        {
            _session = new PlayerSession();
            _window = new MainWindowViewModel();
            _successes = 0;
            _cancellations = 0;
        }

        [Test]
        public void Register_EmptyFields_ShowsRequiredIndicatorsWithoutStartingSession()
        {
            // Arrange
            RegisterAccountViewModel viewModel = CreateViewModel();
            PasswordBox[] passwords = { new PasswordBox(), new PasswordBox() };
            // Act
            viewModel.RegisterCommand.Execute(passwords);
            // Assert
            Assert.That(viewModel.IsUsernameRequiredVisible, Is.True);
            Assert.That(viewModel.IsEmailRequiredVisible, Is.True);
            Assert.That(viewModel.IsPasswordRequiredVisible, Is.True);
            Assert.That(viewModel.IsConfirmPasswordRequiredVisible, Is.True);
            Assert.That(viewModel.RegisterCommand.CanExecute(passwords), Is.True);
            Assert.That(_session.IsAuthenticated, Is.False);
            Assert.That(_window.IsDialogVisible, Is.False);
            Assert.That(_successes, Is.Zero);
        }

        [Test]
        public void Register_DifferentPasswords_ShowsLocalizedWarningWithoutStartingSession()
        {
            // Arrange
            RegisterAccountViewModel viewModel = CreateViewModel();
            viewModel.Username = "player";
            viewModel.Email = "player@gmail.com";
            PasswordBox[] passwords =
            {
                new PasswordBox { Password = "StrongPass1!" },
                new PasswordBox { Password = "Different1!" }
            };
            // Act
            viewModel.RegisterCommand.Execute(passwords);
            // Assert
            Assert.That(_window.IsDialogVisible, Is.True);
            Assert.That(_window.CurrentDialogViewModel.Message,
                Is.EqualTo(LanguageManager.Instance["MessageAccount_msgIncorrectPassword"]));
            Assert.That(viewModel.RegisterCommand.CanExecute(passwords), Is.True);
            Assert.That(_session.IsAuthenticated, Is.False);
            Assert.That(_successes, Is.Zero);
        }

        [Test]
        public void Cancel_IdleRegistration_InvokesNavigationWithoutChangingSession()
        {
            // Arrange
            RegisterAccountViewModel viewModel = CreateViewModel();
            // Act
            viewModel.CancelCommand.Execute(null);
            // Assert
            Assert.That(_cancellations, Is.EqualTo(1));
            Assert.That(_successes, Is.Zero);
            Assert.That(_session.IsAuthenticated, Is.False);
        }

        private RegisterAccountViewModel CreateViewModel()
        {
            RegisterAccountViewModel viewModel = new RegisterAccountViewModel(
                _session,
                OnSuccess,
                OnCancel);
            viewModel.Initialize(
                _window.DialogService,
                new RegistrationClient(),
                NullLogger<RegisterAccountViewModel>.Instance);
            return viewModel;
        }

        private void OnSuccess()
        {
            _successes++;
        }

        private void OnCancel()
        {
            _cancellations++;
        }
    }
}
