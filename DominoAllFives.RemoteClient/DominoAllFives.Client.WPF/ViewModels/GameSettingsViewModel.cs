using System.Windows;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Presentation retained for a future remote use case implementation.</summary>
    public class GameSettingsViewModel : ViewModelBase
    {
        public ViewModelBase CurrentModal { get; set; }
        public bool IsModalVisible { get; set; }
        public bool IsAccountSectionVisible { get; set; }
        public bool IsLanguageSectionVisible { get; set; }
        public string SelectedLanguageCode { get; set; } = string.Empty;
        public bool IsSpanishSelected { get; set; }
        public bool IsEnglishSelected { get; set; }
        public bool IsPortugueseSelected { get; set; }
        public bool IsGuest { get; set; }
        public Visibility AccountFeaturesVisibility { get; set; }
        public RelayCommand ShowAccountSectionCommand { get; }
        public RelayCommand ShowLanguageSectionCommand { get; }
        public RelayCommand SelectLanguageCommand { get; }
        public RelayCommand SaveLanguageCommand { get; }
        public RelayCommand GoBackCommand { get; }
        public RelayCommand LogoutCommand { get; }
        public RelayCommand GoToChangePassword { get; }
        public RelayCommand GoToDeleteAccount { get; }

        public GameSettingsViewModel(IFrameNavigationService navigationService)
        {
            ShowAccountSectionCommand = new RelayCommand(() => { }, () => false);
            ShowLanguageSectionCommand = new RelayCommand(() => { }, () => false);
            SelectLanguageCommand = new RelayCommand(() => { }, () => false);
            SaveLanguageCommand = new RelayCommand(() => { }, () => false);
            GoBackCommand = new RelayCommand(() => navigationService.GoBack());
            LogoutCommand = new RelayCommand(() => { }, () => false);
            GoToChangePassword = new RelayCommand(() => { }, () => false);
            GoToDeleteAccount = new RelayCommand(() => { }, () => false);
        }
    }
}
