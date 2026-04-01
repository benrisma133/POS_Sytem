using Newtonsoft.Json; 
using Newtonsoft.Json.Linq;
using POS_BLL;
using POS_WPF.Category;
using POS_WPF.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;


namespace POS_WPF.Pages
{
    public partial class CategoryPage : UserControl
    {
        public class IconPathData
        {
            public string PathData { get; set; }
            public string Stroke { get; set; } = "#000000";
            public double StrokeThickness { get; set; } = 2;
        }

        private bool _isLoadingWarehouseFilter = false;
        private bool _warehousesLoaded = false; // To load ComboBox only once

        public bool IsSidebarToggling { get; set; } = false;

        public CategoryPage()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
            this.SizeChanged += UserControl_SizeChanged;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadWarehousesToComboBoxAsync();
            await LoadCategoriesAsync();
            DynamicCardContainer.PreviewMouseWheel += DynamicCardContainer_PreviewMouseWheel;
        }

        private void DynamicCardContainer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (CardScrollViewer != null)
            {
                CardScrollViewer.ScrollToVerticalOffset(CardScrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private string GetSearchText()
        {
            return txtSearch.Dispatcher.Invoke(() =>
            {
                return txtSearch?.Text?.ToLower() ?? "";
            });
        }

        // Add this method to your class
        private void StartLoadingAnimation()
        {
            if (SpinnerRotate != null)
            {
                DoubleAnimation animation = new DoubleAnimation
                {
                    From = 0,
                    To = 360,
                    Duration = TimeSpan.FromSeconds(0.8),
                    RepeatBehavior = RepeatBehavior.Forever
                };

                SpinnerRotate.BeginAnimation(RotateTransform.AngleProperty, animation);
            }
        }

        private void StopLoadingAnimation()
        {
            if (SpinnerRotate != null)
            {
                SpinnerRotate.BeginAnimation(RotateTransform.AngleProperty, null);
            }
        }

        private async Task LoadWarehousesToComboBoxAsync()
        {
            try
            {
                if (LoadingOverlay != null)
                {
                    LoadingOverlay.Visibility = Visibility.Visible;
                    StartLoadingAnimation(); // Start the spinner
                }

                if (txtTitle != null)
                    txtTitle.Text = "Loading...";

                await Task.Delay(50);

                // Load warehouses off the UI thread
                DataTable dt = await Task.Run(() => clsWareHouse.GetAll());

                // Update UI on the main thread - use async-friendly approach
                cmbWarehouse.Items.Clear();

                // Add default option
                cmbWarehouse.Items.Add("All Warehouses");

                foreach (DataRow row in dt.Rows)
                {
                    ComboBoxItem item = new ComboBoxItem
                    {
                        Content = row["Name"].ToString(),
                        Tag = row["WarehouseID"]
                    };
                    cmbWarehouse.Items.Add(item);
                }

                cmbWarehouse.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                if (txtTitle != null)
                    txtTitle.Text = "Category Management - Error";

                MessageBox.Show("Error loading categories: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (LoadingOverlay != null)
                {
                    await Task.Delay(100);
                    StopLoadingAnimation(); // Stop the spinner
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                }
            }
        }

        // No code-behind needed! The animation is handled by XAML triggers.
        // Just show/hide the overlay as before:

        public async Task LoadCategoriesAsync(int? warehouseID = null)
        {
            try
            {
                if (LoadingOverlay != null)
                    LoadingOverlay.Visibility = Visibility.Visible;

                if (txtTitle != null)
                    txtTitle.Text = "Loading...";

                // ⚡ Give animation time to start smoothly
                await Task.Delay(50);

                string filter = GetSearchText()?.ToLower() ?? "";
                string warehouseName = "All Warehouses";

                if (warehouseID == null && cmbWarehouse != null)
                {
                    if (cmbWarehouse.SelectedIndex > 0 && cmbWarehouse.SelectedItem is ComboBoxItem selectedItem)
                    {
                        if (int.TryParse(selectedItem.Tag?.ToString(), out int parsedId))
                            warehouseID = parsedId;

                        warehouseName = selectedItem.Content.ToString();
                    }
                }
                else if (warehouseID != null && cmbWarehouse != null)
                {
                    foreach (var obj in cmbWarehouse.Items)
                    {
                        if (obj is ComboBoxItem item && int.TryParse(item.Tag?.ToString(), out int id) && id == warehouseID)
                        {
                            warehouseName = item.Content.ToString();
                            break;
                        }
                    }
                }

                var categoryData = await Task.Run(() =>
                {
                    DataTable dtCategories = warehouseID == null
                        ? clsCategory.GetAll()
                        : clsCategory.GetByWarehouseID(warehouseID.Value);

                    var filteredRows = dtCategories.AsEnumerable()
                        .Where(row =>
                            row["Name"].ToString().ToLower().Contains(filter) ||
                            row["Description"].ToString().ToLower().Contains(filter))
                        .Select(row => new
                        {
                            CategoryID = Convert.ToInt32(row["CategoryID"]),
                            Name = row["Name"].ToString(),
                            Description = row["Description"].ToString(),
                            IconData = row["IconData"].ToString()
                        })
                        .ToList();

                    return filteredRows;
                });

                // ⚡ Clear and add cards in small batches
                DynamicCardContainer.Items.Clear();
                double cardWidth = GetCardWidth();

                
                    foreach (var item in categoryData)
                    {
                        DynamicCardContainer.Items.Add(
                            CreateCategoryCard(item.CategoryID, item.Name, item.Description, item.IconData, cardWidth)
                        );
                    }

                var wrapPanel = FindVisualChild<WrapPanel>(DynamicCardContainer);
                if (wrapPanel != null)
                {
                    wrapPanel.Margin = new Thickness(14);
                    wrapPanel.HorizontalAlignment = HorizontalAlignment.Left;
                    wrapPanel.VerticalAlignment = VerticalAlignment.Top;
                }

                UpdateCardWidths();

                if (txtTitle != null)
                    txtTitle.Text = $"Category Management - {warehouseName}";

                if (txtTotalProducts != null)
                    txtTotalProducts.Text = $"Total Categories : {categoryData.Count}";

                if (NoCategoriesMessage != null)
                {
                    NoCategoriesMessage.Text = $"No categories for {warehouseName}";
                    NoCategoriesMessage.Visibility = categoryData.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                if (txtTitle != null)
                    txtTitle.Text = "Category Management - Error";

                MessageBox.Show("Error loading categories: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (LoadingOverlay != null)
                {
                    await Task.Delay(100);
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                }
            }
        }

        private double GetCardWidth()
        {
            var wrapPanel = FindVisualChild<WrapPanel>(DynamicCardContainer);
            if (wrapPanel == null) return 400;

            double availableWidth = wrapPanel.ActualWidth
                - wrapPanel.Margin.Left
                - wrapPanel.Margin.Right;

            return availableWidth > 900 ? 420 : availableWidth - 20;
        }

        // ⚡ Icon loading method (keep this as before)
        private void LoadIconAsync(Border iconPlaceholder, string iconJson)
        {
            try
            {
                var paths = JArray.Parse(iconJson);
                if (paths.Count == 0) return;

                Dispatcher.Invoke(() =>
                {
                    Canvas canvas = new Canvas();
                    double minX = double.MaxValue, minY = double.MaxValue;
                    double maxX = double.MinValue, maxY = double.MinValue;
                    List<Path> pathList = new List<Path>();

                    foreach (var pathToken in paths)
                    {
                        string pathData = pathToken.ToString();
                        if (string.IsNullOrWhiteSpace(pathData)) continue;

                        Geometry geometry = Geometry.Parse(pathData);
                        Rect bounds = geometry.Bounds;

                        minX = Math.Min(minX, bounds.Left);
                        minY = Math.Min(minY, bounds.Top);
                        maxX = Math.Max(maxX, bounds.Right);
                        maxY = Math.Max(maxY, bounds.Bottom);

                        pathList.Add(new Path { Data = geometry });
                    }

                    if (pathList.Count > 0 && maxX > minX && maxY > minY)
                    {
                        canvas.Width = maxX - minX;
                        canvas.Height = maxY - minY;

                        double iconSize = Math.Max(canvas.Width, canvas.Height);
                        double strokeThickness = iconSize / 22;

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

                        Viewbox iconViewbox = new Viewbox
                        {
                            Width = 54,
                            Height = 54,
                            Stretch = Stretch.Uniform,
                            Child = canvas
                        };

                        iconPlaceholder.Child = iconViewbox;
                        iconPlaceholder.Background = Brushes.Transparent;
                    }
                });
            }
            catch
            {
                // Ignore invalid JSON
            }
        }

        private Border CreateCategoryCard(int id, string name, string description, string iconJson, double width)
        {
            // Main border
            Border cardBorder = new Border
            {
                Width = Math.Max(0, width),
                Background = new SolidColorBrush(Color.FromRgb(250, 251, 252)),
                CornerRadius = new CornerRadius(16),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 12, 12),
                SnapsToDevicePixels = true,
                Cursor = Cursors.Hand,
                Tag = id,
                Opacity = 0,
                RenderTransform = new TranslateTransform(0, 20),
                CacheMode = new BitmapCache()
            };

            // ✅ Shadow (create once)
            var cardShadow = new DropShadowEffect
            {
                Color = Color.FromRgb(148, 163, 184),
                BlurRadius = 12,
                ShadowDepth = 2,
                Opacity = 0.15
            };
            cardBorder.Effect = cardShadow;

            // Grid
            Grid cardGrid = new Grid();
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Text stack
            StackPanel textStack = new StackPanel();

            StackPanel iconAndNameStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 4)
            };

            // Icon placeholder
            Border iconPlaceholder = new Border
            {
                Width = 54,
                Height = 54,
                Margin = new Thickness(8, 0, 18, 0),
                Background = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                CornerRadius = new CornerRadius(8),
                Tag = iconJson
            };
            iconAndNameStack.Children.Add(iconPlaceholder);

            // Name
            TextBlock nameBlock = new TextBlock
            {
                Text = name,
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                FontFamily = new FontFamily("Segoe UI"),
                VerticalAlignment = VerticalAlignment.Center
            };
            iconAndNameStack.Children.Add(nameBlock);

            textStack.Children.Add(iconAndNameStack);

            // Description
            TextBlock descriptionBlock = new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(description) ? "No description available" : description,
                FontSize = 14,
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(string.IsNullOrWhiteSpace(description) ? Color.FromRgb(148, 163, 184) : Color.FromRgb(100, 116, 139)),
                FontFamily = new FontFamily("Segoe UI"),
                LineHeight = 20,
                FontStyle = string.IsNullOrWhiteSpace(description) ? FontStyles.Italic : FontStyles.Normal
            };
            textStack.Children.Add(descriptionBlock);

            cardGrid.Children.Add(textStack);

            // Buttons stack
            StackPanel buttonStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed
            };
            Grid.SetColumn(buttonStack, 1);

            Button editBtn = CardButtonsFactory.CreateEditButton(BtnEdit_Click, id);
            Button deleteBtn = CardButtonsFactory.CreateDeleteButton(BtnDelete_Click, id);
            deleteBtn.Margin = new Thickness(8, 0, 0, 0);

            buttonStack.Children.Add(editBtn);
            buttonStack.Children.Add(deleteBtn);
            cardGrid.Children.Add(buttonStack);

            cardBorder.Child = cardGrid;

            // ⚡ Lazy load icon
            cardBorder.Loaded += async (s, e) => 
            {
                if (!string.IsNullOrWhiteSpace(iconJson))
                {
                    await Task.Run(() => Task.Delay(1));
                    LoadIconAsync(iconPlaceholder, iconJson);
                }

                if (IsSidebarToggling)
                {
                    cardBorder.Opacity = 1;
                    ((TranslateTransform)cardBorder.RenderTransform).Y = 0;
                    return;
                }

                // Fade + slide animation
                cardBorder.BeginAnimation(Border.OpacityProperty,
                    new DoubleAnimation(1, TimeSpan.FromMilliseconds(400))
                    { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } });

                // Fade + slide animation
                // ✅ استخدام TransformGroup باش نتجنب crash
                TranslateTransform translate;
                ScaleTransform scale;
                TransformGroup tg;

                if (cardBorder.RenderTransform is TransformGroup group)
                {
                    tg = group;

                    translate = group.Children.OfType<TranslateTransform>().FirstOrDefault();
                    if (translate == null)
                    {
                        translate = new TranslateTransform(0, 20);
                        tg.Children.Add(translate);
                    }

                    scale = group.Children.OfType<ScaleTransform>().FirstOrDefault();
                    if (scale == null)
                    {
                        scale = new ScaleTransform(1, 1);
                        tg.Children.Add(scale);
                    }
                }
                else
                {
                    translate = new TranslateTransform(0, 20);
                    scale = new ScaleTransform(1, 1);
                    tg = new TransformGroup();
                    tg.Children.Add(scale);
                    tg.Children.Add(translate);
                    cardBorder.RenderTransform = tg;
                }

                translate.BeginAnimation(
                    TranslateTransform.YProperty,
                    new DoubleAnimation(0, TimeSpan.FromMilliseconds(400))
                    { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } });

