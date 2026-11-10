using System;
using System.Collections.ObjectModel;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Presentation retained for a future remote use case implementation.</summary>
    public class MatchHistoryViewModel : ViewModelBase
    {
        private IFrameNavigationService _navigationService;
        public RelayCommand GoBackCommand { get; }
        public ObservableCollection<object> MatchHistoryList { get; } = new ObservableCollection<object>();

        public MatchHistoryViewModel()
        {
            GoBackCommand = new RelayCommand(() => _navigationService.GoBack());
        }

        public void Initialize(
            IFrameNavigationService navigationService)
        {
            if (_navigationService != null)
            {
                throw new InvalidOperationException(
                    "MatchHistoryViewModel has already been initialized.");
            }

            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));

            _navigationService = navigationService;
        }
    }
}
