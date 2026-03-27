using System.Windows;
using System.Windows.Controls;

namespace POS_WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ToggleMenu_Click(object sender, RoutedEventArgs e)
        {
            if (ColSidebar.Width.Value == 220)
                ColSidebar.Width = new GridLength(55);
            else
                ColSidebar.Width = new GridLength(220);
        }

        private void Warehouse_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Children.Clear();
            MainContent.Children.Add(new Pages.WarehousePage());
        }
    }
}
