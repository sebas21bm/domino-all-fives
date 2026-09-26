using DominoAllFives.Client.WPF.ViewModels;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Client.WPF.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DominoAllFives.Client.WPF.Services
{
    public class FrameNavigationService : IFrameNavigationService
    {
        private readonly Frame _navigationFrame;
        private readonly Dictionary<Type, Type> _viewModelToPageMap;

        public bool CanGoBack => _navigationFrame != null && _navigationFrame.CanGoBack;
        public FrameNavigationService(Frame navigationFrame)
        {
            _navigationFrame = navigationFrame ?? throw new ArgumentNullException(nameof(navigationFrame));
            _viewModelToPageMap = new Dictionary<Type, Type>();

            RegisterRoutes();
        }


        public void GoBack()
        {
            throw new NotImplementedException();
        }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            NavigateTo<TViewModel>(null);
        }

        public void NavigateTo<TViewModel>(object parameter) where TViewModel : ViewModelBase
        {
            Type viewModelType = typeof(TViewModel);

            if (!_viewModelToPageMap.TryGetValue(viewModelType, out Type pageType))
            {
                throw new InvalidOperationException($"No page registered for ViewModel type: {viewModelType.FullName}");
            }

            ViewModelBase viewModelInstance;
            try
            {
                if (parameter != null)
                {
                    viewModelInstance = (ViewModelBase)Activator.CreateInstance(viewModelType, this, parameter);
                }
                else
                {
                    viewModelInstance = (ViewModelBase)Activator.CreateInstance(viewModelType, this);
                }
            }
            catch (MissingMethodException ex)
            {
                throw new InvalidOperationException(
                    $"The ViewModel {viewModelType.Name} must have a constructor accepting IFrameNavigationService.", ex);
            }

            Page pageInstance = (Page)Activator.CreateInstance(pageType);
            pageInstance.DataContext = viewModelInstance;

            _navigationFrame.Navigate(pageInstance);
        }

        private void RegisterRoutes()
        {
            _viewModelToPageMap.Add(typeof(HomePageViewModel), typeof(HomePage));
            _viewModelToPageMap.Add(typeof(MainMenuViewModel), typeof(MainMenu));
            _viewModelToPageMap.Add(typeof(GameSettingsViewModel), typeof(GameSettings));
            _viewModelToPageMap.Add(typeof(ProfileViewModel), typeof(Profile));
            _viewModelToPageMap.Add(typeof(FriendsViewModel), typeof(Friends));
        }
    }
}
