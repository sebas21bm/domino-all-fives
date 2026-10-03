using System;
using System.Windows.Input;

using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class HowToPlayViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;

        public ICommand NavigateBackCommand { get; }

        public HowToPlayViewModel(IFrameNavigationService navigationService)
        {
            _navigationService = navigationService ?? 
                throw new ArgumentNullException(nameof(navigationService));

            NavigateBackCommand = new RelayCommand(ExecuteNavigateBack);
        }

        private void ExecuteNavigateBack()
        {
            _navigationService.GoBack();
        }
    }
}
