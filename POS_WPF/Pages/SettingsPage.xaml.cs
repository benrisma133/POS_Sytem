using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace POS_WPF.Pages
{
    public partial class SettingsPage : UserControl
    {
        // ─── Track current theme ───────────────────────────────────────
        private bool _isDarkMode = false;

        public SettingsPage()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Restore saved preference if you persist it (optional)
            //_isDarkMode = Properties.Settings.Default.DarkMode;
            //toggleTheme.IsChecked = _isDarkMode;
            //ApplyTheme(_isDarkMode);

            // جلب اللغة المحفوظة
            string savedLanguage = Properties.Settings.Default.AppLanguage;

            // البحث عن العنصر المناسب فـ ComboBox حسب Tag
            foreach (ComboBoxItem item in cmbLanguage.Items)
            {
                if (item.Tag is string culture && culture == savedLanguage)
                {
                    cmbLanguage.SelectedItem = item;
                    break;
                }
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  THEME TOGGLE
        // ──────────────────────────────────────────────────────────────
        private void toggleTheme_Checked(object sender, RoutedEventArgs e)
        {
            _isDarkMode = true;
            ApplyTheme(true);
        }

        private void toggleTheme_Unchecked(object sender, RoutedEventArgs e)
        {
            _isDarkMode = false;
            ApplyTheme(false);
        }

        /// <summary>
        /// Swap app-level resource dictionary colors and update labels.
        /// </summary>
        private void ApplyTheme(bool dark)
        {
            // Update helper labels on this page
            txtThemeLabel.Text = dark ? "Light Mode" : "Dark Mode";
            txtThemeDesc.Text = dark ? "Switch to a lighter interface"
                                      : "Switch to a darker interface";

            // ── Update the Application-level resources ─────────────────
            var res = Application.Current.Resources;

            if (dark)
            {
                // Page / window backgrounds
                res["PageBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A2E"));
                res["CardBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16213E"));
                res["PrimaryText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAEAEA"));
                res["SecondaryText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A4B0BE"));
                res["BorderColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50"));

                // Also update this UserControl's background directly
                this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A2E"));
            }
            else
            {
                res["PageBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F7F7FB"));
                res["CardBackground"] = new SolidColorBrush(Colors.White);
                res["PrimaryText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50"));
                res["SecondaryText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#95A5A6"));
                res["BorderColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CED6E0"));

                this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F7F7FB"));
            }

            // Optional: save preference
            // Properties.Settings.Default.DarkMode = dark;
            // Properties.Settings.Default.Save();
        }

        // ──────────────────────────────────────────────────────────────
        //  LANGUAGE
        // ──────────────────────────────────────────────────────────────
        private void cmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo &&
                    combo.SelectedItem is ComboBoxItem item &&
                    item.Tag is string culture)
            {
                LanguageManager.Instance.ChangeLanguage(culture);

                // Save language
                Properties.Settings.Default.AppLanguage = culture;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Apply a new culture to the current thread.
        /// For full runtime localization, trigger a resource-dictionary swap here.
        /// </summary>
        private void ChangeLanguage(string cultureName)
        {
            var culture = new System.Globalization.CultureInfo(cultureName);
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            // If you use resource dictionaries for localization, swap them here:
            // var dict = new ResourceDictionary
            // {
            //     Source = new Uri($"/Resources/Strings.{cultureName}.xaml", UriKind.Relative)
            // };
            // Application.Current.Resources.MergedDictionaries[langDictIndex] = dict;
        }
    }
}
