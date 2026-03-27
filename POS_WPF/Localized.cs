using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace POS_WPF
{
    public class Localized : INotifyPropertyChanged
    {
        private readonly ResourceManager _rm;
        public event PropertyChangedEventHandler PropertyChanged;

        public Localized(ResourceManager rm)
        {
            _rm = rm;
        }

        public string this[string key] => _rm.GetString(key, CultureInfo.CurrentUICulture);

        public void Refresh()
        {
            // NotifyPropertyChanged لكل keys دفعة وحدة
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }
    }
}