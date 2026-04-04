using POS_BLL;
using POS_WPF.Category;
using POS_WPF.Controls;
using POS_WPF.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace POS_WPF.Product
{
    public partial class frmAddEditProduct : Window
    {
        public Action<int, string> OnProductSaved;
        public bool IsSaved { get; private set; } = false;

        private bool _isLoadingForm = false;

        enum enMode { AddNew = 1, Update = 2 }
        enMode FormMode = enMode.AddNew;

        clsProduct _Product;
        int _ProductID = -1;
        int _WarehouseID = -1; // You may want to pass this in constructor if needed for loading product in edit mode

        // ============================
        // CONSTRUCTORS
        // ============================
        public frmAddEditProduct()
        {
            InitializeComponent();
            FormMode = enMode.AddNew;
        }

        public frmAddEditProduct(int productID , int warehouseID)
        {
            InitializeComponent();
            FormMode = enMode.Update;
            _ProductID = productID;
            _WarehouseID = warehouseID;
        }

        // ============================
        // WINDOW LOADED
        // ============================
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ResetDefaultValues();

            _isLoadingForm = true;

            LoadWarehousesToComboBox();
            LoadCategoriesToComboBox();
            LoadModelsToCombBox();

            if (FormMode == enMode.Update)
                LoadData();

            _isLoadingForm = false;
        }

        // ============================
        // RESET
        // ============================
        private void ResetDefaultValues()
        {
            if (FormMode == enMode.AddNew)
            {
                _Product = new clsProduct();
                txtbTitle.Text = "Add New Product";

                ProductName.Text = "";
                ProductDescription.Text = "";
                ProductPrice.Text = "";
                ProductQuantity.Text = "";
            }
            else
            {
                txtbTitle.Text = "Edit Product";
            }
        }

        // ============================
        // LOAD DATA (Edit mode)
        // ============================
        private void LoadData()
        {
            // You need a warehouse context to find the product — adjust if needed
            // For now we load just the base product fields
             _Product = clsProduct.FindByIDInWarehouse(_ProductID, _WarehouseID);

            // If you have a FindByID without warehouse, use that here.
            // For now, leave as placeholder:
            if (_Product == null)
            {
                MessageBox.Show("Product record not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            ProductName.Text = _Product.ProductName;
            ProductDescription.Text = _Product.Description;
            ProductPrice.Text = _Product.Price.ToString();
            ProductQuantity.Text = _Product.Quantity.ToString();

            foreach (ComboBoxItem item in cmbWarehouse.Items)
            {
                if ((int)item.Tag == _Product.WarehouseID)
                {
                    cmbWarehouse.SelectedItem = item;
                    break;
                }
            }

            foreach (ComboBoxItem item in cmbCategory.Items)
            {
                if ((int)item.Tag == _Product.CategoryID)
                {
                    cmbCategory.SelectedItem = item;
                    break;
                }
            }

            foreach (ComboBoxItem item in cmbModel.Items)
            {
                if ((int)item.Tag == _Product.ModelID)
                {
                    cmbModel.SelectedItem = item;
                    break;
                }
            }
        }

        // ============================
        // LOADERS
        // ============================
        private void LoadWarehousesToComboBox()
        {
            try
            {
                DataTable dt = clsWareHouse.GetAll();
                cmbWarehouse.Items.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    cmbWarehouse.Items.Add(new ComboBoxItem
                    {
                        Content = row["Name"].ToString(),
                        Tag = Convert.ToInt32(row["WarehouseID"])
                    });
                }
                if (cmbWarehouse.Items.Count > 0)
                    cmbWarehouse.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading warehouses:\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCategoriesToComboBox()
        {
            try
            {
                DataTable dt = clsCategory.GetAll();
                cmbCategory.Items.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    cmbCategory.Items.Add(new ComboBoxItem
                    {
                        Content = row["Name"].ToString(),
                        Tag = Convert.ToInt32(row["CategoryID"])
                    });
                }
                if (cmbCategory.Items.Count > 0)
                    cmbCategory.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading categories:\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadModelsToCombBox()
        {
            try
            {
                DataTable dt = clsModel.GetAll();
                cmbModel.Items.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    cmbModel.Items.Add(new ComboBoxItem
                    {
                        Content = row["Name"].ToString(),
                        Tag = Convert.ToInt32(row["ModelID"])
                    });
                }
                if (cmbModel.Items.Count > 0)
                    cmbModel.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading models:\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================
        // LIVE VALIDATION
        // ============================
        private void ProductNameInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            //ValidateInput(sender as ModernInput,
            //    "This product name already exists.",
            //    name => clsProduct.IsProductExistsByName(name),
            //    name => clsProduct.IsProductExistsByNameExcludingID(name, _ProductID));
        }

        private void ValidateInput(ModernInput control, string errorMessage,
            Func<string, bool> existsFunc, Func<string, bool> existsExceptIdFunc)
        {
            if (_isLoadingForm) return;
            if (control == null) return;
            if (string.IsNullOrWhiteSpace(control.Text)) return;

            control.Validate(live: true, externalValidator: text =>
            {
                text = text.Trim();
                if (FormMode == enMode.Update)
                {
                    if (existsExceptIdFunc != null && existsExceptIdFunc(text))
                        return errorMessage;
                }
                else
                {
                    if (existsFunc(text))
                        return errorMessage;
                }
                return null;
            });
        }

        // ============================
        // PROCESS & SAVE
        // ============================
        private void ProcessFormData()
        {
            _Product.ProductName = ProductName.Text.Trim();
            _Product.Description = ProductDescription.Text.Trim();
            _Product.Price = decimal.Parse(ProductPrice.Text.Trim());
            _Product.Quantity = int.Parse(ProductQuantity.Text.Trim());

            _Product.CategoryID = cmbCategory.SelectedItem is ComboBoxItem cat
                ? (int)cat.Tag : -1;

            _Product.ModelID = cmbModel.SelectedItem is ComboBoxItem mod
                ? (int)mod.Tag : -1;

            _Product.WarehouseID = cmbWarehouse.SelectedItem is ComboBoxItem wh
                ? (int)wh.Tag : -1;

            // Quantity via reflection workaround since it's private set
            // You may want to add a SetQuantity method or make it settable
            // _Product.Quantity = int.Parse(ProductQuantity.Text.Trim());

            if (_Product.Save())
            {
                OnProductSaved?.Invoke(_Product.ProductID, _Product.ProductName);

                if (FormMode == enMode.AddNew)
                {
                    MessageBox.Show("New product added successfully.",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    IsSaved = true;
                    FormMode = enMode.Update;
                    txtbTitle.Text = "Edit Product";
                    _ProductID = _Product.ProductID;
                }
                else
                {
                    MessageBox.Show("Product updated successfully.",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    IsSaved = true;
                }
            }
            else
            {
                MessageBox.Show("Error saving product record.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================
        // VALIDATION
        // ============================
        private ValidationResult ValidateAllFields()
        {
            var result = new ValidationResult { IsValid = true };
            var errors = new List<string>();

            // Product Name
            //ProductName.ValidateForce();
            //ProductName.Validate(live: false, externalValidator: text =>
            //{
            //    if (FormMode == enMode.Update)
            //    {
            //        if (clsProduct.IsProductExistsByNameExcludingID(text.Trim(), _ProductID))
            //            return "This product name already exists.";
            //    }
            //    else
            //    {
            //        if (clsProduct.IsProductExistsByName(text.Trim()))
            //            return "This product name already exists.";
            //    }
            //    return null;
            //});

            if (!ProductName.IsValid)
            {
                errors.Add($"• {ProductName.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = ProductName;
            }

            // Price
            ProductPrice.ValidateForce();
            if (!ProductPrice.IsValid)
            {
                errors.Add($"• {ProductPrice.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = ProductPrice;
            }
            else if (!decimal.TryParse(ProductPrice.Text, out decimal price) || price <= 0)
            {
                errors.Add("• Price must be a valid number greater than zero.");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = ProductPrice;
            }

            // Quantity
            ProductQuantity.ValidateForce();
            if (!ProductQuantity.IsValid)
            {
                errors.Add($"• {ProductQuantity.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = ProductQuantity;
            }
            else if (!int.TryParse(ProductQuantity.Text, out int qty) || qty < 0)
            {
                errors.Add("• Quantity must be a valid non-negative number.");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = ProductQuantity;
            }

            // Warehouse
            if (cmbWarehouse.SelectedItem == null)
            {
                errors.Add("• Please select a warehouse.");
            }

            // Category
            if (cmbCategory.SelectedItem == null)
            {
                errors.Add("• Please select a category.");
            }

            if (errors.Any())
            {
                result.IsValid = false;
                result.Errors = errors;
            }

            return result;
        }

        // ============================
        // SAVE CLICK
        // ============================
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            HideMessages();

            ValidationResult validationResults = ValidateAllFields();

            if (validationResults.IsValid)
            {
                ShowSuccessMessage();
                ProcessFormData();
            }
            else
            {
                ShowErrorMessage(validationResults.Errors);
                ScrollToFirstError(validationResults.FirstInvalidControl);
            }
        }

        // ============================
        // MESSAGE HELPERS
        // ============================
        private void ShowErrorMessage(List<string> errors)
        {
            ErrorMessageText.Text = string.Join("\n", errors);
            ErrorMessageBox.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            ErrorMessageBox.BeginAnimation(OpacityProperty, fadeIn);

            FindScrollViewer(this)?.ScrollToTop();
        }

        private void ShowSuccessMessage()
        {
            SuccessMessageBox.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            SuccessMessageBox.BeginAnimation(OpacityProperty, fadeIn);

            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(8)
            };
            timer.Tick += (s, ev) => { HideMessages(); timer.Stop(); };
            timer.Start();

            FindScrollViewer(this)?.ScrollToTop();
        }

        private void HideMessages()
        {
            ErrorMessageBox.Visibility = Visibility.Collapsed;
            SuccessMessageBox.Visibility = Visibility.Collapsed;
        }

        private void ScrollToFirstError(FrameworkElement control)
        {
            control?.BringIntoView();
        }

        private ScrollViewer FindScrollViewer(DependencyObject obj)
        {
            if (obj is ScrollViewer sv) return sv;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var result = FindScrollViewer(VisualTreeHelper.GetChild(obj, i));
                if (result != null) return result;
            }
            return null;
        }

        // ============================
        // HEADER & WINDOW
        // ============================
        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();

        // ============================
        // COMBO SELECTION CHANGED
        // ============================
        private void cmbWarehouse_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        // ============================
        // ADD BUTTONS
        // ============================
        private void btnAddWarehouse_Click(object sender, RoutedEventArgs e)
        {
            frmAddEditWarehouse frm = new frmAddEditWarehouse();
            frm.ShowDialog();
            LoadWarehousesToComboBox();
        }

        private void btnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            frmAddEditCategory frm = new frmAddEditCategory();
            frm.ShowDialog();
            LoadCategoriesToComboBox();
        }

        private void btnAddModel_Click(object sender, RoutedEventArgs e)
        {
            frmAddEditModel frm = new frmAddEditModel();
            frm.ShowDialog();
            LoadModelsToCombBox();
        }

        // ============================
        // VALIDATION RESULT CLASS
        // ============================
        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; }
            public FrameworkElement FirstInvalidControl { get; set; }
        }
    }
}