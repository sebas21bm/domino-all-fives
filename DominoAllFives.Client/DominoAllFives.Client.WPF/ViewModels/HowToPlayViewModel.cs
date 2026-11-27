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
            _navigationService = navigationService;
        }

        private void ExecuteNavigateBack()
        {
            _navigationService.GoBack();
        }
    }
}
