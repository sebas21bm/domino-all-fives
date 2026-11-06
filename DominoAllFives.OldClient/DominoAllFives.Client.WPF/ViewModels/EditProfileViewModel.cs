using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.ProfilePicture;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Contracts.Domain;
using DomainProfilePicture = DominoAllFives.Contracts.Domain.ProfilePicture;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.OldServices;
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
                    RetrieveInformationStatus.NotFound);
                return;
            }

            bool usernameChanged =
                Username != _originalUsername;

            bool hasPictureReplacement =
                _selectedProfilePictureData != null;

            if (!usernameChanged &&
                !hasPictureReplacement)
            {
                return;
            }

            try
            {
                UpdateProfileStatus reason;
                if (hasPictureReplacement)
                {
                    DomainProfilePicture picture = DomainProfilePicture.FromSelectedContent(
                        _selectedProfilePictureFileName, _selectedProfilePictureData);
                    reason = _accountService.UpdateProfile(_playerSession.PlayerId.Value, Username, picture);
                }
                else
                {
                    reason = _accountService.UpdateProfile(_playerSession.PlayerId.Value, Username);
                }
                if (reason != UpdateProfileStatus.None)
                {
                    ShowUpdateProfileFailure(reason);
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Profile update failed: {0}", ex);
                ShowUpdateProfileFailure(UpdateProfileStatus.ServiceUnavailable);
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
                    RetrieveInformationStatus.NotFound);
                return;
            }

            PlayerProfile profile;
            try
            {
                profile = _accountService.GetProfile(_playerSession.PlayerId.Value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Profile retrieval failed: {0}", ex);
                ShowRetrieveInformationFailure(RetrieveInformationStatus.ServiceUnavailable);
                return;
            }

            if (profile == null)
            {
                ShowRetrieveInformationFailure(RetrieveInformationStatus.NotFound);
                return;
            }

            _originalUsername = profile.Username;
            Username = profile.Username;

            ProfilePicture =
                _profilePictureImageService.GetProfilePicture(
                    profile.Picture.ImageData);
        }

        private void ShowRetrieveInformationFailure(
            RetrieveInformationStatus reason)
        {
            switch (reason)
            {
                case RetrieveInformationStatus.NotFound:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey = "Global_msgNotFoundTitle",
                        MessageKey = "Global_msgNotFound"
                    });
                    break;

                case RetrieveInformationStatus.ServiceUnavailable:
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
            UpdateProfileStatus reason)
        {
            switch (reason)
            {
                case UpdateProfileStatus.InvalidUsername:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey =
                            "MessageAccount_msgInvalidUsernameTitle",
                        MessageKey =
                            "MessageAccount_msgInvalidUsername"
                    });
                    break;

                case UpdateProfileStatus.UsernameAlreadyExists:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey =
                            "MessageAccount_msgUsernameUsedTitle",
                        MessageKey =
                            "MessageAccount_msgUsernameUsed"
                    });
                    break;

                case UpdateProfileStatus.InvalidProfilePictureFormat:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey =
                            "MessageProfile_msgPhotoInvalidFormatTitle",
                        MessageKey =
                            "MessageProfile_msgPhotoInvalidFormat"
                    });
                    break;

                case UpdateProfileStatus.InvalidProfilePicture:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey =
                            "MessageProfile_msgPhotoInvalidTitle",
                        MessageKey =
                            "MessageProfile_msgPhotoInvalid"
                    });
                    break;

                case UpdateProfileStatus.ProfilePictureTooLarge:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Warning,
                        TitleKey =
                            "MessageProfile_msgPhotoTooLargeTitle",
                        MessageKey =
                            "MessageProfile_msgPhotoTooLarge"
                    });
                    break;

                case UpdateProfileStatus.PlayerNotFound:
                case UpdateProfileStatus.ServiceUnavailable:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey =
                            "MessageProfile_msgUpdateProfileErrorTitle",
                        MessageKey =
                            "MessageProfile_msgUpdateProfileError"
                    });
                    break;

                case UpdateProfileStatus.SameUsername:
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
