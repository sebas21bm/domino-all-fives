using System;
using System.Windows.Media.Imaging;

using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Validation;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using Microsoft.Win32;


namespace DominoAllFives.Client.WPF.ViewModels
{
    public class UploadProfilePictureViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ProfilePictureController _profilePictureController;
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

        public RelayCommand SelectPhotoCommand { get; }
        public RelayCommand AddPhotoCommand { get; }
        public RelayCommand SkipPhotoCommand { get; }
        public RelayCommand CancelCommand { get; }

        public UploadProfilePictureViewModel(
            IDialogService dialogService,
            ProfilePictureController profilePictureController,
            PlayerSession playerSession,
            Action onFinishRegistration,
            Action onCancel)
        {
            _dialogService = dialogService
                ?? throw new ArgumentNullException(
                    nameof(dialogService));

            _profilePictureController = profilePictureController
                ?? throw new ArgumentNullException(
                    nameof(profilePictureController));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(
                    nameof(playerSession));

            _onFinishRegistration = onFinishRegistration
                ?? throw new ArgumentNullException(
                    nameof(onFinishRegistration));

            _onCancel = onCancel
                ?? throw new ArgumentNullException(
                    nameof(onCancel));

            SelectPhotoCommand =
                new RelayCommand(SelectPhoto);

            AddPhotoCommand =
                new RelayCommand(AddPhoto);

            SkipPhotoCommand =
                new RelayCommand(SkipPhoto);

            CancelCommand =
                new RelayCommand(_onCancel);
        }

        private void SelectPhoto()
        {
            OpenFileDialog fileDialog =
                new OpenFileDialog
                {
                    Filter =
                        "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                    Multiselect = false
                };

            bool? result = fileDialog.ShowDialog();

            if (result != true)
            {
                return;
            }

            string selectedFilePath =
                fileDialog.FileName;

            if (!ProfilePictureValidator.HasValidExtension(
                selectedFilePath))
            {
                ShowInvalidFormatMessage();
                return;
            }

            if (!ProfilePictureValidator.HasValidFileSize(
                selectedFilePath))
            {
                ShowPhotoTooLargeMessage();
                return;
            }

            if (!LoadPreview(selectedFilePath))
            {
                ShowInvalidFormatMessage();
                return;
            }

            _selectedFilePath = selectedFilePath;
        }

        private void AddPhoto()
        {
            if (string.IsNullOrWhiteSpace(
                _selectedFilePath))
            {
                ShowInvalidFormatMessage();
                return;
            }

            if (!_playerSession.PlayerId.HasValue)
            {
                ShowPhotoCannotBeUploadedMessage();
                return;
            }

            bool wasSaved =
                _profilePictureController.SetProfilePicture(
                    _playerSession.PlayerId.Value,
                    _selectedFilePath);

            if (!wasSaved)
            {
                ShowPhotoCannotBeUploadedMessage();
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
                BitmapImage image =
                    new BitmapImage();

                image.BeginInit();

                image.CacheOption =
                    BitmapCacheOption.OnLoad;

                image.UriSource =
                    new Uri(
                        filePath,
                        UriKind.Absolute);

                image.EndInit();
                image.Freeze();

                ProfilePicturePreview = image;

                return true;
            }
            catch
            {
                ProfilePicturePreview = null;

                return false;
            }
        }

        private void ShowInvalidFormatMessage()
        {
            _dialogService.ShowDialog(
                DialogType.Warning,
                "MessageProfile_msgPhotoInvalidFormatTitle",
                "MessageProfile_msgPhotoInvalidFormat",
                () => { });
        }

        private void ShowPhotoTooLargeMessage()
        {
            _dialogService.ShowDialog(
                DialogType.Warning,
                "MessageProfile_msgPhotoTooLargeTitle",
                "MessageProfile_msgPhotoTooLarge",
                () => { });
        }

        private void ShowPhotoCannotBeUploadedMessage()
        {
            _dialogService.ShowDialog(
                DialogType.Error,
                "MessageProfile_msgPhotoCannotBeUploadedTitle",
                "MessageProfile_msgPhotoCannotBeUploaded",
                () => { });
        }
    }
}