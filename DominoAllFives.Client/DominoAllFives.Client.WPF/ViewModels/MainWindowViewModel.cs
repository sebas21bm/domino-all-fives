using System;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private IFrameNavigationService _navigationService;
        private bool _isDialogVisible;
        private CustomMessageViewModel _currentDialogViewModel;
        public IFrameNavigationService Navigation => _navigationService;
        public IDialogService DialogService { get; }
        public bool IsDialogVisible { get =>
            _isDialogVisible; set =>
            SetProperty(
            ref _isDialogVisible,
            value); }
        public CustomMessageViewModel CurrentDialogViewModel { get =>
            _currentDialogViewModel; set =>
            SetProperty(
            ref _currentDialogViewModel,
            value); }

        public MainWindowViewModel()
        {
            DialogService = new DialogService(ShowDialog);
        }

        public void Initialize(IFrameNavigationService navigationService)
        {
            if (_navigationService != null)
            {
                throw new InvalidOperationException(
                    "MainWindowViewModel has already been initialized.");
            }

            _navigationService = navigationService ?? throw new ArgumentNullException(
                nameof(
                navigationService));
        }

        private void ShowDialog(CustomMessageViewModel dialogViewModel)
        {
            CurrentDialogViewModel = dialogViewModel;
            IsDialogVisible = dialogViewModel != null;
        }
    }
}
