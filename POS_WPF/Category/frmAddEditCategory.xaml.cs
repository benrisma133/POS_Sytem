using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using POS_BLL;
using POS_DAL;
using POS_WPF.Controls;
using POS_WPF.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace POS_WPF.Category
{
    public partial class frmAddEditCategory : Window
    {
        public Action<int, string> OnCategorySaved;

        private List<int> _selectedWarehouseIDs = new List<int>();

        private bool _isLoadingWarehouses = false;

        private bool _isLoadingForm = false;

        public bool IsSaved { get; private set; } = false;

        enum enMode { AddNew = 1, Update = 2 }
        enMode FormMode = enMode.AddNew;

        clsCategory _Category;
        int _CategoryID = -1;

        public frmAddEditCategory()
        {
            InitializeComponent();
            FormMode = enMode.AddNew;
        }

        public frmAddEditCategory(int CategoryID)
        {
            InitializeComponent();
            FormMode = enMode.Update;
            _CategoryID = CategoryID;
        }

        private void CategoryNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {

            ValidateInput(sender as ModernInput,
                "Category name already exists.",
                text => clsCategory.IsCategoryExistByName(text),
                text => clsCategory.IsCategoryExistByNameExceptID(text, _CategoryID)
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

            var ids = _Category.GetAssignedWarehouseIDs();

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
            ResetDefaultValues();

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
                LoadData();
            }

            _isLoadingWarehouses = false;
            _isLoadingForm = false;
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
                else if(cmbWarehouseFilter.SelectedIndex == 0)
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

        private Viewbox CreateIconViewboxFromJson(string iconJson)
        {
            Canvas iconCanvas = new Canvas
            {
                Width = 24,
                Height = 24
            };

            JArray paths = JArray.Parse(iconJson);

            foreach (var token in paths)
            {
                string pathData = token.ToString();
                if (string.IsNullOrWhiteSpace(pathData)) continue;

                Path path = new Path
                {
                    Data = Geometry.Parse(pathData),
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    StrokeLineJoin = PenLineJoin.Round
                };

                iconCanvas.Children.Add(path);
            }

            return new Viewbox
            {
                Width = 32,
                Height = 32,
                Child = iconCanvas
            };
        }

        // Helper method to create icon canvas from JSON data
        private Canvas CreateIconCanvasFromJson(string iconJson)
        {
            if (string.IsNullOrWhiteSpace(iconJson))
                return null;

            try
            {
                JArray paths = JArray.Parse(iconJson);
                if (paths.Count == 0)
                    return null;

                Canvas canvas = new Canvas();
                double minX = double.MaxValue, minY = double.MaxValue;
                double maxX = double.MinValue, maxY = double.MinValue;

                List<Path> pathList = new List<Path>();

                foreach (var token in paths)
                {
                    string pathData = token.ToString();
                    if (string.IsNullOrWhiteSpace(pathData))
                        continue;

                    Geometry geometry = Geometry.Parse(pathData);
                    Rect bounds = geometry.Bounds;

                    // Track overall bounds
                    minX = Math.Min(minX, bounds.Left);
                    minY = Math.Min(minY, bounds.Top);
                    maxX = Math.Max(maxX, bounds.Right);
                    maxY = Math.Max(maxY, bounds.Bottom);

                    pathList.Add(new Path { Data = geometry });
                }

                // Set canvas size to content bounds
                if (pathList.Count > 0 && maxX > minX && maxY > minY)
                {
                    canvas.Width = maxX - minX;
                    canvas.Height = maxY - minY;

                    // Calculate adaptive stroke thickness based on icon size
                    double iconSize = Math.Max(canvas.Width, canvas.Height);
                    double strokeThickness = iconSize / 18; // Adaptive: larger icons = thicker strokes

                    // Add all paths with proper styling
                    foreach (Path path in pathList)
                    {
                        path.Stroke = Brushes.Black;
                        path.StrokeThickness = strokeThickness;
                        path.StrokeStartLineCap = PenLineCap.Round;
                        path.StrokeEndLineCap = PenLineCap.Round;
                        path.StrokeLineJoin = PenLineJoin.Round;
                        path.Fill = Brushes.Transparent;

                        TransformGroup transforms = new TransformGroup();
                        transforms.Children.Add(new TranslateTransform(-minX, -minY));
                        path.RenderTransform = transforms;
                        canvas.Children.Add(path);
                    }

                    return canvas;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        void LoadData()
        {
            _Category = clsCategory.FindByID(_CategoryID);

            if (_Category == null)
            {
                MessageBox.Show("Category record not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            CategoryName.Text = _Category.Name;
            CategoryDescription.Text = _Category.Description;

            // ✅ LOAD ICON
            if (_Category.IconID.HasValue)
            {
                var icon = clsCategoryIcon.FindByID(_Category.IconID.Value);
                if (icon != null && !string.IsNullOrWhiteSpace(icon.IconData))
                {
                    _selectedIconID = icon.IconID;
                    _selectedIconData = icon.IconData;

                    Canvas iconCanvas = CreateIconCanvasFromJson(icon.IconData);
                    if (iconCanvas != null)
                    {
                        vbSelectedIcon.Child = iconCanvas;
                        vbSelectedIcon.Visibility = Visibility.Visible;
                        btnRemoveIcon.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        vbSelectedIcon.Visibility = Visibility.Collapsed;
                        btnRemoveIcon.Visibility = Visibility.Collapsed;
                    }
                }
            }

            // hide picker by default


            LoadSelectedWarehouses();
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        void ResetDefaultValues()
        {
            if (FormMode == enMode.AddNew)
            {
                _Category = new clsCategory();
                txtbTitle.Text = "Add New Category";

                CategoryName.Text = "";
                CategoryDescription .Text = "";
            }
            else
            {
                txtbTitle.Text = "Edit Category";
            }
        }
        private readonly ILogger _logger = AppLogger.CreateLogger<frmAddEditCategory>();

        private bool ProcessFormData()
        {
            // Collect form data
            _Category.Name = CategoryName.Text.Trim();
            _Category.Description = CategoryDescription.Text;

            if (_selectedIconID != -1)
                _Category.IconID = _selectedIconID;
            else
                _Category.IconID = null;

            // Try to save category
            try
            {
                if (!_Category.Save())
                {
                    _logger.LogWarning("Failed to save category: {CategoryName}", _Category.Name);
                    MessageBox.Show(
                        "Failed to save category: " + _Category.Name,
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
                        !_Category.AssignToWarehouses(warehouseIDsToUse))
                    {
                        MessageBox.Show(
                            "Category saved but failed to assign warehouses.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                    }

                    OnCategorySaved?.Invoke(_Category.CategoryID, _Category.Name);

                    IsSaved = true;
                }
                else if (FormMode == enMode.Update)
                {
                    if (!_Category.UpdateWarehouses(warehouseIDsToUse))
                    {
                        MessageBox.Show(
                            "Category updated but failed to update warehouses.",
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
                _logger.LogError(ex, "Unexpected error saving category: {CategoryName}", _Category.Name);
                MessageBox.Show(
                    "حدث خطأ غير متوقع أثناء حفظ التصنيف. المرجو الاتصال بالدعم.",
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
            // else: ProcessFormData already shows message, no need to show success
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {

            CategoryName.Reset();
            CategoryDescription.Reset();

            // Hide messages
            HideMessages();

            // Optional: Show feedback
            MessageBox.Show("Form cleared successfully!", "Clear Form",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.BorderBrush = string.IsNullOrWhiteSpace(tb.Text)
                    ? new SolidColorBrush(Color.FromRgb(231, 76, 60)) // red
                    : new SolidColorBrush(Color.FromRgb(204, 204, 204)); // gray
            }
        }

        private string _selectedIconData = null;
        private int _selectedIconID = -1;

        public class IconInfo
        {
            public int IconID { get; set; }
            public bool Selected { get; set; }
            public Button ButtonRef { get; set; }
        }

        // Assuming _selectedIconData contains JSON of selected icon
        private void UpdateSelectedIconPreview()
        {
            if (string.IsNullOrWhiteSpace(_selectedIconData))
            {
                vbSelectedIcon.Visibility = Visibility.Collapsed;
                btnRemoveIcon.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                JArray paths = JArray.Parse(_selectedIconData);
                if (paths.Count == 0)
                {
                    vbSelectedIcon.Visibility = Visibility.Collapsed;
                    btnRemoveIcon.Visibility = Visibility.Collapsed;
                    return;
                }

                Canvas canvas = new Canvas();
                double minX = double.MaxValue, minY = double.MaxValue;
                double maxX = double.MinValue, maxY = double.MinValue;

                List<Path> pathList = new List<Path>();

                foreach (var token in paths)
                {
                    string pathData = token.ToString();
                    if (string.IsNullOrWhiteSpace(pathData))
                        continue;

                    Geometry geometry = Geometry.Parse(pathData);
                    Rect bounds = geometry.Bounds;

                    // Track overall bounds
                    minX = Math.Min(minX, bounds.Left);
                    minY = Math.Min(minY, bounds.Top);
                    maxX = Math.Max(maxX, bounds.Right);
                    maxY = Math.Max(maxY, bounds.Bottom);

                    pathList.Add(new Path { Data = geometry });
                }

                // Set canvas size to content bounds
                if (pathList.Count > 0 && maxX > minX && maxY > minY)
                {
                    canvas.Width = maxX - minX;
                    canvas.Height = maxY - minY;

                    // Calculate adaptive stroke thickness based on icon size
                    double iconSize = Math.Max(canvas.Width, canvas.Height);
                    double strokeThickness = iconSize / 22; // Adaptive: larger icons = thicker strokes

                    // Add all paths with proper styling
                    foreach (Path path in pathList)
                    {
                        path.Stroke = Brushes.Black;
                        path.StrokeThickness = strokeThickness;
                        path.StrokeStartLineCap = PenLineCap.Round;
                        path.StrokeEndLineCap = PenLineCap.Round;
                        path.StrokeLineJoin = PenLineJoin.Round;
                        path.Fill = Brushes.Transparent;

                        TransformGroup transforms = new TransformGroup();
                        transforms.Children.Add(new TranslateTransform(-minX, -minY));
                        path.RenderTransform = transforms;
                        canvas.Children.Add(path);
                    }

                    vbSelectedIcon.Child = canvas;
                    vbSelectedIcon.Visibility = Visibility.Visible;
                    btnRemoveIcon.Visibility = Visibility.Visible;
                }
                else
                {
                    vbSelectedIcon.Visibility = Visibility.Collapsed;
                    btnRemoveIcon.Visibility = Visibility.Collapsed;
                }
            }
            catch
            {
                vbSelectedIcon.Visibility = Visibility.Collapsed;
                btnRemoveIcon.Visibility = Visibility.Collapsed;
            }
        }

        void LoadIcons()
        {
            wpIcons.Children.Clear();
            wpIcons.Margin = new Thickness(8); // Add this before LoadIcons() if needed
            DataTable dt = POS_BLL.clsCategoryIcon.GetAll();

            foreach (DataRow row in dt.Rows)
            {
                int iconID = Convert.ToInt32(row["IconID"]);
                string iconName = row["IconName"].ToString();
                string iconJson = row["IconData"]?.ToString();

                if (string.IsNullOrWhiteSpace(iconJson))
                    continue;

                try
                {
                    JArray paths = JArray.Parse(iconJson);
                    if (paths.Count == 0)
                        continue;

                    // Use Viewbox directly - it automatically handles any geometry size
                    Viewbox iconViewbox = new Viewbox
                    {
                        Width = 40,
                        Height = 40,
                        Stretch = Stretch.Uniform,
                        StretchDirection = StretchDirection.Both
                    };

                    // Multiple paths - use Canvas as container
                    Canvas canvas = new Canvas();
                    double minX = double.MaxValue, minY = double.MaxValue;
                    double maxX = double.MinValue, maxY = double.MinValue;

                    List<Path> pathList = new List<Path>();

                    foreach (var token in paths)
                    {
                        string pathData = token.ToString();
                        if (string.IsNullOrWhiteSpace(pathData))
                            continue;

                        Geometry geometry = Geometry.Parse(pathData);
                        Rect bounds = geometry.Bounds;

                        // Track overall bounds
                        minX = Math.Min(minX, bounds.Left);
                        minY = Math.Min(minY, bounds.Top);
                        maxX = Math.Max(maxX, bounds.Right);
                        maxY = Math.Max(maxY, bounds.Bottom);

                        pathList.Add(new Path { Data = geometry });
                    }

                    // Set canvas size to content bounds
                    if (pathList.Count > 0 && maxX > minX && maxY > minY)
                    {
                        canvas.Width = maxX - minX;
                        canvas.Height = maxY - minY;

                        // Calculate adaptive stroke thickness based on icon size
                        double iconSize = Math.Max(canvas.Width, canvas.Height);
                        double strokeThickness = iconSize / 22; // Adaptive: larger icons = thicker strokes

                        // Add all paths with proper styling
                        foreach (Path path in pathList)
                        {
                            path.Stroke = Brushes.Black;
                            path.StrokeThickness = strokeThickness;
                            path.StrokeStartLineCap = PenLineCap.Round;
                            path.StrokeEndLineCap = PenLineCap.Round;
                            path.StrokeLineJoin = PenLineJoin.Round;
                            path.Fill = Brushes.Transparent;

                            TransformGroup transforms = new TransformGroup();
                            transforms.Children.Add(new TranslateTransform(-minX, -minY));
                            path.RenderTransform = transforms;
                            canvas.Children.Add(path);
                        }

                        iconViewbox.Child = canvas;
                    }

                    // Skip if no content was added
                    if (iconViewbox.Child == null)
                        continue;

                    // Create StackPanel to hold icon and name
                    StackPanel iconStack = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    iconStack.Children.Add(iconViewbox);

                    // Add icon name below the icon
                    TextBlock iconNameText = new TextBlock
                    {
                        Text = iconName,
                        FontSize = 10,
                        Foreground = Brushes.Black,
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 4, 0, 0),
                        MaxWidth = 64
                    };
                    iconStack.Children.Add(iconNameText);

                    Button btn = new Button
                    {
                        Width = 68,
                        Height = 80, // Increased height to accommodate text
                        Style = (Style)FindResource("IconPickerButtonStyle"),
                        Content = iconStack,
                        Padding = new Thickness(2)
                    };

                    var iconInfo = new IconInfo { IconID = iconID, Selected = false, ButtonRef = btn };
                    btn.Tag = iconInfo;

                    // Hover effect
                    btn.MouseEnter += (s, e) =>
                    {
                        if (!iconInfo.Selected)
                        {
                            btn.BorderBrush = Brushes.Gray;
                            btn.BorderThickness = new Thickness(2);
                        }
                    };

                    btn.MouseLeave += (s, e) =>
                    {
                        if (!iconInfo.Selected)
                        {
                            btn.BorderBrush = new SolidColorBrush(
                                (Color)ColorConverter.ConvertFromString("#FF8C42")
                            );

                            btn.BorderThickness = new Thickness(0);
                        }
                    };

                    // Click selection
                    btn.Click += (s, e) =>
                    {
                        _selectedIconData = iconJson;
                        _selectedIconID = iconID;

                        // Deselect all buttons
                        foreach (Button b in wpIcons.Children)
                        {
                            if (b.Tag is IconInfo info)
                            {
                                info.Selected = false;
                                b.BorderBrush = Brushes.Transparent;
                                b.BorderThickness = new Thickness(0);
                            }
                        }

                        // Select clicked button
                        iconInfo.Selected = true;
                        btn.BorderBrush = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#FF8C42")
                        );

                        btn.BorderThickness = new Thickness(2);
                    };

                    wpIcons.Children.Add(btn);
                }
                catch
                {
                    continue;
                }
            }
        }

        private void BtnSetIcon_Click(object sender, RoutedEventArgs e)
        {
            grdIconPickerOverlay.Visibility = Visibility.Visible;
            LoadIcons();
        }

        // Remove icon
        private void BtnRemoveIcon_Click(object sender, RoutedEventArgs e)
        {
            _selectedIconData = null;
            _selectedIconID = 0;
            vbSelectedIcon.Visibility = Visibility.Collapsed;
            btnRemoveIcon.Visibility = Visibility.Collapsed;

            // also deselect all buttons in icon picker
            foreach (Button b in wpIcons.Children)
            {
                if (b.Tag is IconInfo info)
                {
                    info.Selected = false;
                    b.BorderBrush = Brushes.Transparent;
                    b.BorderThickness = new Thickness(0);
                }
            }
        }

        private void BtnIconCancel_Click(object sender, RoutedEventArgs e)
        {
            grdIconPickerOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnIconSelect_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedIconData == null)
            {
                MessageBox.Show("Please select an icon");
                return;
            }

            // save icon to category object
            // Category.IconData = _selectedIconData;
            UpdateSelectedIconPreview();
            grdIconPickerOverlay.Visibility = Visibility.Collapsed;
        }

        private ValidationResult ValidateAllFields()
        {
            var result = new ValidationResult { IsValid = true };
            var errors = new List<string>();

            // Force validation
            CategoryName.ValidateForce();

            // Force validation with duplicate check
            CategoryName.Validate(live: false, externalValidator: text =>
            {
                if (FormMode == enMode.Update)
                {
                    if (clsCategory.IsCategoryExistByNameExceptID(text.Trim(), _CategoryID))
                        return "This category name already exists.";
                }
                else
                {
                    if (clsCategory.IsCategoryExistByName(text.Trim()))
                        return "This category name already exists.";
                }

                return null; // no errors
            });

            // Now check the result
            if (!CategoryName.IsValid)
            {
                errors.Add($"• {CategoryName.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = CategoryName;
            }





            // Validate Bio - FORCE validation
            CategoryDescription.ValidateForce();
            if (!CategoryDescription.IsValid)
            {
                errors.Add($"• {CategoryDescription.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = CategoryDescription;
            }


            // Set result
            if (errors.Any())
            {
                result.IsValid = false;
                result.Errors = errors;
            }

            return result;
        }

        private bool CategoryExist(string Category)
        {
            return clsCategory.IsCategoryExistByName(Category);
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
