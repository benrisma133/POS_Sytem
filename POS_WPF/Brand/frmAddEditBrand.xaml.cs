using Microsoft.Extensions.Logging;
using POS_BLL;
using POS_WPF.Category;
using POS_WPF.Controls;
using POS_WPF.Pages;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POS_WPF.Brand
{
    /// <summary>
    /// Interaction logic for frmAddEditBrand.xaml
    /// </summary>
    public partial class frmAddEditBrand : Window
    {

        enum enMode { AddNew = 1, Update = 2 }
        enMode FormMode = enMode.AddNew;

        public bool IsSaved { get; private set; } = false;

        private readonly ILogger _logger = AppLogger.CreateLogger<frmAddEditBrand>();

        private bool _isLoadingForm = false;

        int _BrandID;
        clsBrand _Brand;

        private List<int> _selectedWarehouseIDs = new List<int>();

        private bool _isLoadingWarehouses = false;

        public frmAddEditBrand()
        {
            InitializeComponent();

            FormMode = enMode.AddNew;
        }

        public frmAddEditBrand(int BrandID)
        {
            InitializeComponent();

            _BrandID = BrandID;

            FormMode = enMode.Update;
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void LoadWarehousesToListBox()
        {
            try
            {
                DataTable dt = clsWareHouse.GetAll(); // BLL method

                lstWarehouses.Items.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    ListBoxItem item = new ListBoxItem
                    {
                        Content = row["Name"].ToString(),
                        Tag = Convert.ToInt32(row["WarehouseID"])
                    };

                    lstWarehouses.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error loading warehouses:\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadSelectedWarehouses()
        {
            _isLoadingWarehouses = true;

            var ids = _Brand.GetAssignedWarehouseIDs();

            foreach (ListBoxItem item in lstWarehouses.Items)
                item.IsSelected = ids.Contains((int)item.Tag);

            _isLoadingWarehouses = false;

            int total = lstWarehouses.Items.Count;
            int selected = lstWarehouses.SelectedItems.Count;

            if (selected == total && total > 0)
            {
                cmbWarehouseFilter.SelectedIndex = 0;
                grdSelectWarehouses.Visibility = Visibility.Collapsed;
                txtSelectedWarehouses.Text = "All Warehouses selected";
            }
            else
            {
                cmbWarehouseFilter.SelectedIndex = 1;
                grdSelectWarehouses.Visibility = Visibility.Visible;
                txtSelectedWarehouses.Text =
                    selected == 0
                        ? "No warehouse selected"
                        : $"{selected} warehouse(s) selected";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _ResetDefaultValues();

            LoadWarehousesToListBox();

            _isLoadingForm = true;
            _isLoadingWarehouses = true;

            if (FormMode == enMode.AddNew)
            {
                // ADD: start with All
                cmbWarehouseFilter.SelectedIndex = 0;
                grdSelectWarehouses.Visibility = Visibility.Collapsed;
                txtSelectedWarehouses.Text = "All Warehouses selected";
            }
            else
            {
                // UPDATE: load actual selection
                _LoadData();
            }

            _isLoadingWarehouses = false;
            _isLoadingForm = false;

        }


        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ValidateInput(ModernInput control, string errorMessage,
            Func<string, bool> existsFunc, Func<string, bool> existsExceptIdFunc)
        {
            if (_isLoadingForm) return;
            if (control == null) return;

            if (string.IsNullOrWhiteSpace(control.Text))
                return;

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

        private void BrandNameInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ValidateInput(sender as ModernInput,
                "Category name already exists.",
                text => clsBrand.IsBrandExistByName(text),
                text => clsBrand.IsBrandExistByName(text, _BrandID)
            );
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void _LoadData()
        {
            _Brand = clsBrand.FindByID(_BrandID);

            if (_Brand == null)
            {
                MessageBox.Show("Brand record not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            BrandName.Text = _Brand.Name;
            BrandDescription.Text = _Brand.Description;

            LoadSelectedWarehouses();
        }

        void _ResetDefaultValues()
        {
            if (FormMode == enMode.AddNew)
            {
                _Brand = new clsBrand();
                txtbTitle.Text = "Add New Brand";

                BrandName.Text = "";
                BrandDescription.Text = "";
            }
            else
            {
                txtbTitle.Text = "Edit Brand";
            }
        }

        private bool ProcessFormData()
        {
            // Collect form data
            _Brand.Name = BrandName.Text.Trim();
            _Brand.Description = BrandDescription.Text;

            // Try to save brand
            try
            {
                if (!_Brand.Save())
                {
                    _logger.LogWarning("Failed to save brand: {BrandName}", _Brand.Name);
                    MessageBox.Show(
                        "Failed to save brand: " + _Brand.Name,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return false;
                }

                // Optional: handle warehouse assignment
                List<int> warehouseIDsToUse = (cmbWarehouseFilter.SelectedIndex == 0)
                    ? clsWareHouse.GetAllWarehouseIDs()
                    : _selectedWarehouseIDs;

                if (FormMode == enMode.AddNew)
                {
                    if (warehouseIDsToUse.Count > 0 &&
                        !_Brand.AssignToWarehouses(warehouseIDsToUse))
                    {
                        MessageBox.Show(
                            "Brand saved but failed to assign warehouses.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                    }


                    IsSaved = true;
                }
                else if (FormMode == enMode.Update)
                {
                    if (!_Brand.UpdateWarehouses(warehouseIDsToUse))
                    {
                        MessageBox.Show(
                            "Brand updated but failed to update warehouses.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error saving brand: {BrandName}", _Brand.Name);
                MessageBox.Show(
                    "حدث خطأ غير متوقع أثناء حفظ العلامة التجارية. المرجو الاتصال بالدعم.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return false;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            HideMessages();

            ValidationResult validationResults = ValidateAllFields();

            if (!validationResults.IsValid)
            {
                ShowErrorMessage(validationResults.Errors);
                ScrollToFirstError(validationResults.FirstInvalidControl);
                return;
            }

            // Validation passed — now try to save
            bool saved = ProcessFormData();

            if (saved)
            {
                ShowSuccessMessage();


            }
        }

        private void cmbWarehouseFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingWarehouses) return;
            if (lstWarehouses == null) return;

            grdSelectWarehouses.Visibility =
                cmbWarehouseFilter.SelectedIndex == 0
                    ? Visibility.Collapsed
                    : Visibility.Visible;

            // -------- ADD MODE ONLY --------
            if (FormMode == enMode.AddNew)
            {
                _isLoadingWarehouses = true;

                if (cmbWarehouseFilter.SelectedIndex == 0) // All
                {
                    foreach (ListBoxItem i in lstWarehouses.Items)
                        i.IsSelected = true;

                    txtSelectedWarehouses.Text = "All Warehouses selected";
                }
                else // Select Warehouses
                {
                    foreach (ListBoxItem i in lstWarehouses.Items)
                        i.IsSelected = false;

                    txtSelectedWarehouses.Text = "No warehouse selected";
                }

                _isLoadingWarehouses = false;
            }

            // -------- UPDATE MODE --------
            if (FormMode == enMode.Update)
            {
                int totalCount = lstWarehouses.Items.Count;
                int selectedCount = lstWarehouses.SelectedItems.Count;

                if (selectedCount == totalCount && totalCount > 0)
                {
                    txtSelectedWarehouses.Text = "All Warehouses selected";
                }
                else if (selectedCount == 0)
                {
                    txtSelectedWarehouses.Text = "No warehouse selected";
                }
                else if (cmbWarehouseFilter.SelectedIndex == 0)
                {
                    foreach (ListBoxItem i in lstWarehouses.Items)
                        i.IsSelected = true;

                    txtSelectedWarehouses.Text = "All Warehouses selected";
                }
                else
                {
                    txtSelectedWarehouses.Text = $"{selectedCount} warehouse(s) selected";
                }

            }

        }

        private void lstWarehouses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingWarehouses) return;

            int total = lstWarehouses.Items.Count;
            int selected = lstWarehouses.SelectedItems.Count;

            if (selected == 0)
            {
                txtSelectedWarehouses.Text = "No warehouse selected";
            }
            else if (selected == total)
            {
                txtSelectedWarehouses.Text = "All Warehouses selected";
            }
            else
            {
                txtSelectedWarehouses.Text = $"{selected} warehouse(s) selected";
            }

            _selectedWarehouseIDs = lstWarehouses.SelectedItems
                .Cast<ListBoxItem>()
                .Select(i => (int)i.Tag)
                .ToList();
        }


        private ValidationResult ValidateAllFields()
        {
            var result = new ValidationResult { IsValid = true };
            var errors = new List<string>();


            // Force validation
            BrandName.ValidateForce();

            if (!BrandName.IsValid)
            {
                errors.Add($"• {BrandName.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = BrandName;
            }

            // Force validation with duplicate check
            BrandName.Validate(live: false, externalValidator: text =>
            {
                if (FormMode == enMode.Update)
                {
                    if (clsBrand.IsBrandExistByName(text.Trim(), _BrandID))
                        return "This brand name already exists.";
                }
                else
                {
                    if (clsBrand.IsBrandExistByName(text.Trim()))
                        return "This brand name already exists.";
                }

                return null; // no errors
            });


            // Validate Bio - FORCE validation
            BrandDescription.ValidateForce();
            if (!BrandDescription.IsValid)
            {
                errors.Add($"• {BrandDescription.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = BrandDescription;
            }


            // Set result
            if (errors.Any())
            {
                result.IsValid = false;
                result.Errors = errors;
            }

            return result;
        }

        private void ShowErrorMessage(List<string> errors)
        {
            ErrorMessageText.Text = string.Join("\n", errors);
            ErrorMessageBox.Visibility = Visibility.Visible;

            // Animate in
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = System.TimeSpan.FromMilliseconds(300)
            };
            ErrorMessageBox.BeginAnimation(OpacityProperty, fadeIn);

            // Optional: Scroll to top to show error
            var scrollViewer = FindScrollViewer(this);
            scrollViewer?.ScrollToTop();
        }

        private void ShowSuccessMessage()
        {
            SuccessMessageBox.Visibility = Visibility.Visible;

            // Animate in
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = System.TimeSpan.FromMilliseconds(300)
            };
            SuccessMessageBox.BeginAnimation(OpacityProperty, fadeIn);

            // Optional: Auto-hide after 5 seconds
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = System.TimeSpan.FromSeconds(8)
            };
            timer.Tick += (s, e) =>
            {
                HideMessages();
                timer.Stop();
            };
            timer.Start();

            // Scroll to top to show success
            var scrollViewer = FindScrollViewer(this);
            scrollViewer?.ScrollToTop();
        }

        private void HideMessages()
        {
            ErrorMessageBox.Visibility = Visibility.Collapsed;
            SuccessMessageBox.Visibility = Visibility.Collapsed;
        }

        private void ScrollToFirstError(FrameworkElement control)
        {
            if (control != null)
            {
                control.BringIntoView();
            }
        }

        private ScrollViewer FindScrollViewer(DependencyObject obj)
        {
            if (obj is ScrollViewer scrollViewer)
                return scrollViewer;

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);
                var result = FindScrollViewer(child);
                if (result != null)
                    return result;
            }

            return null;
        }

        // Helper class for validation results
        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; }
            public FrameworkElement FirstInvalidControl { get; set; }
        }
    }
}
