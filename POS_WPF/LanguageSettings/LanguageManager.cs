
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Resources;
using POS_WPF.Localization;

namespace POS_WPF
{
    public class LanguageManager : INotifyPropertyChanged
    {
        public static LanguageManager Instance { get; } = new LanguageManager();

        public Localized Strings { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private LanguageManager()
        {
            Strings = new Localized(AppResources.ResourceManager);
        }

        public void ChangeLanguage(string cultureCode)
        {
            CultureInfo culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            Strings.Refresh(); // هادي كتبدل جميع bindings اللي فـ Strings[index]
            OnPropertyChanged(nameof(CurrentFlowDirection));
        }

        public FlowDirection CurrentFlowDirection =>
            Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft
                ? FlowDirection.RightToLeft
                : FlowDirection.LeftToRight;

        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
