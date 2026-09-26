using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class UploadProfilePictureViewModel : ViewModelBase
    {
        private readonly Action _onFinishRegistration;
        private readonly Action _onCancel;

        private string _selectedImagePath;

        public string SelectedImagePath
        {
            get => _selectedImagePath;
            set => SetProperty(ref _selectedImagePath, value);
        }

        public RelayCommand SelectImageCommand { get; }
        public RelayCommand UploadCommand { get; }
        public RelayCommand SkipCommand { get; }
        public RelayCommand CancelCommand { get; }

        public UploadProfilePictureViewModel(
            Action onFinishRegistration,
            Action onCancel)
        {
            _onFinishRegistration = onFinishRegistration ?? throw new ArgumentNullException(nameof(onFinishRegistration));
            _onCancel = onCancel ?? throw new ArgumentNullException(nameof(onCancel));

            SelectImageCommand = new RelayCommand(ExecuteSelectImage);
            UploadCommand = new RelayCommand(ExecuteUpload);
            SkipCommand = new RelayCommand(ExecuteSkip);
            CancelCommand = new RelayCommand(_onCancel);
        }

        private void ExecuteSelectImage()
        {
            // TODO: Abrir OpenFileDialog para seleccionar la ruta de la imagen
        }

        private void ExecuteUpload()
        {
            // TODO: Transición directa al finalizar el registro
            _onFinishRegistration?.Invoke();
        }

        private void ExecuteSkip()
        {
            // TODO: Omitir selección y finalizar registro (FA-06 CU-05)
            _onFinishRegistration?.Invoke();
        }
    }
}
