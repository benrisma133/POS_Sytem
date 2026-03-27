using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace POS_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Use software rendering instead of GPU
            //RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            base.OnStartup(e);

            Batteries.Init(); // ✅ REQUIRED for Microsoft.Data.Sqlite

            string savedLanguage = POS_WPF.Properties.Settings.Default.AppLanguage;

            if (!string.IsNullOrEmpty(savedLanguage))
            {
                LanguageManager.Instance.ChangeLanguage(savedLanguage);
            }
        }
    }
}
