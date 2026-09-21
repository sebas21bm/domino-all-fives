using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DominoAllFives.Client.WPF.Controls
{
    /// <summary>
    /// Lógica de interacción para UploadProfilePicture.xaml
    /// </summary>
    public partial class UploadProfilePicture : UserControl
    {
        private readonly Action _onBack;
        private readonly Action _onAddPhoto;
        private readonly Action _onSkipPhoto;

        public UploadProfilePicture(Action onBack, Action onAddPhoto, Action onSkipPhoto)
        {
            _onBack = onBack;
            _onAddPhoto = onAddPhoto;
            _onSkipPhoto = onSkipPhoto;
            InitializeComponent();
        }

        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            _onBack.Invoke();
        }

        private void SelectPictureButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void AddPhotoButtonClick(object sender, RoutedEventArgs e)
        {

            _onAddPhoto.Invoke();
        }

        private void SkipPhotoButtonClick(object sender, RoutedEventArgs e)
        {
            _onSkipPhoto.Invoke();
        }
    }
}
