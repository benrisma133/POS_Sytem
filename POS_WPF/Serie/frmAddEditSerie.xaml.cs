using Microsoft.Extensions.Logging;
using POS_BLL;
using POS_WPF.Brand;
using POS_WPF.Controls;
using POS_WPF.Pages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace POS_WPF.Serie
{
    public partial class frmAddEditSerie : Window
    {
        enum enMode { AddNew = 1, Update = 2 }
        enMode FormMode = enMode.AddNew;

        public bool IsSaved { get; private set; } = false;

        private readonly ILogger _logger = AppLogger.CreateLogger<frmAddEditSerie>();

        private bool _isLoadingForm = false;

        int _SerieID;
        int _SelectedBrandID = -1;
        clsSeries _Serie;

        private List<int> _selectedWarehouseIDs = new List<int>();

        private bool _isLoadingWarehouses = false;

        public frmAddEditSerie()
        {
            InitializeComponent();
            FormMode = enMode.AddNew;
        }

        public frmAddEditSerie(int SerieID)
        {
            InitializeComponent();

            _SerieID = SerieID;
            FormMode = enMode.Update;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _ResetDefaultValues();

            LoadBrandsToComboBox();
            LoadWarehousesToListBox();

            _isLoadingForm = true;
            _isLoadingWarehouses = true;

            if (FormMode == enMode.AddNew)
            {
                cmbWarehouseFilter.SelectedIndex = 0;
                grdSelectWarehouses.Visibility = Visibility.Collapsed;
                txtSelectedWarehouses.Text = "All Warehouses selected";
            }
            else
            {
                _LoadData();
            }

            _isLoadingWarehouses = false;
            _isLoadingForm = false;
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

        private void LoadBrandsToComboBox()
        {
            try
            {
                cmbBrand.Items.Clear();

                DataTable dt = clsBrand.GetAll();

                foreach (DataRow row in dt.Rows)
                {
                    cmbBrand.Items.Add(new ComboBoxItem
                    {
                        Content = row["Name"].ToString(),
                        Tag = Convert.ToInt32(row["BrandID"])
                    });
                }

                if (cmbBrand.Items.Count > 0)
                    cmbBrand.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading brands: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadWarehousesToListBox()
        {
            try
            {
                DataTable dt = clsWareHouse.GetAll();

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
                MessageBox.Show("Error loading warehouses:\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSelectedWarehouses()
        {
            _isLoadingWarehouses = true;

            var ids = _Serie.GetAssignedWarehouseIDs();

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

        private void _LoadData()
        {
            _Serie = clsSeries.FindByID(_SerieID);

            if (_Serie == null)
            {
                MessageBox.Show("Serie record not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            SerieName.Text = _Serie.Name;
            SerieDescription.Text = _Serie.Description;

            // Select the matching brand in the combo box
            foreach (ComboBoxItem item in cmbBrand.Items)
            {
                if ((int)item.Tag == _Serie.BrandID)
                {
                    cmbBrand.SelectedItem = item;
                    _SelectedBrandID = _Serie.BrandID;
                    break;
                }
            }

            LoadSelectedWarehouses();
        }

        private void _ResetDefaultValues()
        {
            if (FormMode == enMode.AddNew)
            {
                _Serie = new clsSeries();
                txtbTitle.Text = "Add New Serie";

                SerieName.Text = "";
                SerieDescription.Text = "";
            }
            else
            {
                txtbTitle.Text = "Edit Serie";
            }
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

        private void SerieNameInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ValidateInput(sender as ModernInput,
                "Serie name already exists.",
                text => clsSeries.IsSeriesExistByName(text),
                text => clsSeries.IsSeriesExistByName(text, _SerieID)
            );
        }

        private void cmbBrand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbBrand.SelectedItem is ComboBoxItem selected)
                _SelectedBrandID = (int)selected.Tag;
        }

        private void cmbWarehouseFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingWarehouses) return;
            if (lstWarehouses == null) return;

            grdSelectWarehouses.Visibility =
                cmbWarehouseFilter.SelectedIndex == 0
                    ? Visibility.Collapsed
                    : Visibility.Visible;

            if (FormMode == enMode.AddNew)
            {
                _isLoadingWarehouses = true;

                if (cmbWarehouseFilter.SelectedIndex == 0)
                {
                    foreach (ListBoxItem i in lstWarehouses.Items)
                        i.IsSelected = true;

                    txtSelectedWarehouses.Text = "All Warehouses selected";
                }
                else
                {
                    foreach (ListBoxItem i in lstWarehouses.Items)
                        i.IsSelected = false;

                    txtSelectedWarehouses.Text = "No warehouse selected";
                }

                _isLoadingWarehouses = false;
            }

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
                txtSelectedWarehouses.Text = "No warehouse selected";
            else if (selected == total)
                txtSelectedWarehouses.Text = "All Warehouses selected";
            else
                txtSelectedWarehouses.Text = $"{selected} warehouse(s) selected";

            _selectedWarehouseIDs = lstWarehouses.SelectedItems
                .Cast<ListBoxItem>()
                .Select(i => (int)i.Tag)
                .ToList();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool ProcessFormData()
        {
            _Serie.Name = SerieName.Text.Trim();
            _Serie.Description = SerieDescription.Text;
            _Serie.BrandID = _SelectedBrandID;

            try
            {
                if (!_Serie.Save())
                {
                    _logger.LogWarning("Failed to save serie: {SerieName}", _Serie.Name);
                    MessageBox.Show(
                        "Failed to save serie: " + _Serie.Name,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                List<int> warehouseIDsToUse = (cmbWarehouseFilter.SelectedIndex == 0)
                    ? clsWareHouse.GetAllWarehouseIDs()
                    : _selectedWarehouseIDs;

                if (FormMode == enMode.AddNew)
                {
                    if (warehouseIDsToUse.Count > 0 &&
                        !_Serie.AssignToWarehouses(warehouseIDsToUse))
                    {
                        MessageBox.Show(
                            "Serie saved but failed to assign warehouses.",
                            "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    IsSaved = true;
                }
                else if (FormMode == enMode.Update)
                {
                    if (!_Serie.UpdateWarehouses(warehouseIDsToUse))
                    {
                        MessageBox.Show(
                            "Serie updated but failed to update warehouses.",
                            "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    IsSaved = true;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error saving serie: {SerieName}", _Serie.Name);
                MessageBox.Show(
                    "An unexpected error occurred while saving the serie. Please contact support.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            bool saved = ProcessFormData();

            if (saved)
                ShowSuccessMessage();
        }

        private ValidationResult ValidateAllFields()
        {
            var result = new ValidationResult { IsValid = true };
            var errors = new List<string>();

            // Validate Serie Name
            SerieName.ValidateForce();

            if (!SerieName.IsValid)
            {
                errors.Add($"• {SerieName.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = SerieName;
            }

            SerieName.Validate(live: false, externalValidator: text =>
            {
                if (FormMode == enMode.Update)
                {
                    if (clsSeries.IsSeriesExistByName(text.Trim(), _SerieID))
                        return "This serie name already exists.";
                }
                else
                {
                    if (clsSeries.IsSeriesExistByName(text.Trim()))
                        return "This serie name already exists.";
                }

                return null;
            });

            // Validate Brand selection
            if (_SelectedBrandID == -1)
            {
                errors.Add("• Please select a brand.");
            }

            // Validate Description
            SerieDescription.ValidateForce();
            if (!SerieDescription.IsValid)
            {
                errors.Add($"• {SerieDescription.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = SerieDescription;
            }

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
            timer.Tick += (s, e) =>
            {
                HideMessages();
                timer.Stop();
            };
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

        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; }
            public FrameworkElement FirstInvalidControl { get; set; }
        }

        private void btnManageWarehouses_Click(object sender, RoutedEventArgs e)
        {
            frmAddEditBrand frmAdd = new frmAddEditBrand();
            frmAdd.ShowDialog();
            LoadBrandsToComboBox();
        }
    }
}