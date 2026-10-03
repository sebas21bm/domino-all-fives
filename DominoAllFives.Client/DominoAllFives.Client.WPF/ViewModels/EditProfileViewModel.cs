using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class EditProfileViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;
        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }
        public RelayCommand GoBackCommand { get; }
        public RelayCommand ChangePictureCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand DiscardCommand { get; }

        public EditProfileViewModel(IFrameNavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            GoBackCommand = new RelayCommand(ExecuteGoBack);
            ChangePictureCommand = new RelayCommand(ExecuteChangePicture);
            SaveCommand = new RelayCommand(ExecuteSave);
            DiscardCommand = new RelayCommand(ExecuteDiscard);
        }

        private void ExecuteGoBack(object parameter)
        {
            _navigationService.GoBack();
        }

        private void ExecuteChangePicture(object parameter)
        {
        }

        private void ExecuteSave(object parameter)
        {
            _navigationService.GoBack();
        }

        private void ExecuteDiscard(object parameter)
        {
            _navigationService.GoBack();
        }
    }
}
