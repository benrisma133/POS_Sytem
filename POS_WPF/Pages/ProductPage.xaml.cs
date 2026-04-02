using POS_BLL;
using POS_WPF.Category;
using POS_WPF.Product;
using POS_WPF.UI;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace POS_WPF.Pages
{
    public partial class ProductPage : UserControl
    {
        // 🔐 Stored in memory
        private int _lastAddedProductID = -1;
        private string _lastAddedProductName = string.Empty;

        private bool _isLoaded = false;

        public ProductPage()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
            this.SizeChanged += UserControl_SizeChanged;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            await LoadWarehousesToComboBoxAsync();
            await LoadProductsAsync();
            //DynamicCardContainer.PreviewMouseWheel += DynamicCardContainer_PreviewMouseWheel;
        }

        //private void DynamicCardContainer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    if (CardScrollViewer != null)
        //    {
        //        CardScrollViewer.ScrollToVerticalOffset(CardScrollViewer.VerticalOffset - e.Delta);
        //        e.Handled = true;
        //    }
        //}

        private string GetSearchText()
        {
            return txtSearch.Dispatcher.Invoke(() =>
            {
                return txtSearch?.Text?.ToLower() ?? "";
            });
        }

        // ================= LOAD PRODUCTS =================
        public async Task LoadProductsAsync()
        {
            try
            {
                if (LoadingOverlay != null)
                    LoadingOverlay.Visibility = Visibility.Visible;

                if (txtTitle != null)
                    txtTitle.Text = "Loading...";

                string filter = await Task.Run(() => GetSearchText());

                DataTable dt = await Task.Run(() => clsProduct.GetAll());

                // Get selected warehouse
                string selectedWarehouse = null;
                if (cmbWarehouse.SelectedItem is ComboBoxItem selectedItem)
                    selectedWarehouse = selectedItem.Content.ToString();
                else if (cmbWarehouse.SelectedItem is string all)
                    selectedWarehouse = all;

                // Filter rows
                var filteredRows = dt.AsEnumerable()
                    .Where(r =>
                    {
                        bool matchesText = r["ProductName"].ToString().ToLower().Contains(filter)
                                           || r["Category"].ToString().ToLower().Contains(filter)
                                           || r["Model"].ToString().ToLower().Contains(filter);

                        bool matchesWarehouse = selectedWarehouse == "All Warehouses"
                                                || r["WarehouseName"].ToString() == selectedWarehouse;

                        return matchesText && matchesWarehouse;
                    })
                    .ToList();

                DynamicCardContainer.Dispatcher.Invoke(() =>
                {
                    DynamicCardContainer.Items.Clear();
                    double cardWidth = GetCardWidth();

                    if (filteredRows.Count == 0)
                    {
                        NoProductsMessage.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        NoProductsMessage.Visibility = Visibility.Collapsed;

                        foreach (var row in filteredRows)
                        {
                            int id = Convert.ToInt32(row["ProductID"]);

                            var card = CreateProductCard(
                                id,
                                row["ProductName"].ToString(),
                                row["Category"].ToString(),
                                row["Model"].ToString(),
                                Convert.ToDecimal(row["Price"]),
                                Convert.ToInt32(row["Quantity"]),
                                cardWidth,
                                row["WarehouseName"].ToString(),
                                row["WarehouseColor"].ToString()
                            );

                            DynamicCardContainer.Items.Add(card);
                        }

                        var wrapPanel = FindVisualChild<WrapPanel>(DynamicCardContainer);
                        if (wrapPanel != null)
                        {
                            wrapPanel.Margin = new Thickness(14);
                            wrapPanel.HorizontalAlignment = HorizontalAlignment.Left;
                            wrapPanel.VerticalAlignment = VerticalAlignment.Top;
                        }

                        UpdateCardWidths();
                    }

                    if (txtTitle != null)
                        txtTitle.Text = "Products Management";

                    // Update total products
                    txtTotalProducts.Text = $"Total Products: {filteredRows.Count}";
                });

            }
            finally
            {
                if (LoadingOverlay != null)
                    LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadWarehousesToComboBoxAsync()
        {
            try
            {
                // Optionally show a loading overlay here
                if (LoadingOverlay != null)
                    LoadingOverlay.Visibility = Visibility.Visible;

                // Load warehouses off the UI thread
                DataTable dt = await Task.Run(() => clsWareHouse.GetAll());

                // Update UI on the main thread
                cmbWarehouse.Dispatcher.Invoke(() =>
                {
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

                    cmbWarehouse.SelectedIndex = 0; // Default to All Categories
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading warehouses: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (LoadingOverlay != null)
                    LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        // ================= CARD WIDTH =================
        private double GetCardWidth()
        {
            var wrapPanel = FindVisualChild<WrapPanel>(DynamicCardContainer);
            if (wrapPanel == null) return 400;

            double availableWidth =
                wrapPanel.ActualWidth - wrapPanel.Margin.Left - wrapPanel.Margin.Right;

            return availableWidth > 900 ? 420 : availableWidth - 20;
        }

        private StackPanel CreateButtonWithHoverText(Button btn, string hoverText)
        {
            StackPanel container = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock text = new TextBlock
            {
                Text = hoverText,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4), // space between text and button
                Opacity = 0, // start hidden
                HorizontalAlignment = HorizontalAlignment.Center
            };

            container.Children.Add(text);
            container.Children.Add(btn);

            // Hover animations
            btn.MouseEnter += (_, __) =>
            {
                DoubleAnimation fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(250));
                text.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            btn.MouseLeave += (_, __) =>
            {
                DoubleAnimation fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250));
                text.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            };

            return container;
        }

        private Border CreateProductCard(
            int id,
            string name,
            string category,
            string model,
            decimal price,
            int quantity,
            double width,
            string warehouse,
            string warehouseColor)
        {
            // ---------- CARD BORDER ----------
            Border cardBorder = new Border
            {
                Width = Math.Max(0, width),
                Background = new SolidColorBrush(Color.FromRgb(250, 251, 252)), // Soft off-white
                CornerRadius = new CornerRadius(16),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)), // Soft blue-gray
                BorderThickness = new Thickness(1),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 12, 12),
                Cursor = Cursors.Hand,
                Tag = id,
                Opacity = 0,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new TransformGroup
                {
                    Children =
                    {
                        new ScaleTransform(1,1),
                        new TranslateTransform(0,20)
                    }
                },
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(148, 163, 184), // same as category card
                    BlurRadius = 12,                       // soft blur
                    ShadowDepth = 2,                       // subtle depth
                    Opacity = 0.15                         // light shadow
                }

            };

            // ---------- ROOT GRID ----------
            Grid rootGrid = new Grid();
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // ---------- WAREHOUSE BADGE ----------
            Border warehouseBadge = new Border
            {
                Background = Brushes.White,
                
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(12, 5, 12, 5),
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 0, 12),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 6,
                    ShadowDepth = 1,
                    Opacity = 0.2
                }
            };

            warehouseBadge.Child = new TextBlock
            {
                Text = warehouse.ToUpper(),
                Foreground = BrushFromHex(warehouseColor),
                FontWeight = FontWeights.SemiBold,
                FontSize = 12,
                FontFamily = new FontFamily("Segoe UI")
            };

            Grid.SetRow(warehouseBadge, 0);
            rootGrid.Children.Add(warehouseBadge);

            // ---------- CONTENT GRID ----------
            Grid contentGrid = new Grid();
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

            // ---------- TEXT STACK ----------
            StackPanel textStack = new StackPanel { Margin = new Thickness(0, 4, 0, 0) };

            // Product Name
            textStack.Children.Add(new TextBlock
            {
                Text = name,
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)), // Deep slate
                FontFamily = new FontFamily("Segoe UI"),
                Margin = new Thickness(0, 0, 0, 8)
            });

            // Category
            textStack.Children.Add(new TextBlock
            {
                Text = $"Category: {category}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)), // Muted slate
                FontFamily = new FontFamily("Segoe UI"),
                Margin = new Thickness(0, 0, 0, 4)
            });

            // Model
            textStack.Children.Add(new TextBlock
            {
                Text = $"Model: {model}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                FontFamily = new FontFamily("Segoe UI"),
                Margin = new Thickness(0, 0, 0, 8)
            });

            // Price
            textStack.Children.Add(new TextBlock
            {
                Text = $"{price:C}",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(16, 185, 129)), // Emerald green
                FontFamily = new FontFamily("Segoe UI"),
                Margin = new Thickness(0, 0, 0, 10)
            });

            // Stock Status
            if (quantity == 0 || quantity < 10)
            {
                Color bgColor = quantity == 0
                    ? Color.FromRgb(239, 68, 68)    // Soft red
                    : Color.FromRgb(251, 146, 60);  // Warm orange
                string warningText = quantity == 0 ? "Out of stock" : "Low stock";

                Border alertBorder = new Border
                {
                    Background = new SolidColorBrush(bgColor),
                    BorderThickness = new Thickness(0),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10, 6, 10, 6),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 0, 0, 4),
                    MaxWidth = 220
                };

                TextBlock alertText = new TextBlock
                {
                    Text = $"{quantity} units · {warningText}",
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 13,
                    FontFamily = new FontFamily("Segoe UI"),
                    TextAlignment = TextAlignment.Center
                };

                alertBorder.Child = alertText;
                textStack.Children.Add(alertBorder);
            }
            else
            {
                Border stockBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(236, 253, 245)), // Light green bg
                    BorderBrush = new SolidColorBrush(Color.FromRgb(16, 185, 129)), // Green border
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10, 6, 10, 6),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 0, 0, 4)
                };

                TextBlock okText = new TextBlock
                {
                    Text = $"{quantity} units in stock",
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Color.FromRgb(5, 150, 105)), // Deep green
                    FontWeight = FontWeights.SemiBold,
                    FontFamily = new FontFamily("Segoe UI")
                };

                stockBorder.Child = okText;
                textStack.Children.Add(stockBorder);
            }

            Grid.SetColumn(textStack, 0);
            contentGrid.Children.Add(textStack);

            // ---------- BUTTONS STACK ----------
            StackPanel buttonsStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed
            };

            buttonsStack.Children.Add(CreateButtonWithHoverText(CardButtonsFactory.CreateEditButton(BtnEdit_Click, id), "Edit"));
            buttonsStack.Children.Add(CreateButtonWithHoverText(CardButtonsFactory.CreateTransferButton(BtnTransfer_Click, id), "Transfer"));
            buttonsStack.Children.Add(CreateButtonWithHoverText(CardButtonsFactory.CreateDeleteButton(BtnDelete_Click, id), "Delete"));

            Grid.SetColumn(buttonsStack, 1);
            contentGrid.Children.Add(buttonsStack);

            Grid.SetRow(contentGrid, 1);
            rootGrid.Children.Add(contentGrid);

            cardBorder.Child = rootGrid;

            // ---------- CARD HOVER ANIMATION ----------
            cardBorder.MouseEnter += (s, e) =>
            {
                // Scale animation
                var tg = (TransformGroup)cardBorder.RenderTransform;
                var scale = (ScaleTransform)tg.Children.OfType<ScaleTransform>().First();

                var scaleAnim = new DoubleAnimation(1.0002, TimeSpan.FromMilliseconds(350))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);

                // Background color
                var bgAnim = new ColorAnimation(
                    Color.FromRgb(255, 255, 255),
                    TimeSpan.FromMilliseconds(300))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                cardBorder.Background.BeginAnimation(SolidColorBrush.ColorProperty, bgAnim);

                // 🔹 Border color (hover)
                var borderAnim = new ColorAnimation(
                    Color.FromRgb(155, 155, 155),
                    TimeSpan.FromMilliseconds(300))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                cardBorder.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, borderAnim);

                buttonsStack.Visibility = Visibility.Visible;

                // Shadow
                if (cardBorder.Effect is DropShadowEffect shadow)
                {
                    shadow.BlurRadius = 20;
                    shadow.ShadowDepth = 4;
                    shadow.Opacity = 0.25;
                }
            };


            cardBorder.MouseLeave += (s, e) =>
            {
                var tg = (TransformGroup)cardBorder.RenderTransform;
                var scale = (ScaleTransform)tg.Children.OfType<ScaleTransform>().First();

                var scaleAnim = new DoubleAnimation(1, TimeSpan.FromMilliseconds(350))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);

                // Background reset
                var bgAnim = new ColorAnimation(
                    Color.FromRgb(250, 251, 252),
                    TimeSpan.FromMilliseconds(300))
                {
                    EasingFunction = new QuadraticEase()
                };
                cardBorder.Background.BeginAnimation(SolidColorBrush.ColorProperty, bgAnim);

                // 🔹 Border reset
                var borderAnim = new ColorAnimation(
                    Color.FromRgb(226, 232, 240), // original soft blue-gray
                    TimeSpan.FromMilliseconds(300))
                {
                    EasingFunction = new QuadraticEase()
                };
                cardBorder.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, borderAnim);

                buttonsStack.Visibility = Visibility.Collapsed;

                // Shadow reset
                if (cardBorder.Effect is DropShadowEffect shadow)
                {
                    shadow.BlurRadius = 12;
                    shadow.ShadowDepth = 2;
                    shadow.Opacity = 0.15;
                }
            };


            // ---------- CARD LOAD ANIMATION ----------
            cardBorder.Loaded += (s, e) =>
            {
                var opacityAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200))
                { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut } };
                cardBorder.BeginAnimation(OpacityProperty, opacityAnim);

                var tg = (TransformGroup)cardBorder.RenderTransform;
                var tt = (TranslateTransform)tg.Children.OfType<TranslateTransform>().First();
                var slideAnim = new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(100))
                { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut } };
                tt.BeginAnimation(TranslateTransform.YProperty, slideAnim);
            };

            return cardBorder;
        }

        // ------------------- WAREHOUSE COLORS -------------------
        private SolidColorBrush BrushFromHex(string hex)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hex))
                    return new SolidColorBrush(Color.FromRgb(99, 110, 114));

                return (SolidColorBrush)new BrushConverter().ConvertFromString(hex);
            }
            catch
            {
                return new SolidColorBrush(Color.FromRgb(99, 110, 114));
            }
        }

        // ================= ANIMATIONS =================
        private void Card_MouseEnterAnimations(Border card, StackPanel buttons)
        {
            ScaleTransform scale = new ScaleTransform(1, 1);
            card.RenderTransformOrigin = new Point(0.5, 0.5);
            card.RenderTransform = scale;

            scale.BeginAnimation(ScaleTransform.ScaleXProperty,
                new DoubleAnimation(1, 1.01, TimeSpan.FromMilliseconds(250)));
            scale.BeginAnimation(ScaleTransform.ScaleYProperty,
                new DoubleAnimation(1, 1.01, TimeSpan.FromMilliseconds(250)));

            DropShadowEffect shadow = new DropShadowEffect
            {
                BlurRadius = 18,
                ShadowDepth = 0,
                Opacity = 0.28
            };
            card.Effect = shadow;

            buttons.Visibility = Visibility.Visible;
            buttons.BeginAnimation(StackPanel.OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)));
        }

        private void Card_MouseLeaveAnimations(Border card, StackPanel buttons)
        {
            if (card.RenderTransform is ScaleTransform scale)
            {
                scale.BeginAnimation(ScaleTransform.ScaleXProperty,
                    new DoubleAnimation(1.01, 1, TimeSpan.FromMilliseconds(250)));
                scale.BeginAnimation(ScaleTransform.ScaleYProperty,
                    new DoubleAnimation(1.01, 1, TimeSpan.FromMilliseconds(250)));
            }

            if (card.Effect is DropShadowEffect shadow)
            {
                shadow.BeginAnimation(DropShadowEffect.OpacityProperty,
                    new DoubleAnimation(0.28, 0, TimeSpan.FromMilliseconds(220)));
            }

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            fadeOut.Completed += (s, e) => buttons.Visibility = Visibility.Collapsed;
            buttons.BeginAnimation(StackPanel.OpacityProperty, fadeOut);
        }

        // ================= HELPERS =================
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

            double scrollbarWidth =
                CardScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible
                ? SystemParameters.VerticalScrollBarWidth
                : 0;

            double availableWidth =
                CardScrollViewer.ActualWidth
                - wrapPanel.Margin.Left
                - wrapPanel.Margin.Right
                - scrollbarWidth;

            int count = wrapPanel.Children.Count;

            foreach (Border card in wrapPanel.Children)
            {
                if (count == 1)
                    card.Width = availableWidth;
                else if (count == 2)
                    card.Width = (availableWidth / 2) - 16;
                else
                    card.Width = availableWidth > 820
                        ? (availableWidth / 2) - 16
                        : availableWidth - 16;
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

            await LoadProductsAsync();
            UpdateCardWidths();
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            frmAddEditProduct addProductWindow = new frmAddEditProduct(); // Add mode
            addProductWindow.Owner = Application.Current.MainWindow;
            addProductWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addProductWindow.ShowDialog();
        }

        // ================= BUTTON EVENTS =================
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            int productId = (int)((Button)sender).Tag;
            // Open edit product window
        }

        private void BtnTransfer_Click(object sender, RoutedEventArgs e)
        {
            int productId = (int)((Button)sender).Tag;
            // Open transfer window
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            int productId = (int)((Button)sender).Tag;

            if (MessageBox.Show("Delete this product?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                clsProduct.Delete(productId);
                await LoadProductsAsync();
            }
        }

        private async void cmbWarehouse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return; // Skip until loaded

            // Get selected warehouse name
            string warehouseName = "All Warehouses";

            if (cmbWarehouse.SelectedItem is ComboBoxItem selectedItem)
                warehouseName = selectedItem.Content.ToString();
            else if (cmbWarehouse.SelectedItem is string s)
                warehouseName = s;

            // Update the main title
            txtTitle.Text = warehouseName == "All Warehouses"
                ? "Product Management - All"
                : $"Product Management - {warehouseName}";

            // Update the message text dynamically
            NoProductsMessage.Text = warehouseName == "All Warehouses"
                ? "No products in any warehouse"
                : $"No products in {warehouseName}";

            // Reload products
            await LoadProductsAsync();
        }

    }
}
