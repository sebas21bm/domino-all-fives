using System;
using System.Windows.Media.Imaging;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.ProfilePicture;
using DominoAllFives.Client.WPF.Services.Threading;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFivesService;
using Microsoft.Extensions.Logging;
using RemotePicture = DominoAllFivesService.ProfilePicture;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>
    /// Edits the profile draft and persists confirmed changes through the server.
    /// </summary>
    public class EditProfileViewModel : ViewModelBase
    {
        private IFrameNavigationService _navigationService;
        private IProfileClient _profileClient;
        private IDialogService _dialogService;
        private readonly PlayerSession _playerSession;
        private ProfilePictureImageService _imageService;
        private ProfilePictureFileService _fileService;
        private ILogger<EditProfileViewModel> _logger;
        private string _username;
        private string _originalUsername;
        private BitmapImage _profilePicture;
        private RemotePicture _selectedPicture;
        private bool _isBusy;
        private bool _isLoaded;
        private bool _hasStoredPicture;
        private bool _pictureRemoved;

        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                if (SetProperty(ref _username, value))
                {
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public BitmapImage ProfilePicture
        {
            get
            {
                return _profilePicture;
            }
            private set
            {
                SetProperty(ref _profilePicture, value);
            }
        }

        public RelayCommand GoBackCommand { get; }
        public RelayCommand ChangePictureCommand { get; }
        public RelayCommand RemovePictureCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand DiscardCommand { get; }

        /// <summary>Creates the profile draft and commands without querying the server.</summary>
        /// <param name = "playerSession">The current local player session.</param>
        /// <exception cref = "ArgumentNullException">The session is null.</exception>
        public EditProfileViewModel(PlayerSession playerSession)
        {
            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));
            _username = string.Empty;
            GoBackCommand = new RelayCommand(GoBack, CanGoBack);
            ChangePictureCommand = new RelayCommand(ChangePicture, CanEdit);
            RemovePictureCommand = new RelayCommand(ConfirmRemovePicture, CanRemovePicture);
            SaveCommand = new RelayCommand(Save, CanSave);
            DiscardCommand = new RelayCommand(GoBack, CanGoBack);
        }

        /// <summary>
        /// Configures local picture services before initializing profile editing.
        /// </summary>
        /// <param name = "imageService">
        /// Creates display images from selected or stored content.
        /// </param>
        /// <param name = "fileService">Selects and reads a local picture file.</param>
        /// <exception cref = "ArgumentNullException">A picture service is null.</exception>
        /// <exception cref = "InvalidOperationException">
        /// Picture services are already configured.
        /// </exception>
        public void ConfigurePictures(
            ProfilePictureImageService imageService,
            ProfilePictureFileService fileService)
        {
            if (_imageService != null || _fileService != null || _profileClient != null)
            {
                throw new InvalidOperationException(
                    "Profile picture services are already configured.");
            }

            ProfilePictureImageService configuredImageService = imageService
                ?? throw new ArgumentNullException(nameof(imageService));
            ProfilePictureFileService configuredFileService = fileService
                ?? throw new ArgumentNullException(nameof(fileService));
            _imageService = configuredImageService;
            _fileService = configuredFileService;
        }

        /// <summary>
        /// Injects editing services and loads the draft after configuration is complete.
        /// </summary>
        /// <param name = "navigationService">The application navigation service.</param>
        /// <param name = "profileClient">The remote profile client.</param>
        /// <param name = "dialogService">The localized dialog service.</param>
        /// <param name = "logger">The profile editing logger.</param>
        /// <exception cref = "ArgumentNullException">A required service is null.</exception>
        /// <exception cref = "InvalidOperationException">
        /// Already initialized or local picture services have not been configured.
        /// </exception>
        public void Initialize(
            IFrameNavigationService navigationService,
            IProfileClient profileClient,
            IDialogService dialogService,
            ILogger<EditProfileViewModel> logger)
        {
            if (_profileClient != null)
            {
                throw new InvalidOperationException("EditProfileViewModel is already initialized.");
            }

            if (_imageService == null || _fileService == null)
            {
                throw new InvalidOperationException(
                    "Configure picture services before initialization.");
            }

            _navigationService = navigationService
                ?? throw new ArgumentNullException(nameof(navigationService));
            _dialogService = dialogService
                ?? throw new ArgumentNullException(nameof(dialogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _profileClient = profileClient
                ?? throw new ArgumentNullException(nameof(profileClient));
            SetBusy(false);
            LoadProfile();
        }

        private void EnsureInitialized()
        {
            if (_profileClient == null)
            {
                throw new InvalidOperationException(
                    "EditProfileViewModel must be initialized first.");
            }
        }

        private void LoadProfile()
        {
            if (!_playerSession.HasServerSession)
            {
                ShowFailure(ProfileIssue.PlayerNotFound);
                return;
            }

            BackgroundJob<string, PlayerProfile> job = new BackgroundJob<string, PlayerProfile>(
                _profileClient.GetMyProfile,
                _playerSession.SessionKey,
                CompleteLoad,
                HandleLoadFailure);
            SetBusy(true);
            BackgroundWork.Start(job);
        }

        private void CompleteLoad(PlayerProfile profile, string sessionKey)
        {
            SetBusy(false);
            if (_playerSession.SessionKey != sessionKey)
            {
                return;
            }

            if (profile == null || profile.Picture == null
                || string.IsNullOrWhiteSpace(profile.Username))
            {
                _logger.LogError("Profile service returned an incomplete profile for editing.");
                ShowFailure();
                return;
            }

            _originalUsername = profile.Username;
            Username = profile.Username;
            byte[] imageData = profile.Picture.ImageData;
            _hasStoredPicture = imageData != null && imageData.Length > 0;
            ProfilePicture = _imageService.GetProfilePicture(imageData);
            _isLoaded = true;
            RaiseCommands();
        }

        private void ChangePicture()
        {
            EnsureInitialized();
            if (!CanEdit())
            {
                return;
            }

            ProfilePictureFileResult selection = _fileService.SelectProfilePicture();
            if (selection.WasCancelled)
            {
                return;
            }

            if (!selection.IsSuccessful)
            {
                ShowFailure(ProfileIssue.InvalidProfilePicture);
                return;
            }

            _selectedPicture = new RemotePicture
            {
                OriginalFileName = selection.FileName,
                ImageData = selection.ImageData
            };
            ProfilePicture = _imageService.GetProfilePicture(selection.ImageData);
            SaveCommand.RaiseCanExecuteChanged();
        }

        private void Save()
        {
            EnsureInitialized();
            if (!CanSave())
            {
                return;
            }

            ProfileChanges changes = new ProfileChanges(
                _playerSession.SessionKey,
                Username,
                _selectedPicture);
            BackgroundJob<ProfileChanges, bool> job = new BackgroundJob<ProfileChanges, bool>(
                SaveProfile,
                changes,
                CompleteSave,
                HandleSaveFailure);
            SetBusy(true);
            BackgroundWork.Start(job);
        }

        private bool SaveProfile(ProfileChanges changes)
        {
            if (changes.Picture == null)
            {
                _profileClient.UpdateProfileName(changes.SessionKey, changes.Username);
            }
            else
            {
                _profileClient.UpdateProfileWithPicture(
                    changes.SessionKey,
                    changes.Username,
                    changes.Picture);
            }

            return true;
        }

        private void CompleteSave(bool result, ProfileChanges changes)
        {
            SetBusy(false);
            if (_playerSession.SessionKey != changes.SessionKey)
            {
                return;
            }

            _playerSession.UpdateUsername(changes.Username);
            _originalUsername = changes.Username;
            Username = changes.Username;
            if (changes.Picture != null)
            {
                _hasStoredPicture = true;
                ProfilePicture = _imageService.GetProfilePicture(changes.Picture.ImageData);
            }

            _selectedPicture = null;
            RaiseCommands();
            _dialogService.ShowDialog(new DialogRequest
            {
                Type = DialogType.Information,
                TitleKey = "MessageProfile_msgProfileUpdatedTitle",
                MessageKey = "MessageProfile_msgProfileUpdated",
                OnAccept = RefreshProfile
            });
        }

        private void ConfirmRemovePicture()
        {
            EnsureInitialized();
            if (!CanRemovePicture())
            {
                return;
            }

            _dialogService.ShowDialog(new DialogRequest
            {
                Type = DialogType.Confirmation,
                TitleKey = "MessageProfile_msgRemovePictureTitle",
                MessageKey = "MessageProfile_msgRemovePicture",
                OnAccept = RemovePicture
            });
        }

        private void RemovePicture()
        {
            EnsureInitialized();
            if (!CanRemovePicture())
            {
                return;
            }

            BackgroundJob<string, bool> job = new BackgroundJob<string, bool>(
                RemoveStoredPicture,
                _playerSession.SessionKey,
                CompleteRemove,
                HandleLoadFailure);
            SetBusy(true);
            BackgroundWork.Start(job);
        }

        private bool RemoveStoredPicture(string sessionKey)
        {
            _profileClient.RemoveProfilePicture(sessionKey);
            return true;
        }

        private void CompleteRemove(bool result, string sessionKey)
        {
            SetBusy(false);
            if (_playerSession.SessionKey != sessionKey)
            {
                return;
            }

            _hasStoredPicture = false;
            _pictureRemoved = true;
            _selectedPicture = null;
            ProfilePicture = _imageService.GetProfilePicture(null);
            RaiseCommands();
        }

        private void HandleLoadFailure(Exception ex, string sessionKey)
        {
            SetBusy(false);
            if (_playerSession.SessionKey == sessionKey)
            {
                HandleFailure(ex);
            }
        }

        private void HandleSaveFailure(Exception ex, ProfileChanges changes)
        {
            HandleLoadFailure(ex, changes.SessionKey);
        }

        private void HandleFailure(Exception ex)
        {
            ProfileFailureDialogs.HandleRemoteFailure(
                ex,
                _logger,
                "Profile operation",
                ShowFailure);
        }

        private bool CanGoBack()
        {
            return _profileClient != null && !_isBusy;
        }

        private bool CanEdit()
        {
            return _profileClient != null && !_isBusy && _isLoaded
                && _playerSession.HasServerSession;
        }

        private bool CanSave()
        {
            return CanEdit()
                && (Username != _originalUsername || _selectedPicture != null);
        }

        private bool CanRemovePicture()
        {
            return CanEdit() && _hasStoredPicture;
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;
            RaiseCommands();
        }

        private void RaiseCommands()
        {
            GoBackCommand.RaiseCanExecuteChanged();
            ChangePictureCommand.RaiseCanExecuteChanged();
            RemovePictureCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            DiscardCommand.RaiseCanExecuteChanged();
        }

        private void GoBack()
        {
            EnsureInitialized();
            if (!CanGoBack())
            {
                return;
            }

            if (_pictureRemoved)
            {
                RefreshProfile();
            }
            else
            {
                _navigationService.GoBack();
            }
        }

        private void RefreshProfile()
        {
            _navigationService.GoBackToRefresh<ProfileViewModel>();
        }

        private void ShowFailure(
            ProfileIssue? issue = null,
            bool connectionFailure = false)
        {
            DialogRequest dialog;
            switch (issue)
            {
                case ProfileIssue.InvalidUsername:
                    dialog = FailureDialogs.Create(
                        DialogType.Warning,
                        "MessageAccount_msgInvalidUsernameTitle",
                        "MessageAccount_msgInvalidUsername");
                    break;
                case ProfileIssue.UsernameAlreadyExists:
                    dialog = FailureDialogs.Create(
                        DialogType.Warning,
                        "MessageAccount_msgUsernameUsedTitle",
                        "MessageAccount_msgUsernameUsed");
                    break;
                case ProfileIssue.PlayerNotFound:
                    dialog = ProfileFailureDialogs.ProfileNotFound();
                    break;
                case ProfileIssue.SameUsername:
                    return;
                default:
                    dialog = ProfileFailureDialogs.ForPictureOrTechnicalFailure(
                        issue,
                        connectionFailure);
                    break;
            }

            _dialogService.ShowDialog(dialog);
        }

        private sealed class ProfileChanges
        {
            public string SessionKey { get; }
            public string Username { get; }
            public RemotePicture Picture { get; }

            public ProfileChanges(string sessionKey, string username, RemotePicture picture)
            {
                SessionKey = sessionKey;
                Username = username;
                Picture = picture;
            }
        }
    }
}
