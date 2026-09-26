using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;

        public MainMenuViewModel(IFrameNavigationService navigationService)
        {
            _navigationService = navigationService;
        }
    }
}
