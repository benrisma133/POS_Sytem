using POS_BLL;
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
using System.Windows.Shapes;
using System.Xml.Linq;


namespace POS_WPF.Pages
{
    public partial class WarehousePage : UserControl
    {

        // 🔐 Stored in memory (delegate results)
        private int _lastAddedWarehouseID = -1;
        private string _lastAddedWarehouseName = string.Empty;
        private bool _isFirstLoad = true;


        // Edit icon (pencil)
        private Geometry EditIcon = Geometry.Parse(
            "M20.1498 7.93997L8.27978 19.81C7.21978 20.88 4.04977 21.3699 3.32977 20.6599C2.60977 19.9499 3.11978 16.78 4.17978 15.71L16.0498 3.84C16.5979 3.31801 17.3283 3.03097 18.0851 3.04019C18.842 3.04942 19.5652 3.35418 20.1004 3.88938C20.6356 4.42457 20.9403 5.14781 20.9496 5.90463C20.9588 6.66146 20.6718 7.39189 20.1498 7.93997Z");

        // Delete icon (trash)
        private Geometry DeleteIcon = Geometry.Parse(
            "M10 12V17 M14 12V17 M4 7H20 M6 10V18C6 19.6569 7.34315 21 9 21H15C16.6569 21 18 19.6569 18 18V10 M9 5C9 3.89543 9.89543 3 11 3H13C14.1046 3 15 3.89543 15 5V7H9V5Z");


        public WarehousePage()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
            this.SizeChanged += UserControl_SizeChanged;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadWarehousesAsync();
            DynamicCardContainer.PreviewMouseWheel += DynamicCardContainer_PreviewMouseWheel;
        }

