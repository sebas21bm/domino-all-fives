using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.Services
{
    public interface IFrameNavigationService
    {
        bool CanGoBack { get; }

        void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;

        void NavigateTo<TViewModel>(object parameter) where TViewModel : ViewModelBase;

        void GoBack();
    }
}
