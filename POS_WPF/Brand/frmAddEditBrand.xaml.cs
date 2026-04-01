using Microsoft.Extensions.Logging;
using POS_BLL;
using POS_WPF.Category;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _ResetDefaultValues();

            _isLoadingForm = true;

            if(FormMode == enMode.Update)
                _LoadData();

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

        bool ProcessFormData()
        {
            _Brand.Name = BrandName.Text.Trim();
            _Brand.Description = BrandDescription.Text;

            try
            {
                bool isBrandSaved = _Brand.Save();
                if (!isBrandSaved)
                {
                    _logger.LogWarning($"Failed to save category: {BrandName}", _Brand.Name);
                    MessageBox.Show(
                        "Failed to save category: " + _Brand.Name,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return false;
                }

                IsSaved = true;

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error saving category: {BrandName}", _Brand.Name);
                MessageBox.Show(
                    "حدث خطأ غير متوقع أثناء حفظ التصنيف. المرجو الاتصال بالدعم.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return false;
            }

            return true;
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
