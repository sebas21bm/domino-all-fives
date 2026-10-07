using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.ProfilePicture;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Services;
using System;
using System.Windows.Media.Imaging;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class EditProfileViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;
        private readonly IAccountService _accountService;
        private readonly IDialogService _dialogService;
        private readonly PlayerSession _playerSession;
        private readonly ProfilePictureImageService _profilePictureImageService;
        private readonly ProfilePictureFileService _profilePictureFileService;

        private string _username;
        private BitmapImage _profilePicture;
        private string _originalUsername;
        private byte[] _selectedProfilePictureData;
        private string _selectedProfilePictureFileName;

        public string Username
        {
            get => _username;
            set
            {
                if (_username == value)
                {
                    return;
                }

                _username = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage ProfilePicture
        {
            get => _profilePicture;
            private set
            {
                if (_profilePicture == value)
                {
                    return;
                }

                _profilePicture = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand GoBackCommand { get; }
        public RelayCommand ChangePictureCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand DiscardCommand { get; }

        public EditProfileViewModel(
            IFrameNavigationService navigationService,
            IAccountService accountService,
            IDialogService dialogService,
            PlayerSession playerSession,
            ProfilePictureImageService profilePictureImageService,
            ProfilePictureFileService profilePictureFileService)
        {
            _navigationService = navigationService
                ?? throw new ArgumentNullException(
                    nameof(navigationService));

            _accountService = accountService
                ?? throw new ArgumentNullException(
                    nameof(accountService));

            _dialogService = dialogService
                ?? throw new ArgumentNullException(
                    nameof(dialogService));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(
                    nameof(playerSession));

            _profilePictureImageService = profilePictureImageService
                ?? throw new ArgumentNullException(
                    nameof(profilePictureImageService));

            _profilePictureFileService= profilePictureFileService
                ?? throw new ArgumentNullException(
                    nameof(profilePictureFileService));

            GoBackCommand = new RelayCommand(ExecuteGoBack);
            ChangePictureCommand = new RelayCommand(ExecuteChangePicture);
            SaveCommand = new RelayCommand(ExecuteSave);
            DiscardCommand = new RelayCommand(ExecuteDiscard);

            LoadProfile();
        }

        private void ExecuteGoBack(object parameter)
        {
            _navigationService.GoBack();
        }

        private void ExecuteChangePicture(object parameter)
        {
            ProfilePictureFileResult result =
                _profilePictureFileService.SelectProfilePicture();

            if (result.WasCancelled)
            {
                return;
            }

            if (!result.IsSuccessful)
            {
                ShowProfilePictureSelectionFailure();
                return;
            }

            _selectedProfilePictureData = result.ImageData;
            _selectedProfilePictureFileName = result.FileName;

            ProfilePicture =
                _profilePictureImageService.GetProfilePicture(
                    result.ImageData);
        }

        private void ExecuteSave(object parameter)
        {
            if (!_playerSession.PlayerId.HasValue)
            {
                ShowRetrieveInformationFailure(
                    RetrieveInformationFailureReason.NotFound);
                return;
            }

            bool usernameChanged =
                Username != _originalUsername;

            bool profilePictureChanged =
                _selectedProfilePictureData != null;

            if (!usernameChanged &&
                !profilePictureChanged)
            {
                return;
            }

            UpdateProfileDto profileData =
                new UpdateProfileDto
                {
                    PlayerId = _playerSession.PlayerId.Value,
                    Username = Username,
                    ProfilePictureChanged = profilePictureChanged,
                    ProfilePictureFileName =
                        _selectedProfilePictureFileName,
                    ProfilePictureData =
                        _selectedProfilePictureData
                };

            UpdateProfileResultDto result =
                _accountService.UpdateProfile(profileData);

            if (!result.IsSuccessful)
            {
                ShowUpdateProfileFailure(
                    result.FailureReason);
                return;
            }

            if (usernameChanged)
            {
                _playerSession.UpdateUsername(Username);
            }

            _dialogService.ShowDialog(new DialogRequest
            {
                Type = DialogType.Information,
                TitleKey =
                    "MessageProfile_msgProfileUpdatedTitle",
                MessageKey =
                    "MessageProfile_msgProfileUpdated",
                OnAccept = () =>
                    _navigationService.GoBackToRefresh<ProfileViewModel>()
            });
        }

        private void ExecuteDiscard(object parameter)
        {
            _navigationService.GoBack();
        }

        private void LoadProfile()
        {
            if (!_playerSession.PlayerId.HasValue)
            {
                ShowRetrieveInformationFailure(
                    RetrieveInformationFailureReason.NotFound);
                return;
            }

            ProfileDto profile =
                _accountService.GetProfile(
                    _playerSession.PlayerId.Value);

            if (profile.FailureReason !=
                RetrieveInformationFailureReason.None)
            {
                ShowRetrieveInformationFailure(
                    profile.FailureReason);
                return;
            }

            _originalUsername = profile.Username;
            Username = profile.Username;

            ProfilePicture =
                _profilePictureImageService.GetProfilePicture(
                    profile.ProfilePictureData);
        }

        private void ShowRetrieveInformationFailure(
            RetrieveInformationFailureReason reason)
        {
            switch (reason)
            {
                case RetrieveInformationFailureReason.NotFound:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey = "Global_msgNotFoundTitle",
                        MessageKey = "Global_msgNotFound"
                    });
                    break;

                case RetrieveInformationFailureReason.ServiceUnavailable:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey = "Global_msgConnectionErrorTitle",
                        MessageKey = "Global_msgConnectionError"
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

        private void ShowProfilePictureSelectionFailure()
        {
            _dialogService.ShowDialog(new DialogRequest
            {
                Type = DialogType.Error,
                TitleKey = "Global_msgDefaultErrorTitle",
                MessageKey = "Global_msgDefaultError"
            });
        }

        private void ShowUpdateProfileFailure(
            UpdateProfileFailureReason reason)
        {
            switch (reason)
            {
                case UpdateProfileFailureReason.InvalidUsername:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey =
                            "MessageAccount_msgInvalidUsernameTitle",
                        MessageKey =
                            "MessageAccount_msgInvalidUsername"
                    });
                    break;

                case UpdateProfileFailureReason.UsernameAlreadyExists:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey =
                            "MessageAccount_msgUsernameUsedTitle",
                        MessageKey =
                            "MessageAccount_msgUsernameUsed"
                    });
                    break;

                case UpdateProfileFailureReason.InvalidProfilePictureFormat:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey =
                            "MessageProfile_msgPhotoInvalidFormatTitle",
                        MessageKey =
                            "MessageProfile_msgPhotoInvalidFormat"
                    });
                    break;

                case UpdateProfileFailureReason.InvalidProfilePicture:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey =
                            "MessageProfile_msgPhotoInvalidTitle",
                        MessageKey =
                            "MessageProfile_msgPhotoInvalid"
                    });
                    break;

                case UpdateProfileFailureReason.ProfilePictureTooLarge:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey =
                            "MessageProfile_msgPhotoTooLargeTitle",
                        MessageKey =
                            "MessageProfile_msgPhotoTooLarge"
                    });
                    break;

                case UpdateProfileFailureReason.PlayerNotFound:
                case UpdateProfileFailureReason.ServiceUnavailable:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey =
                            "MessageProfile_msgUpdateProfileErrorTitle",
                        MessageKey =
                            "MessageProfile_msgUpdateProfileError"
                    });
                    break;

                case UpdateProfileFailureReason.SameUsername:
                    break;

                default:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey =
                            "Global_msgDefaultErrorTitle",
                        MessageKey =
                            "Global_msgDefaultError"
                    });
                    break;
            }
        }
    }
}
