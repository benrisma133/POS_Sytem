using POS_BLL;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace POS_WPF.Product
{
    public partial class frmTransferProduct : Window
    {
        public int ProductID { get; set; }
        public int WarehouseID { get; set; }
        public bool IsTransferred { get; private set; } = false;

        private int _availableQty = 0;

        public frmTransferProduct(int productId, int warehouseID)
        {
            InitializeComponent();
            ProductID = productId;
            WarehouseID = warehouseID;
        }

        // ============================
        // WINDOW LOADED
        // ============================
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadWarehouses();
        }

        // ============================
        // LOAD WAREHOUSES
        // ============================
        private void LoadWarehouses()
        {
            try
            {
                DataTable dt = clsWareHouse.GetAll();

                cmbFromWarehouse.Items.Clear();
                cmbToWarehouse.Items.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    cmbFromWarehouse.Items.Add(new ComboBoxItem
                    {
                        Content = row["Name"].ToString(),
                        Tag = Convert.ToInt32(row["WarehouseID"])
                    });

                    cmbToWarehouse.Items.Add(new ComboBoxItem
                    {
                        Content = row["Name"].ToString(),
                        Tag = Convert.ToInt32(row["WarehouseID"])
                    });
                }

                // Pre-select the warehouse this product came from
                SelectWarehouseByID(cmbFromWarehouse, WarehouseID);

                // Pre-select the first warehouse that is NOT the source
                SelectFirstDifferentWarehouse(cmbToWarehouse, WarehouseID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading warehouses:\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectWarehouseByID(ComboBox combo, int warehouseID)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if ((int)item.Tag == warehouseID)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
            if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;
        }

        private void SelectFirstDifferentWarehouse(ComboBox combo, int excludeWarehouseID)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if ((int)item.Tag != excludeWarehouseID)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
            if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;
        }

        // ============================
        // SELECTION CHANGED
        // ============================
        private void cmbFromWarehouse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAvailableStock();
            ValidateSameWarehouse();
        }

        private void cmbToWarehouse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateSameWarehouse();
        }

        // ============================
        // AVAILABLE STOCK
        // ============================
        private void UpdateAvailableStock()
        {
            if (cmbFromWarehouse.SelectedItem == null) return;

            int warehouseId = (int)((ComboBoxItem)cmbFromWarehouse.SelectedItem).Tag;
            _availableQty = clsProduct.GetQuantityInWarehouse(ProductID, warehouseId);

            txtAvailableStock.Text = _availableQty > 0
                ? $"Available: {_availableQty} units"
                : "No stock available in this warehouse";

            txtAvailableStock.Foreground = _availableQty > 0
                ? new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(107, 114, 128))  // gray
                : new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(220, 38, 38));   // red

            txtAvailableStock.Visibility = Visibility.Visible;
        }

        private void ValidateSameWarehouse()
        {
            if (cmbFromWarehouse.SelectedItem == null || cmbToWarehouse.SelectedItem == null)
                return;

            int fromId = (int)((ComboBoxItem)cmbFromWarehouse.SelectedItem).Tag;
            int toId = (int)((ComboBoxItem)cmbToWarehouse.SelectedItem).Tag;

            if (fromId == toId)
                ShowError("Source and destination warehouses cannot be the same.");
            else
                HideMessages();
        }

        // ============================
        // TRANSFER CLICK
        // ============================
        private void Transfer_Click(object sender, RoutedEventArgs e)
        {
            HideMessages();

            // 1. Warehouses selected?
            if (cmbFromWarehouse.SelectedItem == null || cmbToWarehouse.SelectedItem == null)
            {
                ShowError("Please select both warehouses.");
                return;
            }

            int fromId = (int)((ComboBoxItem)cmbFromWarehouse.SelectedItem).Tag;
            int toId = (int)((ComboBoxItem)cmbToWarehouse.SelectedItem).Tag;

            // 2. Same warehouse?
            if (fromId == toId)
            {
                ShowError("Source and destination warehouses cannot be the same.");
                return;
            }

            // 3. Valid quantity input?
            if (!int.TryParse(TransferQuantity.Text, out int qty) || qty <= 0)
            {
                ShowError("Please enter a valid quantity greater than zero.");
                return;
            }

            // 4. Enough stock?
            if (qty > _availableQty)
            {
                ShowError($"Not enough stock. Only {_availableQty} units available in the selected warehouse.");
                return;
            }

            // 5. No stock at all?
            if (_availableQty <= 0)
            {
                ShowError("No stock available in the source warehouse to transfer.");
                return;
            }

            // 6. Confirm
            var confirm = MessageBox.Show(
                $"Transfer {qty} unit(s) from '{((ComboBoxItem)cmbFromWarehouse.SelectedItem).Content}' " +
                $"to '{((ComboBoxItem)cmbToWarehouse.SelectedItem).Content}'?",
                "Confirm Transfer",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            // 7. Execute
            try
            {
                clsProduct product = clsProduct.FindByIDInWarehouse(ProductID, fromId);

                if (product == null)
                {
                    ShowError("Product not found in the selected source warehouse.");
                    return;
                }

                product.SetTransferMode(fromId, toId, qty);
                bool success = product.Transfer();

                if (success)
                {
                    IsTransferred = true;
                    ShowSuccess();

                    // Refresh available stock display after transfer
                    _availableQty -= qty;
                    txtAvailableStock.Text = $"Available: {_availableQty} units";
                }
                else
                {
                    ShowError("Transfer failed. The source warehouse may not have enough stock.");
                }
            }
            catch (Exception ex)
            {
                ShowError("An error occurred during transfer:\n" + ex.Message);
            }
        }

        // ============================
        // MESSAGE HELPERS
        // ============================
        private void ShowError(string message)
        {
            SuccessMessageBox.Visibility = Visibility.Collapsed;
            ErrorMessageText.Text = message;
            ErrorMessageBox.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            ErrorMessageBox.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void ShowSuccess()
        {
            ErrorMessageBox.Visibility = Visibility.Collapsed;
            SuccessMessageBox.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            SuccessMessageBox.BeginAnimation(OpacityProperty, fadeIn);

            // Auto-close after 2 seconds on success
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            timer.Tick += (s, ev) => { timer.Stop(); this.Close(); };
            timer.Start();
        }

        private void HideMessages()
        {
            ErrorMessageBox.Visibility = Visibility.Collapsed;
            SuccessMessageBox.Visibility = Visibility.Collapsed;
        }

        // ============================
        // WINDOW CHROME
        // ============================
        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}