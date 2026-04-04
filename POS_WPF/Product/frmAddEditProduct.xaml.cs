using POS_WPF.Category;
using POS_WPF.Models;
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

namespace POS_WPF.Product
{
    /// <summary>
    /// Interaction logic for frmAddEditProduct.xaml
    /// </summary>
    public partial class frmAddEditProduct : Window
    {
        public frmAddEditProduct()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            
            MessageBox.Show("Product saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void cmbWarehouse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnAddWarehouse_Click(object sender, RoutedEventArgs e)
        {
            frmAddEditWarehouse frmAddEditWarehouse = new frmAddEditWarehouse();
            frmAddEditWarehouse.ShowDialog();
        }


        private void btnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            frmAddEditCategory frmAddEditCategory = new frmAddEditCategory();
            frmAddEditCategory.ShowDialog();
        }

        private void btnAddModel_Click(object sender, RoutedEventArgs e)
        {
            frmAddEditModel frmAddEditModel = new frmAddEditModel();
            frmAddEditModel.ShowDialog();
        }


    }
}
