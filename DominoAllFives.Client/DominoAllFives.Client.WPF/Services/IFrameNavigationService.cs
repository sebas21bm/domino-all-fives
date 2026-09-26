using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
