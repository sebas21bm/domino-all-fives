using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.Tests
{
    internal sealed class MatchWorkerNavigation : IFrameNavigationService
    {
        public bool CanGoBack => false;
        public int RootNavigations { get; private set; }

        public void NavigateTo<T>() where T : ViewModelBase
        {
        }

        public void NavigateTo<T>(object parameter) where T : ViewModelBase
        {
        }

        public void NavigateAsRoot<T>() where T : ViewModelBase
        {
            RootNavigations++;
        }

        public void GoBackToRefresh<T>() where T : ViewModelBase
        {
        }

        public void GoBack()
        {
        }
    }
}