        private void DynamicCardContainer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //if (CardScrollViewer == null) return;
            //CardScrollViewer.ScrollToVerticalOffset(CardScrollViewer.VerticalOffset - e.Delta);
            //e.Handled = true;
        }

        private string GetSearchText()
        {
            return txtSearch.Dispatcher.Invoke(() =>
            {
                return txtSearch?.Text?.ToLower() ?? "";
            });
        }

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

        public async Task LoadWarehousesAsync()
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

                string filter = await Task.Run(() => GetSearchText());
                DataTable dt = await Task.Run(() => clsWareHouse.GetAll());

                var filteredRows = dt.AsEnumerable()
                    .Where(row =>
                        row["Name"].ToString().ToLower().Contains(filter) ||
                        row["Location"].ToString().ToLower().Contains(filter) ||
                        row["Description"].ToString().ToLower().Contains(filter))
                    .ToList();

                // Update UI on main thread
                DynamicCardContainer.Dispatcher.Invoke(() =>
                {
                    DynamicCardContainer.Items.Clear();
                    double cardWidth = GetCardWidth();

                    foreach (var row in filteredRows)
                    {
                        int id = Convert.ToInt32(row["WarehouseID"]);
                        string name = row["Name"].ToString();
                        string location = row["Location"].ToString();
                        string description = row["Description"].ToString();
                        // hex color
                        string cardColor = row.Table.Columns.Contains("Color") ? row["Color"].ToString() : "#6272A4";

                        var card = CreateWarehouseCard(id, name, location, description, cardColor, cardWidth);
                        DynamicCardContainer.Items.Add(card);
                    }

                    // Fix WrapPanel alignment after adding all cards
                    var wrapPanel = FindVisualChild<WrapPanel>(DynamicCardContainer);
                    if (wrapPanel != null)
                    {
                        wrapPanel.Margin = new Thickness(14);
                        wrapPanel.HorizontalAlignment = HorizontalAlignment.Left;
                        wrapPanel.VerticalAlignment = VerticalAlignment.Top;
                    }

                    UpdateCardWidths();

                    if (txtTitle != null)
                        txtTitle.Text = "Warehouse Management";

                    if (txtTotalWarehouses !=  null)
                        txtTotalWarehouses.Text = $"Total Warehouses: {filteredRows.Count}";

                    if (NoCategoriesMessage != null)
                    {
                        NoCategoriesMessage.Text = $"No wrehouses";
                        NoCategoriesMessage.Visibility = filteredRows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                    }

                });
            }
            catch (Exception ex)
            {
                if (txtTitle != null)
                    txtTitle.Text = "Warehouse Management - Error";

                MessageBox.Show("Error loading warehouses: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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


        private double GetCardWidth()
        {
            var wrapPanel = FindVisualChild<WrapPanel>(DynamicCardContainer);
            if (wrapPanel == null) return 400;
            double availableWidth = wrapPanel.ActualWidth - wrapPanel.Margin.Left - wrapPanel.Margin.Right;
            return availableWidth > 900 ? 420 : availableWidth - 20;
        }

        private SolidColorBrush BrushFromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return new SolidColorBrush(Color.FromRgb(99, 110, 114)); // fallback

            return (SolidColorBrush)new BrushConverter().ConvertFromString(hex);
        }

        private Border CreateWarehouseCard(int id, string name, string location, string description, string hexColor, double width)
        {
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
                CacheMode = new BitmapCache(),
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(148, 163, 184),
                    BlurRadius = 12,
                    ShadowDepth = 2,
                    Opacity = 0.15
                }
            };

            Grid cardGrid = new Grid();
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Text stack
            StackPanel textStack = new StackPanel();

            // Warehouse badge
            Border nameBadge = new Border
            {
                Background = BrushFromHex(hexColor),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Left,
                Effect = new DropShadowEffect { BlurRadius = 6, ShadowDepth = 1, Opacity = 0.2 }
            };
            nameBadge.Child = new TextBlock
            {
                Text = name,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Segoe UI")
            };
            textStack.Children.Add(nameBadge);

            // Location
            StackPanel locationPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            locationPanel.Children.Add(new TextBlock
            {
                Text = location,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
                FontWeight = FontWeights.Medium,
                FontFamily = new FontFamily("Segoe UI"),
                VerticalAlignment = VerticalAlignment.Center
            });
            textStack.Children.Add(locationPanel);

            // Description
            TextBlock descriptionBlock = new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(description) ? "No description available" : description,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                Foreground = string.IsNullOrWhiteSpace(description) ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                FontStyle = string.IsNullOrWhiteSpace(description) ? FontStyles.Italic : FontStyles.Normal,
                FontFamily = new FontFamily("Segoe UI"),
                LineHeight = 20
            };
            textStack.Children.Add(descriptionBlock);

            cardGrid.Children.Add(textStack);

            // Buttons stack
            StackPanel buttonStack = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, Visibility = Visibility.Collapsed };
            Grid.SetColumn(buttonStack, 1);

            Button editBtn = CardButtonsFactory.CreateEditButton(BtnEdit_Click, id);
            Button deleteBtn = CardButtonsFactory.CreateDeleteButton(BtnDelete_Click, id);
            deleteBtn.Margin = new Thickness(8, 0, 0, 0);
            buttonStack.Children.Add(editBtn);
            buttonStack.Children.Add(deleteBtn);

            cardGrid.Children.Add(buttonStack);
            cardBorder.Child = cardGrid;

            // Loaded animation
            cardBorder.Loaded += (s, e) =>
            {
                var fadeAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                cardBorder.BeginAnimation(Border.OpacityProperty, fadeAnim);

                ((TranslateTransform)cardBorder.RenderTransform).BeginAnimation(TranslateTransform.YProperty,
                    new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(400))
                    { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } });
            };

            // Crazy hover
            cardBorder.MouseEnter += (s, e) => Card_MouseEnterAnimations(cardBorder, buttonStack);
            cardBorder.MouseLeave += (s, e) => Card_MouseLeaveAnimations(cardBorder, buttonStack);

            // Scroll wheel
            cardBorder.PreviewMouseWheel += (s, e) =>
            {
                if (CardScrollViewer != null)
                {
                    CardScrollViewer.ScrollToVerticalOffset(CardScrollViewer.VerticalOffset - e.Delta);
                    e.Handled = true;
                }
            };

            return cardBorder;
        }

        // Hover animation (scale + shadow + border + buttons)
        private void Card_MouseEnterAnimations(Border cardBorder, StackPanel buttonStack)
        {
            ScaleTransform scale = new ScaleTransform(1, 1);
            cardBorder.RenderTransformOrigin = new Point(0.5, 0.5);
            cardBorder.RenderTransform = scale;

            var scaleAnim = new DoubleAnimation(1.01, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);

            ColorAnimation borderAnim = new ColorAnimation(Color.FromRgb(155, 155, 155), TimeSpan.FromMilliseconds(350));
            ((SolidColorBrush)cardBorder.BorderBrush).BeginAnimation(SolidColorBrush.ColorProperty, borderAnim);

            DropShadowEffect shadow = new DropShadowEffect { BlurRadius = 0, ShadowDepth = 0, Opacity = 0, Color = Colors.Black };
            cardBorder.Effect = shadow;
            shadow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, new DoubleAnimation(0, 20, TimeSpan.FromMilliseconds(260)));
            shadow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0, 0.28, TimeSpan.FromMilliseconds(260)));
            shadow.BeginAnimation(DropShadowEffect.ShadowDepthProperty, new DoubleAnimation(0, 4, TimeSpan.FromMilliseconds(260)));

            buttonStack.Visibility = Visibility.Visible;
            buttonStack.BeginAnimation(StackPanel.OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)));
        }

        // Leave animation (reset everything)
        private void Card_MouseLeaveAnimations(Border cardBorder, StackPanel buttonStack)
        {
            if (cardBorder.RenderTransform is ScaleTransform scale)
            {
                var scaleAnim = new DoubleAnimation(1, TimeSpan.FromMilliseconds(250))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
            }

            ColorAnimation borderAnim = new ColorAnimation(Color.FromRgb(223, 230, 233), TimeSpan.FromMilliseconds(350));
            ((SolidColorBrush)cardBorder.BorderBrush).BeginAnimation(SolidColorBrush.ColorProperty, borderAnim);

            if (cardBorder.Effect is DropShadowEffect shadow)
            {
                shadow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, new DoubleAnimation(shadow.BlurRadius, 12, TimeSpan.FromMilliseconds(260)));
                shadow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(shadow.Opacity, 0.15, TimeSpan.FromMilliseconds(260)));
                shadow.BeginAnimation(DropShadowEffect.ShadowDepthProperty, new DoubleAnimation(shadow.ShadowDepth, 2, TimeSpan.FromMilliseconds(260)));
            }

            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(220));
            fadeOut.Completed += (s2, e2) => buttonStack.Visibility = Visibility.Collapsed;
            buttonStack.BeginAnimation(StackPanel.OpacityProperty, fadeOut);
        }


        private WrapPanel GetWrapPanel() => FindVisualChild<WrapPanel>(DynamicCardContainer);

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    return t;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
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
                        card.Width = availableWidth > 800 ? (availableWidth / 2) - 16 : availableWidth - 16;
                    }
                }
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateCardWidths();

        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtPlaceholder.Visibility = string.IsNullOrEmpty(txtSearch.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            await LoadWarehousesAsync();
            UpdateCardWidths();
        }

        private async void BtnAddWarehouse_Click(object sender, RoutedEventArgs e)
        {
            frmAddEditWarehouse addWarehouseWindow = new frmAddEditWarehouse();
            addWarehouseWindow.Owner = Application.Current.MainWindow;
            addWarehouseWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // ✅ Delegate for testing
            addWarehouseWindow.OnWarehouseSaved = (id, name) =>
            {
                _lastAddedWarehouseID = id;
                _lastAddedWarehouseName = name;
            };

            addWarehouseWindow.ShowDialog();

            if(addWarehouseWindow.IsSaved)
            {

                await LoadWarehousesAsync();
                txtSearch.Text = _lastAddedWarehouseName;
            }

        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int warehouseId = (int)btn.Tag;
                frmAddEditWarehouse frmAddEditWarehouse = new frmAddEditWarehouse(warehouseId);
                frmAddEditWarehouse.Owner = Application.Current.MainWindow;
                frmAddEditWarehouse.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                frmAddEditWarehouse.ShowDialog();
                if (frmAddEditWarehouse.IsSaved)
                {

                    await LoadWarehousesAsync();
                    txtSearch.Text = clsWareHouse.FindByID(warehouseId).Name;
                }
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int warehouseId = (int)btn.Tag;
                var result = MessageBox.Show("Are you sure you want to delete this warehouse?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    if (clsWareHouse.Delete(warehouseId))
                    {
                        MessageBox.Show("Warehouse deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        if(txtSearch.Text.Length > 0)
                        {
                            txtSearch.Text = "";
                        }
                        await LoadWarehousesAsync();
                    }
                    else
                    {
                        MessageBox.Show("Error deleting warehouse. It might be linked to other records.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Deletion cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
