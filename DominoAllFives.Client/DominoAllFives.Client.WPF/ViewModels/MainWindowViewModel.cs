using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;

        public IFrameNavigationService Navigation => _navigationService;

        public MainWindowViewModel(IFrameNavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        }
    }
}
