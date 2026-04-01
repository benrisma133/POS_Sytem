using POS_WPF.Brand;
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
    /// Interaction logic for frmTest.xaml
    /// </summary>
    public partial class frmTest : Window
    {
        public frmTest()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            frmAddEditBrand updateBrand = new frmAddEditBrand(6);
            updateBrand.ShowDialog();

        }
    }
}
