using System.Windows;
using DominoAllFives.Client.WPF.Localization;

namespace DominoAllFives.Client.WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            LanguageManager.LoadLanguage();

            base.OnStartup(e);
        }
    }
}