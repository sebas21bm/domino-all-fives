using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private IFrameNavigationService _navigationService;
        private bool _isDialogVisible;
        private CustomMessageViewModel _currentDialogViewModel;
        public IFrameNavigationService Navigation
        {
            get
            {
                return _navigationService;
            }
        }
        public IDialogService DialogService { get; }
        public bool IsDialogVisible
        {
            get
            {
                return _isDialogVisible;
            }
            set
            {
                SetProperty(ref _isDialogVisible, value);
            }
        }
        public CustomMessageViewModel CurrentDialogViewModel
        {
            get
            {
                return _currentDialogViewModel;
            }
            set
            {
                SetProperty(ref _currentDialogViewModel, value);
            }
        }

        public MainWindowViewModel()
        {
            DialogService = new DialogService(ShowDialog);
        }

        public void Initialize(IFrameNavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        private void ShowDialog(CustomMessageViewModel dialogViewModel)
        {
            CurrentDialogViewModel = dialogViewModel;
            IsDialogVisible = dialogViewModel != null;
        }
    }
}
