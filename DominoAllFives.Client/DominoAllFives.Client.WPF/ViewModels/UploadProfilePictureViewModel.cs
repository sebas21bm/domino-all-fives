using System;
using System.IO;
using System.Windows.Media.Imaging;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Threading;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFivesService;
using Microsoft.Extensions.Logging;
using RemotePicture = DominoAllFivesService.ProfilePicture;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Allows optional picture selection and upload after account creation.</summary>
    public class UploadProfilePictureViewModel : ViewModelBase
    {
        private IDialogService _dialogService;
        private IProfileClient _profileClient;
        private readonly PlayerSession _playerSession;
        private ProfilePictureFileService _fileService;
        private ILogger<UploadProfilePictureViewModel> _logger;
        private readonly Action _onFinishRegistration;
        private readonly Action _onCancel;
        private RemotePicture _selectedPicture;
        private BitmapImage _profilePicturePreview;
        private bool _isBusy;

        public BitmapImage ProfilePicturePreview
        {
            get
            {
                return _profilePicturePreview;
            }
            private set
            {
                SetProperty(ref _profilePicturePreview, value);
            }
        }

        public RelayCommand SelectPhotoCommand { get; }
        public RelayCommand AddPhotoCommand { get; }
        public RelayCommand SkipPhotoCommand { get; }
        public RelayCommand CancelCommand { get; }

        /// <summary>Creates optional picture presentation state and navigation commands.</summary>
        /// <param name = "playerSession">The account session created during registration.</param>
        /// <param name = "onFinishRegistration">
        /// Continues after uploading or skipping a picture.
        /// </param>
        /// <param name = "onCancel">Closes the optional picture step.</param>
        public UploadProfilePictureViewModel(
            PlayerSession playerSession,
            Action onFinishRegistration,
            Action onCancel)
        {
            _playerSession = playerSession;
            _onFinishRegistration = onFinishRegistration;
            _onCancel = onCancel;
            SelectPhotoCommand = new RelayCommand(SelectPhoto, CanInteract);
            AddPhotoCommand = new RelayCommand(AddPhoto, CanInteract);
            SkipPhotoCommand = new RelayCommand(SkipPhoto, CanInteract);
            CancelCommand = new RelayCommand(Cancel, CanInteract);
        }

        /// <summary>Injects the services required for local selection and remote upload.</summary>
        /// <param name = "dialogService">The localized dialog service.</param>
        /// <param name = "profileClient">The remote profile client.</param>
        /// <param name = "fileService">The local picture selection service.</param>
        /// <param name = "logger">The picture upload logger.</param>
        public void Initialize(
            IDialogService dialogService,
            IProfileClient profileClient,
            ProfilePictureFileService fileService,
            ILogger<UploadProfilePictureViewModel> logger)
        {
            _dialogService = dialogService;
            _fileService = fileService;
            _logger = logger;
            _profileClient = profileClient;
            SetBusy(false);
        }

        private void SelectPhoto()
        {
            if (!CanInteract())
            {
                return;
            }

            ProfilePictureFileResult selection = _fileService.SelectProfilePicture();
            if (selection.WasCancelled)
            {
                return;
            }

            if (!selection.IsSuccessful || !LoadPreview(selection.ImageData))
            {
                _selectedPicture = null;
                ProfilePicturePreview = null;
                ShowFailure(ProfileIssue.InvalidProfilePicture);
                return;
            }

            _selectedPicture = new RemotePicture
            {
                OriginalFileName = selection.FileName,
                ImageData = selection.ImageData
            };
        }

        private bool LoadPreview(byte[] imageData)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(imageData))
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze();
                    ProfilePicturePreview = image;
                    return true;
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogDebug(ex, "Selected image cannot be previewed.");
                return false;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogDebug(ex, "Selected image cannot be previewed.");
                return false;
            }
            catch (NotSupportedException ex)
            {
                _logger.LogDebug(ex, "Selected image cannot be previewed.");
                return false;
            }
            catch (FileFormatException ex)
            {
                _logger.LogDebug(ex, "Selected image cannot be previewed.");
                return false;
            }
            catch (IOException ex)
            {
                _logger.LogDebug(ex, "Selected image cannot be previewed.");
                return false;
            }
        }

        private void AddPhoto()
        {
            if (_isBusy)
            {
                return;
            }

            if (_selectedPicture == null)
            {
                ShowFailure(ProfileIssue.InvalidProfilePicture);
                return;
            }

            if (!_playerSession.HasServerSession)
            {
                ShowFailure(ProfileIssue.PlayerNotFound);
                return;
            }

            PictureUploaded upload = new PictureUploaded(
                _playerSession.SessionKey,
                _selectedPicture);
            BackgroundJob<PictureUploaded, bool> job = new BackgroundJob<PictureUploaded, bool>(
                Upload,
                upload,
                CompleteUpload,
                HandleUploadFailure);
            SetBusy(true);
            BackgroundWork.Start(job);
        }

        private bool Upload(PictureUploaded upload)
        {
            _profileClient.SetProfilePicture(upload.SessionKey, upload.Picture);
            return true;
        }

        private void CompleteUpload(bool result, PictureUploaded upload)
        {
            SetBusy(false);
            if (_playerSession.SessionKey == upload.SessionKey)
            {
                _onFinishRegistration();
            }
        }

        private void HandleUploadFailure(Exception ex, PictureUploaded upload)
        {
            SetBusy(false);
            if (_playerSession.SessionKey != upload.SessionKey)
            {
                return;
            }

            ProfileFailureDialogs.HandleRemoteFailure(ex, _logger, "Picture upload", ShowFailure);
        }

        private bool CanInteract()
        {
            return _profileClient != null && !_isBusy;
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;
            SelectPhotoCommand.RaiseCanExecuteChanged();
            AddPhotoCommand.RaiseCanExecuteChanged();
            SkipPhotoCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }

        private void SkipPhoto()
        {
            if (CanInteract())
            {
                _onFinishRegistration();
            }
        }

        private void Cancel()
        {
            if (CanInteract())
            {
                _onCancel();
            }
        }

        private void ShowFailure(
            ProfileIssue? issue = null,
            bool connectionFailure = false)
        {
            DialogRequest dialog = issue == ProfileIssue.PlayerNotFound
                ? FailureDialogs.Create(
                    DialogType.Error,
                    "MessageProfile_msgPhotoCannotBeUploadedTitle",
                    "MessageProfile_msgPhotoCannotBeUploaded")
                : ProfileFailureDialogs.ForPictureOrTechnicalFailure(issue, connectionFailure);
            _dialogService.ShowDialog(dialog);
        }

        private sealed class PictureUploaded
        {
            public string SessionKey { get; }
            public RemotePicture Picture { get; }

            public PictureUploaded(string sessionKey, RemotePicture picture)
            {
                SessionKey = sessionKey;
                Picture = picture;
            }
        }
    }
}
