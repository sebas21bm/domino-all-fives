using System;
using System.Windows.Media.Imaging;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Presentation retained for a future remote use case implementation.</summary>
    public class EditProfileViewModel : ViewModelBase
    {
        private IFrameNavigationService _navigationService;
        public string Username { get; set; } = string.Empty;
        public BitmapImage ProfilePicture { get; set; }
        public RelayCommand GoBackCommand { get; }
        public RelayCommand ChangePictureCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand DiscardCommand { get; }

        public EditProfileViewModel()
        {
            GoBackCommand = new RelayCommand(() => _navigationService.GoBack());
            ChangePictureCommand = new RelayCommand(() => { }, () => false);
            SaveCommand = new RelayCommand(() => { }, () => false);
            DiscardCommand = new RelayCommand(() => { }, () => false);
        }

        public void Initialize(
            IFrameNavigationService navigationService)
        {
            if (_navigationService != null)
            {
                throw new InvalidOperationException(
                    "EditProfileViewModel has already been initialized.");
            }

            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));

            _navigationService = navigationService;
        }
    }
}
