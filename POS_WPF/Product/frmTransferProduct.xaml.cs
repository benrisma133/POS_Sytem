using POS_BLL;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for frmTransferProduct.xaml
    /// </summary>
    public partial class frmTransferProduct : Window
    {
        // Pass the product ID in when opening this window
        public int ProductID { get; set; }

        public frmTransferProduct(int productId)
        {
            InitializeComponent();
            ProductID = productId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadWarehouses();
        }

        private void LoadWarehouses()
        {
            try
            {
                DataTable dt = clsWareHouse.GetAll();

                cmbFromWarehouse.Items.Clear();
                cmbToWarehouse.Items.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    var fromItem = new ComboBoxItem
                    {
                        Content = row["Name"].ToString(),
                        Tag = Convert.ToInt32(row["WarehouseID"])
                    };
                    var toItem = new ComboBoxItem
                    {
                        Content = row["Name"].ToString(),
                        Tag = Convert.ToInt32(row["WarehouseID"])
                    };
                    cmbFromWarehouse.Items.Add(fromItem);
                    cmbToWarehouse.Items.Add(toItem);
                }

                if (cmbFromWarehouse.Items.Count > 0) cmbFromWarehouse.SelectedIndex = 0;
                if (cmbToWarehouse.Items.Count > 1) cmbToWarehouse.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading warehouses:\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbFromWarehouse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAvailableStock();
        }

        private void cmbToWarehouse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: optionally warn if From == To
        }

        private void UpdateAvailableStock()
        {
            if (cmbFromWarehouse.SelectedItem == null) return;
            ComboBoxItem selected = (ComboBoxItem)cmbFromWarehouse.SelectedItem;
            int warehouseId = (int)selected.Tag;
            // TODO: query available qty for ProductID in warehouseId
            // int available = clsProduct.GetQuantityInWarehouse(ProductID, warehouseId);
            // txtAvailableStock.Text = $"Available: {available} units";
            // txtAvailableStock.Visibility = Visibility.Visible;
        }

        private void Transfer_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessageBox.Visibility = Visibility.Collapsed;
            SuccessMessageBox.Visibility = Visibility.Collapsed;

            if (cmbFromWarehouse.SelectedItem == null || cmbToWarehouse.SelectedItem == null)
            {
                ShowError("Please select both warehouses.");
                return;
            }

            ComboBoxItem from = (ComboBoxItem)cmbFromWarehouse.SelectedItem;
            ComboBoxItem to = (ComboBoxItem)cmbToWarehouse.SelectedItem;

            int fromId = (int)from.Tag;
            int toId = (int)to.Tag;

            if (fromId == toId)
            {
                ShowError("Source and destination warehouses cannot be the same.");
                return;
            }

            if (!int.TryParse(TransferQuantity.Text, out int qty) || qty <= 0)
            {
                ShowError("Please enter a valid quantity greater than zero.");
                return;
            }

            // TODO: call your transfer logic
            // bool success = clsProduct.Transfer(ProductID, fromId, toId, qty);
            // if (success) SuccessMessageBox.Visibility = Visibility.Visible;
            // else ShowError("Transfer failed. Please try again.");
        }

        private void ShowError(string message)
        {
            ErrorMessageText.Text = message;
            ErrorMessageBox.Visibility = Visibility.Visible;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}
