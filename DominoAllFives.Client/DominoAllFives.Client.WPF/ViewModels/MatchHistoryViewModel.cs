using System.Collections.ObjectModel;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Presentation retained for a future remote use case implementation.</summary>
    public class MatchHistoryViewModel : ViewModelBase
    {
        public RelayCommand GoBackCommand { get; }
        public ObservableCollection<object> MatchHistoryList { get; } = new ObservableCollection<object>();

        public MatchHistoryViewModel(IFrameNavigationService navigationService)
        {
            GoBackCommand = new RelayCommand(() => navigationService.GoBack());
        }
    }
}
