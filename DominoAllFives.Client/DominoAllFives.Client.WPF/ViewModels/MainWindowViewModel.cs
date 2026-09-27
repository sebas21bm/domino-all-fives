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
        private bool _isDialogVisible;
        private CustomMessageViewModel _currentDialogViewModel;

        public IFrameNavigationService Navigation => _navigationService;
        public IDialogService DialogService { get; }

        public bool IsDialogVisible
        {
            get => _isDialogVisible;
            set => SetProperty(ref _isDialogVisible, value);
        }

        public CustomMessageViewModel CurrentDialogViewModel
        {
            get => _currentDialogViewModel;
            set => SetProperty(ref _currentDialogViewModel, value);
        }


        public MainWindowViewModel(IFrameNavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            DialogService = new DialogService(dialogVm =>
            {
                CurrentDialogViewModel = dialogVm;
                IsDialogVisible = dialogVm != null;
            });
        }
    }
}
