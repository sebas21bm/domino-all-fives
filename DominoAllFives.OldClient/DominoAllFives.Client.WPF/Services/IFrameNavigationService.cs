using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.Services
{
    /// <summary>
    /// Defines a contract for a navigation service that allows navigation 
    /// between different views (pages) in a WPF application using a Frame control.
    /// </summary>
    public interface IFrameNavigationService
    {
        bool CanGoBack { get; }

        /// <summary>
        /// Navigates to the view associated with the specified ViewModel type.
        /// Allows to go back to the previous view if CanGoBack is true.
        /// </summary>
        void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;

        /// <summary>
        /// Navigates to the view associated with the specified ViewModel type, 
        /// passing a parameter to the target view.
        /// </summary>
        /// <param name="parameter">The parameter to pass to the target view.</param>
        void NavigateTo<TViewModel>(object parameter) where TViewModel : ViewModelBase;

        /// <summary>
        /// Navigates back while recreating the view associated with the specified
        /// ViewModel type to display updated information.
        /// </summary>
        void GoBackToRefresh<TViewModel>() where TViewModel : ViewModelBase;

        /// <summary>
        /// Navigates to the view associated with the specified ViewModel type,
        /// eliminating the history stack and making the new view the root of the navigation stack.
        /// </summary>
        void NavigateAsRoot<TViewModel>() where TViewModel : ViewModelBase;

        /// <summary>
        /// Navigates back to the previous view in the navigation stack, if possible.
        /// </summary>
        void GoBack();
    }
}