                cardBorder.BeginAnimation(
                    Border.OpacityProperty,
                    new DoubleAnimation(1, TimeSpan.FromMilliseconds(400))
                    { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } });

            };

            // Mouse hover
            cardBorder.MouseEnter += (s, e) => Card_MouseEnterAnimations(cardBorder, buttonStack);
            cardBorder.MouseLeave += (s, e) => Card_MouseLeaveAnimations(cardBorder, buttonStack);

            // Scroll wheel support
            cardBorder.PreviewMouseWheel += (s, e) =>
            {
                if (CardScrollViewer != null)
                {
                    CardScrollViewer.ScrollToVerticalOffset(
                        CardScrollViewer.VerticalOffset - e.Delta
                    );
                    e.Handled = true;
                }
            };

            return cardBorder;
        }

        // ======================= Hover Animations =======================
        private void Card_MouseEnterAnimations(Border cardBorder, StackPanel buttonStack)
        {
            // تأكد من وجود TransformGroup
            TransformGroup tg;
            ScaleTransform scale;

            if (cardBorder.RenderTransform is TransformGroup group)
            {
                tg = group;
                scale = group.Children.OfType<ScaleTransform>().FirstOrDefault();
                if (scale == null)
                {
                    scale = new ScaleTransform(1, 1);
                    tg.Children.Add(scale);
                }
            }
            else
            {
                // إنشاء TransformGroup جديد
                scale = new ScaleTransform(1, 1);
                var translate = cardBorder.RenderTransform as TranslateTransform ?? new TranslateTransform(0, 0);
                tg = new TransformGroup();
                tg.Children.Add(scale);
                tg.Children.Add(translate);
                cardBorder.RenderTransform = tg;
            }

            cardBorder.RenderTransformOrigin = new Point(0.5, 0.5);

            // Scale animation
            scale.BeginAnimation(ScaleTransform.ScaleXProperty,
                new DoubleAnimation(1.01, TimeSpan.FromMilliseconds(250))
                { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } });

            scale.BeginAnimation(ScaleTransform.ScaleYProperty,
                new DoubleAnimation(1.01, TimeSpan.FromMilliseconds(250))
                { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } });

            // Border color
            ((SolidColorBrush)cardBorder.BorderBrush).BeginAnimation(
                SolidColorBrush.ColorProperty,
                new ColorAnimation(Color.FromRgb(155, 155, 155), TimeSpan.FromMilliseconds(350))
            );

            // Shadow animation
            if (cardBorder.Effect is DropShadowEffect hoverShadow)
            {
                hoverShadow.BeginAnimation(DropShadowEffect.BlurRadiusProperty,
                    new DoubleAnimation(18, TimeSpan.FromMilliseconds(260)));

                hoverShadow.BeginAnimation(DropShadowEffect.OpacityProperty,
                    new DoubleAnimation(0.28, TimeSpan.FromMilliseconds(260)));
            }

            // Buttons fade in
            buttonStack.Visibility = Visibility.Visible;
            buttonStack.BeginAnimation(StackPanel.OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)));
        }


        private void Card_MouseLeaveAnimations(Border cardBorder, StackPanel buttonStack)
        {
            // Scale back
            if (cardBorder.RenderTransform is ScaleTransform scale)
            {
                scale.BeginAnimation(ScaleTransform.ScaleXProperty,
                    new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(250)));
                scale.BeginAnimation(ScaleTransform.ScaleYProperty,
                    new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(250)));
            }

            // Border color back
            ((SolidColorBrush)cardBorder.BorderBrush).BeginAnimation(
                SolidColorBrush.ColorProperty,
                new ColorAnimation(Color.FromRgb(223, 230, 233), TimeSpan.FromMilliseconds(350))
            );

            // Shadow back
            if (cardBorder.Effect is DropShadowEffect normalShadow)
            {
                normalShadow.BeginAnimation(DropShadowEffect.BlurRadiusProperty,
                    new DoubleAnimation(12, TimeSpan.FromMilliseconds(260)));

                normalShadow.BeginAnimation(DropShadowEffect.OpacityProperty,
                    new DoubleAnimation(0.15, TimeSpan.FromMilliseconds(260)));
            }

            // Buttons fade out
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(220));
            fadeOut.Completed += (s, e) => buttonStack.Visibility = Visibility.Collapsed;
            buttonStack.BeginAnimation(StackPanel.OpacityProperty, fadeOut);
        }


        private WrapPanel GetWrapPanel() => FindVisualChild<WrapPanel>(DynamicCardContainer);

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;

                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private void UpdateCardWidths()
        {
            var wrapPanel = GetWrapPanel();
            if (wrapPanel == null) return;

            int cardCount = wrapPanel.Children.Count;

            double availableWidth =
                CardScrollViewer.ActualWidth
                - wrapPanel.Margin.Left
                - wrapPanel.Margin.Right
                - 16; // padding عام

            foreach (var child in wrapPanel.Children)
            {
                if (child is Border card)
                {
                    if (cardCount == 1)
                    {
                        // 🟢 Card وحدة → Full width
                        card.Width = availableWidth;
                    }
                    else if (cardCount == 2)
                    {
                        // 🔵 جوج كروت → نصف العرض
                        card.Width = (availableWidth / 2) - 16; // 16 = margin بيناتهم
                    }
                    else
                    {
                        // 🟣 3 ولا أكثر
                        card.Width = availableWidth > 760 ? (availableWidth / 2) - 16 : availableWidth - 16;
                    }
                }
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
            => UpdateCardWidths();

        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtPlaceholder.Visibility =
                string.IsNullOrEmpty(txtSearch.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            await LoadCategoriesAsync();
            UpdateCardWidths();
        }

        // Add new category
        private async void BtnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            frmAddEditCategory addCategoryWindow = new frmAddEditCategory(); // Add mode
            addCategoryWindow.Owner = Application.Current.MainWindow;
            addCategoryWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addCategoryWindow.ShowDialog();
            if (addCategoryWindow.IsSaved)
            {
                await LoadCategoriesAsync(); // Refresh the category list after closing
            }
        }

        // Edit existing category
        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int categoryId = (int)btn.Tag; // Get the category ID from the button's Tag
                frmAddEditCategory editCategoryWindow = new frmAddEditCategory(categoryId); // Edit mode constructor
                editCategoryWindow.Owner = Application.Current.MainWindow;
                editCategoryWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                editCategoryWindow.ShowDialog();
                if (editCategoryWindow.IsSaved)
                {
                    await LoadCategoriesAsync(); // Refresh the category list after closing
                }
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int categoryID = (int)btn.Tag;

                // Inform the user clearly about deletion scope
                var result = MessageBox.Show(
                    "This action will permanently delete this category from ALL warehouses.\n\n" +
                    "If you only want to remove it from the selected warehouse, please unselect it instead.\n\n" +
                    "Do you want to proceed?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (clsCategory.DeleteCompletely(categoryID)) // your delete logic
                    {
                        MessageBox.Show(
                            "Category has been removed from all warehouses successfully.",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        await LoadCategoriesAsync();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Error removing category. It might be linked to other records.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void cmbWarehouse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingWarehouseFilter) return;

            // Let LoadCategoriesAsync handle the warehouse ID and title
            await LoadCategoriesAsync();
        }

        bool isDefaultSelected = true;

        // Implementation async
        // DO NOT FORGET ABOUT AWAIT !!!
        private void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Code Here
            //await LoadWarehousesToComboBoxAsync();
            //if(isDefaultSelected) return;
            if(!isDefaultSelected)
            {
                MessageBox.Show("Comming soon...", "Not Implemented Yet.", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            isDefaultSelected = false;
        }

    }
}
