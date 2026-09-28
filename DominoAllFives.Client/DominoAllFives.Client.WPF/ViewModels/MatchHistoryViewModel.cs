using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class MatchHistoryViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;

        public RelayCommand GoBackCommand { get; }

        public ObservableCollection<object> MatchHistoryList { get; }

        public MatchHistoryViewModel(IFrameNavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            GoBackCommand = new RelayCommand(ExecuteGoBack);
            MatchHistoryList = new ObservableCollection<object>();
        }

        private void ExecuteGoBack(object parameter)
        {
            _navigationService.GoBack();
        }
    }
}
