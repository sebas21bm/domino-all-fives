using System;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DomainProfilePicture = DominoAllFives.Contracts.Domain.ProfilePicture;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Services;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>
    /// Manages the profile picture upload process during account registration
    /// </summary>
    public class UploadProfilePictureViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IAccountService _accountService;
        private readonly PlayerSession _playerSession;
        private readonly Action _onFinishRegistration;
        private readonly Action _onCancel;

        private string _selectedFilePath;
        private BitmapImage _profilePicturePreview;

        public BitmapImage ProfilePicturePreview
        {
            get => _profilePicturePreview;
            private set => SetProperty(
                ref _profilePicturePreview,
                value);
        }

        /// <summary>
        /// Gets the command used to select a profile picture.
        /// </summary>
        public RelayCommand SelectPhotoCommand { get; }

        /// <summary>
        /// Gets the command used to add the selected profile picture.
        /// </summary>
        public RelayCommand AddPhotoCommand { get; }

        /// <summary>
        /// Gets the command used to skip profile picture selection.
        /// </summary>
        public RelayCommand SkipPhotoCommand { get; }

        /// <summary>
        /// Gets the command used to cancel profile picture selection.
        /// </summary>
        public RelayCommand CancelCommand { get; }

        /// <summary>
        /// Initializes a new instance of the profile picture view model.
        /// </summary>
        public UploadProfilePictureViewModel(
            IDialogService dialogService,
            IAccountService accountService,
            PlayerSession playerSession,
            Action onFinishRegistration,
            Action onCancel)
        {
            _dialogService = dialogService
                ?? throw new ArgumentNullException(nameof(dialogService));

            _accountService = accountService
                ?? throw new ArgumentNullException(nameof(accountService));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));

            _onFinishRegistration = onFinishRegistration
                ?? throw new ArgumentNullException(nameof(onFinishRegistration));

            _onCancel = onCancel
                ?? throw new ArgumentNullException(nameof(onCancel));

            SelectPhotoCommand = new RelayCommand(SelectPhoto);

            AddPhotoCommand = new RelayCommand(AddPhoto);

            SkipPhotoCommand = new RelayCommand(SkipPhoto);

            CancelCommand = new RelayCommand(_onCancel);
        }

        private void SelectPhoto()
        {
            OpenFileDialog fileDialog =
                new OpenFileDialog
                {
                    Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                    Multiselect = false
                };

            bool? result = fileDialog.ShowDialog();

            if (!(result ?? true))
            {
                return;
            }

            string selectedFilePath = fileDialog.FileName;

            if (!LoadPreview(selectedFilePath))
            {
                ShowProfilePictureFailure(ProfilePictureStatus.InvalidImage);
                return;
            }

            _selectedFilePath = selectedFilePath;
        }

        private void AddPhoto()
        {
            if (string.IsNullOrWhiteSpace(_selectedFilePath))
            {
                ShowProfilePictureFailure(ProfilePictureStatus.InvalidImage);
                return;
            }

            if (!_playerSession.PlayerId.HasValue)
            {
                ShowProfilePictureFailure(ProfilePictureStatus.ServiceUnavailable);
                return;
            }

            byte[] imageData;

            try
            {
                imageData = File.ReadAllBytes(_selectedFilePath);
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException)
            {
                ShowProfilePictureFailure(ProfilePictureStatus.ServiceUnavailable);
                return;
            }

            try
            {
                DomainProfilePicture picture = DomainProfilePicture.FromSelectedContent(
                    Path.GetFileName(_selectedFilePath), imageData);
                ProfilePictureStatus reason = _accountService.SetProfilePicture(_playerSession.PlayerId.Value, picture);
                if (reason != ProfilePictureStatus.None)
                {
                    ShowProfilePictureFailure(reason);
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Profile picture update failed: {0}", ex);
                ShowProfilePictureFailure(ProfilePictureStatus.ServiceUnavailable);
                return;
            }

            _onFinishRegistration();
        }

        private void SkipPhoto()
        {
            _onFinishRegistration();
        }

        private bool LoadPreview(string filePath)
        {
            try
            {
                BitmapImage image = new BitmapImage();

                image.BeginInit();

                image.CacheOption = BitmapCacheOption.OnLoad;

                image.UriSource =
                    new Uri(
                        filePath,
                        UriKind.Absolute);

                image.EndInit();
                image.Freeze();

                ProfilePicturePreview = image;

                return true;
            }
            catch (Exception ex) when (ex is ArgumentException ||
                                       ex is InvalidOperationException ||
                                       ex is NotSupportedException ||
                                       ex is IOException)
            {
                ProfilePicturePreview = null;

                return false;
            }
        }

        private void ShowProfilePictureFailure(
            ProfilePictureStatus status)
        {
            switch (status)
            {
                case ProfilePictureStatus.InvalidFormat:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageProfile_msgPhotoInvalidFormatTitle",
                        MessageKey = "MessageProfile_msgPhotoInvalidFormat"
                    });
                    break;

                case ProfilePictureStatus.InvalidImage:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageProfile_msgPhotoInvalidTitle",
                        MessageKey = "MessageProfile_msgPhotoInvalid"
                    });
                    break;

                case ProfilePictureStatus.FileTooLarge:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey = "MessageProfile_msgPhotoTooLargeTitle",
                        MessageKey = "MessageProfile_msgPhotoTooLarge"
                    });
                    break;

                case ProfilePictureStatus.PlayerNotFound:
                case ProfilePictureStatus.ServiceUnavailable:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey = "MessageProfile_msgPhotoCannotBeUploadedTitle",
                        MessageKey = "MessageProfile_msgPhotoCannotBeUploaded"
                    });
                    break;

                default:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey = "Global_msgDefaultErrorTitle",
                        MessageKey = "Global_msgDefaultError"
                    });
                    break;
            }
        }
    }
}
