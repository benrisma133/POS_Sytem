using POS_BLL;
using POS_WPF.Controls;
using POS_WPF.Pages;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POS_WPF
{
    /// <summary>
    /// Interaction logic for frmAddEditWarehouse.xaml
    /// </summary>
    public partial class frmAddEditWarehouse : Window
    {
        public Action<int, string> OnWarehouseSaved;

        bool _isLoadingForm = false;

        public string ColorHex { get; set; } = "#FFFFFF";

        public bool ShouldInitialize { get; set; } = true;

        public bool IsSaved { get; private set; } = false;

        private bool _warehouseNameHasError;
        private bool _locationHasError;


        enum enMode { AddNew = 1 , Update = 2 }
        enMode FormMode = enMode.AddNew;

        clsWareHouse _Warehouse;

        int _WarehouseID = -1;
        public frmAddEditWarehouse()
        {
            InitializeComponent();

            FormMode = enMode.AddNew;

        }

        public frmAddEditWarehouse(int WarehouseID)
        {
            InitializeComponent();

            FormMode = enMode.Update;

            _WarehouseID = WarehouseID;
        }



        // Make the window draggable
        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void WarehouseNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateInput(
                sender as ModernInput,
                "This warehouse name already exists.",
                clsWareHouse.IsWarehouseExistByName,
                name => clsWareHouse.IsWarehouseExistByName(name, _WarehouseID)
            );
        }

        private void LocationInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateInput(
                sender as ModernInput,
                "This location already exists.",
                clsWareHouse.IsWarehouseExistByLocation,
                name => clsWareHouse.IsWarehouseExistByLocation(name, _WarehouseID)
            );
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


        private void UpdateColorPreview()
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(txtSelectedColor.Text);
                rectPreview.Fill = new SolidColorBrush(color);

                // Update sliders
                sliderR.Value = color.R;
                sliderG.Value = color.G;
                sliderB.Value = color.B;

                // Update textboxes
                txtR.Text = color.R.ToString();
                txtG.Text = color.G.ToString();
                txtB.Text = color.B.ToString();

                // Hex box
                //txtSelectedColor.Text = vm.ColorHex;
            }
            catch
            {
                // fallback in case of invalid color
                rectPreview.Fill = new SolidColorBrush(Colors.White);
            }
        }

        void ResetDefaultValues()
        {
            if (FormMode == enMode.AddNew)
            {
                _Warehouse = new clsWareHouse();
                txtbTitle.Text = "Add New Warehouse";

                // Initialize fields to empty

                WarehouseName.Text = "";
                Location.Text = "";
                WarehouseDescription.Text = "";
                txtSelectedColor.Text = "#FFFFFF";

                UpdateColorPreview();
            }
            else if (FormMode == enMode.Update)
            {
                txtbTitle.Text = "Edit Warehouse";
            }
        }

        void LoadData()
        {
            _Warehouse = clsWareHouse.FindByID(_WarehouseID);

            if (_Warehouse == null)
            {
                MessageBox.Show("Warehouse record not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            // Load into fields
            WarehouseName.Text = _Warehouse.Name;
            Location.Text = _Warehouse.Location;
            WarehouseDescription.Text = _Warehouse.Description;
            txtSelectedColor.Text = string.IsNullOrWhiteSpace(_Warehouse.Color) ? "#FFFFFF" : _Warehouse.Color;

            UpdateColorPreview();
        }

        void ProcessFormData()
        {
            _Warehouse.Name = WarehouseName.Text.Trim();
            _Warehouse.Location = Location.Text.Trim();
            _Warehouse.Description = WarehouseDescription.Text;
            _Warehouse.Color = txtSelectedColor.Text.Trim(); // save selected color

            if (_Warehouse.Save())
            {
                if (FormMode == enMode.AddNew)
                {
                    OnWarehouseSaved?.Invoke(_Warehouse.WareHouseID, _Warehouse.Name);
                    txtbTitle.Text = "Edit Warehouse";
                    FormMode = enMode.Update;
                    _WarehouseID = _Warehouse.WareHouseID;
                    IsSaved = true;
                    MessageBox.Show("Warehouse added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                txtbTitle.Text = "Edit Warehouse";
                MessageBox.Show("Warehouse updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                IsSaved = true;
                return;
            }
            else
            {
                MessageBox.Show("Error saving warehouse record.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {

            // Hide previous messages
            HideMessages();

            // Validate all fields - This will now show errors even if fields are empty
            ValidationResult validationResults = ValidateAllFields();

            if (validationResults.IsValid)
            {
                // All fields are valid - show success
                ShowSuccessMessage();

                // Optional: Process the form data
                ProcessFormData();
            }
            else
            {
                // Show error messages
                ShowErrorMessage(validationResults.Errors);

                // Scroll to first error
                ScrollToFirstError(validationResults.FirstInvalidControl);
            }

            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ShouldInitialize = false;

            this.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            ShouldInitialize = false;

            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!ShouldInitialize)
                return; // skip initialization if flag is false


            // Initialize empty for AddNew
            
            ResetDefaultValues();

            _isLoadingForm = true;


            if (FormMode == enMode.Update)
                LoadData();

            _isLoadingForm = false;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                // Red if empty, gray otherwise
                tb.BorderBrush = string.IsNullOrWhiteSpace(tb.Text)
                                 ? new SolidColorBrush(Color.FromRgb(231, 76, 60)) // red
                                 : new SolidColorBrush(Color.FromRgb(204, 204, 204)); // gray
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sliderR == null || sliderG == null || sliderB == null) return;

            byte r = (byte)sliderR.Value;
            byte g = (byte)sliderG.Value;
            byte b = (byte)sliderB.Value;

            txtR.Text = r.ToString();
            txtG.Text = g.ToString();
            txtB.Text = b.ToString();

            var color = System.Windows.Media.Color.FromRgb(r, g, b);
            rectPreview.Fill = new SolidColorBrush(color);

            txtSelectedColor.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private ValidationResult ValidateAllFields()
        {
            var result = new ValidationResult { IsValid = true };
            var errors = new List<string>();

            #region Validate Warehouse Name
            // Force validation
            WarehouseName.ValidateForce();

            // Force validation with duplicate check
            WarehouseName.Validate(live: false, externalValidator: text =>
            {
                string errMessage = "This warehouse name already exists.";
                if (FormMode == enMode.Update)
                {
                    if (clsWareHouse.IsWarehouseExistByName(text.Trim(), _WarehouseID))
                        return errMessage;
                }
                else
                {
                    if (clsWareHouse.IsWarehouseExistByName(text.Trim()))
                        return errMessage;
                }

                return null; // no errors
            });

            // Now check the result
            if (!WarehouseName.IsValid)
            {
                errors.Add($"• {WarehouseName.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = WarehouseName;
            }

            #endregion

            #region Validate Location
            // Validate Location - FORCE validation
            Location.ValidateForce();

            // Force validation with duplicate check
            Location.Validate(live: false, externalValidator: text =>
            {
                string errMessage = "This location already exists.";
                if (FormMode == enMode.Update)
                {
                    if (clsWareHouse.IsWarehouseExistByLocation(text.Trim(), _WarehouseID))
                        return errMessage;
                }
                else
                {
                    if (clsWareHouse.IsWarehouseExistByLocation(text.Trim()))
                        return errMessage;
                }

                return null; // no errors
            });

            // Now check the result
            if (!Location.IsValid)
            {
                errors.Add($"• {Location.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = Location;
            }

            #endregion

            #region Description
            // Validate Bio - FORCE validation
            WarehouseDescription.ValidateForce();
            if (!WarehouseDescription.IsValid)
            {
                errors.Add($"• {WarehouseDescription.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = WarehouseDescription;
            }

            #endregion

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
