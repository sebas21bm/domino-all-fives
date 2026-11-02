using System;
using System.Windows.Media.Imaging;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Presentation retained for a future remote use case implementation.</summary>
    public class UploadProfilePictureViewModel : ViewModelBase
    {
        public BitmapImage ProfilePicturePreview { get; set; }
        public RelayCommand SelectPhotoCommand { get; }
        public RelayCommand AddPhotoCommand { get; }
        public RelayCommand SkipPhotoCommand { get; }
        public RelayCommand CancelCommand { get; }

        public UploadProfilePictureViewModel(Action onCancel = null)
        {
            SelectPhotoCommand = new RelayCommand(() => { }, () => false);
            AddPhotoCommand = new RelayCommand(() => { }, () => false);
            SkipPhotoCommand = new RelayCommand(() => { }, () => false);
            CancelCommand = new RelayCommand(() => onCancel?.Invoke());
        }
    }
}
