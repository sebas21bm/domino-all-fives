using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Navigation;
using DominoAllFives.Client.WPF.ViewModels;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Client.WPF.Views;

namespace DominoAllFives.Client.WPF.Services
{
    public class FrameNavigationService : IFrameNavigationService
    {
        private readonly Frame _navigationFrame;
        private readonly Dictionary<Type, Type> _viewModelToPageMap;
        private ViewModelFactory _viewModelFactory;

        public bool CanGoBack
        {
            get
            {
                return _navigationFrame != null && _navigationFrame.CanGoBack;
            }
        }

        public FrameNavigationService(Frame navigationFrame)
        {
            _navigationFrame = navigationFrame
                ?? throw new ArgumentNullException(nameof(navigationFrame));
            _viewModelToPageMap = new Dictionary<Type, Type>();
            RegisterRoutes();
        }

        public void GoBack()
        {
            if (_navigationFrame != null && _navigationFrame.CanGoBack)
            {
                _navigationFrame.GoBack();
            }
        }

        public void NavigateTo<TViewModel>()
            where TViewModel : ViewModelBase
        {
            NavigateTo<TViewModel>(null);
        }

        public void NavigateTo<TViewModel>(object parameter)
            where TViewModel : ViewModelBase
        {
            Page pageInstance = CreatePage<TViewModel>(parameter);
            _navigationFrame.Navigate(pageInstance);
        }

        public void GoBackToRefresh<TViewModel>()
            where TViewModel : ViewModelBase
        {
            if (!_navigationFrame.CanGoBack)
            {
                return;
            }

            Page pageInstance = CreatePage<TViewModel>();
            _navigationFrame.Navigated += CompleteRefresh;
            _navigationFrame.Navigate(pageInstance);
        }

        public void NavigateAsRoot<TViewModel>()
            where TViewModel : ViewModelBase
        {
            Page pageInstance = CreatePage<TViewModel>();
            _navigationFrame.Navigated += ClearHistory;
            _navigationFrame.Navigate(pageInstance);
        }

        public void SetViewModelFactory(ViewModelFactory viewModelFactory)
        {
            _viewModelFactory = viewModelFactory
                ?? throw new ArgumentNullException(nameof(viewModelFactory));
        }

        private TViewModel CreateViewModel<TViewModel>(object parameter = null)
            where TViewModel : ViewModelBase
        {
            Type viewModelType = typeof(TViewModel);
            ViewModelBase viewModelInstance;
            try
            {
                if (parameter != null)
                {
                    viewModelInstance = (ViewModelBase)Activator.CreateInstance(
                        viewModelType,
                        this,
                        parameter);
                }
                else
                {
                    if (_viewModelFactory == null)
                    {
                        throw new InvalidOperationException(
                            "The ViewModelFactory has not been configured.");
                    }

                    viewModelInstance = _viewModelFactory.Create<TViewModel>();
                }
            }
            catch (MissingMethodException ex)
            {
                throw new InvalidOperationException(
                    $"No matching navigation constructor was found for {viewModelType.Name}.",
                    ex);
            }

            return (TViewModel)viewModelInstance;
        }

        private Page CreatePage<TViewModel>(object parameter = null)
            where TViewModel : ViewModelBase
        {
            Type viewModelType = typeof(TViewModel);
            if (!_viewModelToPageMap.TryGetValue(viewModelType, out Type pageType))
            {
                string viewModelName = viewModelType.FullName;
                throw new InvalidOperationException(
                    $"No page is registered for the ViewModel type {viewModelName}.");
            }

            TViewModel viewModelInstance = CreateViewModel<TViewModel>(parameter);
            Page pageInstance = (Page)Activator.CreateInstance(pageType);
            pageInstance.DataContext = viewModelInstance;
            return pageInstance;
        }

        private void RegisterRoutes()
        {
            _viewModelToPageMap.Add(typeof(MatchViewModel), typeof(Match));
            _viewModelToPageMap.Add(typeof(HomePageViewModel), typeof(HomePage));
            _viewModelToPageMap.Add(typeof(MainMenuViewModel), typeof(MainMenu));
            _viewModelToPageMap.Add(typeof(GameSettingsViewModel), typeof(GameSettings));
            _viewModelToPageMap.Add(typeof(ProfileViewModel), typeof(Profile));
            _viewModelToPageMap.Add(typeof(FriendsViewModel), typeof(Friends));
            _viewModelToPageMap.Add(typeof(HowToPlayViewModel), typeof(HowToPlay));
            _viewModelToPageMap.Add(typeof(RankingsViewModel), typeof(Rankings));
            _viewModelToPageMap.Add(typeof(CreateGameViewModel), typeof(CreateGame));
            _viewModelToPageMap.Add(typeof(JoinGameViewModel), typeof(JoinGame));
            _viewModelToPageMap.Add(typeof(LobbyViewModel), typeof(Lobby));
            _viewModelToPageMap.Add(typeof(EditProfileViewModel), typeof(EditProfile));
            _viewModelToPageMap.Add(typeof(MatchHistoryViewModel), typeof(MatchHistory));
        }

        private void CompleteRefresh(object sender, NavigationEventArgs eventArgs)
        {
            _navigationFrame.Navigated -= CompleteRefresh;
            _navigationFrame.RemoveBackEntry();
            if (_navigationFrame.CanGoBack)
            {
                _navigationFrame.RemoveBackEntry();
            }
        }

        private void ClearHistory(object sender, NavigationEventArgs eventArgs)
        {
            _navigationFrame.Navigated -= ClearHistory;
            while (_navigationFrame.CanGoBack)
            {
                _navigationFrame.RemoveBackEntry();
            }
        }
    }
}
