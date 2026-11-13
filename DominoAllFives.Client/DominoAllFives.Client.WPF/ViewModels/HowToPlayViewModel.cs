using System;
using System.Windows.Input;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class HowToPlayViewModel : ViewModelBase
    {
        private IFrameNavigationService _navigationService;
        public ICommand NavigateBackCommand { get; }

        public HowToPlayViewModel()
        {
            NavigateBackCommand = new RelayCommand(ExecuteNavigateBack);
        }

        public void Initialize(IFrameNavigationService navigationService)
        {
            if (_navigationService != null)
            {
                throw new InvalidOperationException(
                    "HowToPlayViewModel has already been initialized.");
            }

            if (navigationService == null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            _navigationService = navigationService;
        }

        private void ExecuteNavigateBack()
        {
            _navigationService.GoBack();
        }
    }
}
