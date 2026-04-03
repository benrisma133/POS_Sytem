using POS_DAL;
using POS_WPF.Controls;
using POS_WPF.Serie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POS_WPF
{
    /// <summary>
    /// Interaction logic for WindowTest.xaml
    /// </summary>
    public partial class WindowTest : Window
    {
        public WindowTest()
        {
            InitializeComponent();

            var card = new ProductCard();
            card.LoadProduct(5, 1);
            // Example: productID=1, warehouseID=1
            //MainGrid.Children.Add(card); // Assuming your Grid is named MainGrid
            //string cs = clsDataAccessSettigs.ConnectionString;
            //MessageBox.Show(cs);
        }

        private void ShowGalleryButton_Click(object sender, RoutedEventArgs e)
        {
            frmAddEditSerie frm = new frmAddEditSerie(8);
            frm.ShowDialog();
        }
    }
}
