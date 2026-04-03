using POS_BLL;
using POS_WPF.Serie;
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

namespace POS_WPF.Pages
{
    public partial class SeriePage : UserControl
    {
        private bool _isLoadingWarehouseFilter = false;
        public bool IsSidebarToggling { get; set; } = false;

        public SeriePage()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
            this.SizeChanged += UserControl_SizeChanged;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadWarehousesToComboBoxAsync();
            await LoadSeriesAsync();
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
                txtSearch?.Text?.ToLower() ?? "");
        }

        private void StartLoadingAnimation()
        {
            if (SpinnerRotate != null)
            {
                SpinnerRotate.BeginAnimation(RotateTransform.AngleProperty,
                    new DoubleAnimation
                    {
                        From = 0,
                        To = 360,
                        Duration = TimeSpan.FromSeconds(0.8),
                        RepeatBehavior = RepeatBehavior.Forever
                    });
            }
        }

        private void StopLoadingAnimation()
        {
            SpinnerRotate?.BeginAnimation(RotateTransform.AngleProperty, null);
        }

        // ======================= LOAD WAREHOUSES =======================
        private async Task LoadWarehousesToComboBoxAsync()
        {
            try
            {
                if (LoadingOverlay != null)
                {
                    LoadingOverlay.Visibility = Visibility.Visible;
                    StartLoadingAnimation();
                }

                if (txtTitle != null)
                    txtTitle.Text = "Loading...";

                await Task.Delay(50);

                DataTable dt = await Task.Run(() => clsWareHouse.GetAll());

                cmbWarehouse.Items.Clear();
                cmbWarehouse.Items.Add("All Warehouses");

                foreach (DataRow row in dt.Rows)
                {
                    cmbWarehouse.Items.Add(new ComboBoxItem
                    {
                        Content = row["Name"].ToString(),
                        Tag = row["WarehouseID"]
                    });
                }

                cmbWarehouse.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                if (txtTitle != null)
                    txtTitle.Text = "Serie Management - Error";

                MessageBox.Show("Error loading warehouses: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (LoadingOverlay != null)
                {
                    await Task.Delay(100);
                    StopLoadingAnimation();
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                }
            }
        }

        // ======================= LOAD SERIES =======================
        public async Task LoadSeriesAsync(int? warehouseID = null)
        {
            try
            {
                if (LoadingOverlay != null)
                    LoadingOverlay.Visibility = Visibility.Visible;

                if (txtTitle != null)
                    txtTitle.Text = "Loading...";

                await Task.Delay(50);

                string filter = GetSearchText();
                string warehouseName = "All Warehouses";

                // Resolve warehouse from ComboBox if not passed
                if (warehouseID == null && cmbWarehouse != null)
                {
                    if (cmbWarehouse.SelectedIndex > 0 &&
                        cmbWarehouse.SelectedItem is ComboBoxItem selectedItem)
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
                        if (obj is ComboBoxItem item &&
                            int.TryParse(item.Tag?.ToString(), out int id) &&
                            id == warehouseID)
                        {
                            warehouseName = item.Content.ToString();
                            break;
                        }
                    }
                }

                // Fetch + filter off UI thread
                var serieData = await Task.Run(() =>
                {
                    DataTable dt = warehouseID == null
                        ? clsSeries.GetAll()
                        : clsSeries.GetByWarehouseID(warehouseID.Value);

                    return dt.AsEnumerable()
                        .Where(row =>
                            row["Name"].ToString().ToLower().Contains(filter) ||
                            row["Description"].ToString().ToLower().Contains(filter))
                        .Select(row => new
                        {
                            SerieID = Convert.ToInt32(row["SeriesID"]),
                            Name = row["Name"].ToString(),
                            Description = row["Description"].ToString(),
                            BrandName = row["BrandName"].ToString()
                        })
                        .ToList();
                });

                // Build cards
                DynamicCardContainer.Items.Clear();
                double cardWidth = GetCardWidth();

                foreach (var item in serieData)
                {
                    DynamicCardContainer.Items.Add(
                        CreateSerieCard(item.SerieID, item.Name, item.Description, item.BrandName, cardWidth));
                }

                // Fix WrapPanel layout
                var wrapPanel = FindVisualChild<WrapPanel>(DynamicCardContainer);
                if (wrapPanel != null)
                {
                    wrapPanel.Margin = new Thickness(14);
                    wrapPanel.HorizontalAlignment = HorizontalAlignment.Left;
                    wrapPanel.VerticalAlignment = VerticalAlignment.Top;
                }

                UpdateCardWidths();

                if (txtTitle != null)
                    txtTitle.Text = $"Serie Management - {warehouseName}";

                if (txtTotalSeries != null)
                    txtTotalSeries.Text = $"Total Series : {serieData.Count}";

                if (NoSeriesMessage != null)
                {
                    NoSeriesMessage.Text = $"No series for {warehouseName}";
                    NoSeriesMessage.Visibility = serieData.Count == 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                if (txtTitle != null)
                    txtTitle.Text = "Serie Management - Error";

                MessageBox.Show("Error loading series: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        // ======================= CARD FACTORY =======================
        private Border CreateSerieCard(int id, string name, string description, string brandName, double width)
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
                CacheMode = new BitmapCache()
            };

            cardBorder.Effect = new DropShadowEffect
            {
                Color = Color.FromRgb(148, 163, 184),
                BlurRadius = 12,
                ShadowDepth = 2,
                Opacity = 0.15
            };

            // Layout grid
            Grid cardGrid = new Grid();
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // ── Left : serie initial badge + text ──
            StackPanel textStack = new StackPanel();

            StackPanel badgeAndNameStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 4)
            };

            // Coloured initial badge
            Border badge = new Border
            {
                Width = 54,
                Height = 54,
                Margin = new Thickness(8, 0, 18, 0),
                Background = new SolidColorBrush(Color.FromRgb(255, 140, 66)),
                CornerRadius = new CornerRadius(10)
            };
            badge.Child = new TextBlock
            {
                Text = name.Length > 0 ? name[0].ToString().ToUpper() : "S",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            badgeAndNameStack.Children.Add(badge);

            // Name + Brand pill
            StackPanel nameAndBrand = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            nameAndBrand.Children.Add(new TextBlock
            {
                Text = name,
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                FontFamily = new FontFamily("Segoe UI")
            });

            // Brand pill badge
            bool hasBrand = !string.IsNullOrWhiteSpace(brandName);
            Border brandPill = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 243, 232)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(255, 140, 66)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 2, 8, 2),
                Margin = new Thickness(0, 4, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            brandPill.Child = new TextBlock
            {
                Text = hasBrand ? brandName : "No Brand",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 80, 0)),
                FontFamily = new FontFamily("Segoe UI"),
                FontStyle = hasBrand ? FontStyles.Normal : FontStyles.Italic
            };
            nameAndBrand.Children.Add(brandPill);

            badgeAndNameStack.Children.Add(nameAndBrand);
            textStack.Children.Add(badgeAndNameStack);

            // Description
            bool hasDesc = !string.IsNullOrWhiteSpace(description);
            textStack.Children.Add(new TextBlock
            {
                Text = hasDesc ? description : "No description available",
                FontSize = 14,
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(
                    hasDesc ? Color.FromRgb(100, 116, 139) : Color.FromRgb(148, 163, 184)),
                FontFamily = new FontFamily("Segoe UI"),
                LineHeight = 20,
                FontStyle = hasDesc ? FontStyles.Normal : FontStyles.Italic
            });

            cardGrid.Children.Add(textStack);

            // ── Right : action buttons ──
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

            // ── Entrance animation ──
            cardBorder.Loaded += (s, e) =>
            {
                if (IsSidebarToggling)
                {
                    cardBorder.Opacity = 1;
                    ((TranslateTransform)cardBorder.RenderTransform).Y = 0;
                    return;
                }

                TranslateTransform translate;
                ScaleTransform scale;
                TransformGroup tg;

                if (cardBorder.RenderTransform is TransformGroup group)
                {
                    tg = group;
                    translate = group.Children.OfType<TranslateTransform>().FirstOrDefault()
                                ?? new TranslateTransform(0, 20);
                    scale = group.Children.OfType<ScaleTransform>().FirstOrDefault()
                                ?? new ScaleTransform(1, 1);
                    if (!tg.Children.Contains(translate)) tg.Children.Add(translate);
                    if (!tg.Children.Contains(scale)) tg.Children.Add(scale);
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

                var ease = new QuadraticEase { EasingMode = EasingMode.EaseOut };

                translate.BeginAnimation(TranslateTransform.YProperty,
                    new DoubleAnimation(0, TimeSpan.FromMilliseconds(400)) { EasingFunction = ease });

                cardBorder.BeginAnimation(Border.OpacityProperty,
                    new DoubleAnimation(1, TimeSpan.FromMilliseconds(400)) { EasingFunction = ease });
            };

            // ── Hover ──
            cardBorder.MouseEnter += (s, e) => Card_MouseEnterAnimations(cardBorder, buttonStack);
            cardBorder.MouseLeave += (s, e) => Card_MouseLeaveAnimations(cardBorder, buttonStack);

            // ── Scroll passthrough ──
            cardBorder.PreviewMouseWheel += (s, e) =>
            {
                CardScrollViewer?.ScrollToVerticalOffset(
                    CardScrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            };

            return cardBorder;
        }

        // ======================= Hover Animations =======================
        private void Card_MouseEnterAnimations(Border cardBorder, StackPanel buttonStack)
        {
            ScaleTransform scale;
            TransformGroup tg;

            if (cardBorder.RenderTransform is TransformGroup group)
            {
                tg = group;
                scale = group.Children.OfType<ScaleTransform>().FirstOrDefault();
                if (scale == null) { scale = new ScaleTransform(1, 1); tg.Children.Add(scale); }
            }
            else
            {
                scale = new ScaleTransform(1, 1);
                var tr = cardBorder.RenderTransform as TranslateTransform ?? new TranslateTransform(0, 0);
                tg = new TransformGroup();
                tg.Children.Add(scale);
                tg.Children.Add(tr);
                cardBorder.RenderTransform = tg;
            }

            cardBorder.RenderTransformOrigin = new Point(0.5, 0.5);

            var ease = new QuadraticEase { EasingMode = EasingMode.EaseOut };
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.01, TimeSpan.FromMilliseconds(250)) { EasingFunction = ease });
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.01, TimeSpan.FromMilliseconds(250)) { EasingFunction = ease });

            ((SolidColorBrush)cardBorder.BorderBrush).BeginAnimation(
                SolidColorBrush.ColorProperty,
                new ColorAnimation(Color.FromRgb(155, 155, 155), TimeSpan.FromMilliseconds(350)));

            if (cardBorder.Effect is DropShadowEffect shadow)
            {
                shadow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, new DoubleAnimation(18, TimeSpan.FromMilliseconds(260)));
                shadow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.28, TimeSpan.FromMilliseconds(260)));
            }

            buttonStack.Visibility = Visibility.Visible;
            buttonStack.BeginAnimation(StackPanel.OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)));
        }

        private void Card_MouseLeaveAnimations(Border cardBorder, StackPanel buttonStack)
        {
            if (cardBorder.RenderTransform is TransformGroup tg)
            {
                var scale = tg.Children.OfType<ScaleTransform>().FirstOrDefault();
                scale?.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(250)));
                scale?.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(250)));
            }

            ((SolidColorBrush)cardBorder.BorderBrush).BeginAnimation(
                SolidColorBrush.ColorProperty,
                new ColorAnimation(Color.FromRgb(223, 230, 233), TimeSpan.FromMilliseconds(350)));

            if (cardBorder.Effect is DropShadowEffect shadow)
            {
                shadow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, new DoubleAnimation(12, TimeSpan.FromMilliseconds(260)));
                shadow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.15, TimeSpan.FromMilliseconds(260)));
            }

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(220));
            fadeOut.Completed += (s, e) => buttonStack.Visibility = Visibility.Collapsed;
            buttonStack.BeginAnimation(StackPanel.OpacityProperty, fadeOut);
        }

        // ======================= Helpers =======================
        private double GetCardWidth()
        {
            var wp = FindVisualChild<WrapPanel>(DynamicCardContainer);
            if (wp == null) return 400;
            double avail = wp.ActualWidth - wp.Margin.Left - wp.Margin.Right;
            return avail > 900 ? 420 : avail - 20;
        }

        private void UpdateCardWidths()
        {
            var wp = FindVisualChild<WrapPanel>(DynamicCardContainer);
            if (wp == null) return;

            int count = wp.Children.Count;
            double avail = CardScrollViewer.ActualWidth - wp.Margin.Left - wp.Margin.Right - 16;

            foreach (var child in wp.Children)
            {
                if (child is Border card)
                {
                    card.Width = count == 1 ? avail
                               : count == 2 ? (avail / 2) - 16
                               : avail > 760 ? (avail / 2) - 16 : avail - 16;
                }
            }
        }

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

        // ======================= Events =======================
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
            => UpdateCardWidths();

        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtPlaceholder.Visibility = string.IsNullOrEmpty(txtSearch.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            await LoadSeriesAsync();
            UpdateCardWidths();
        }

        private async void cmbWarehouse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingWarehouseFilter) return;
            await LoadSeriesAsync();
        }

        private async void BtnAddSerie_Click(object sender, RoutedEventArgs e)
        {
            var win = new frmAddEditSerie();
            win.Owner = Application.Current.MainWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.ShowDialog();
            if (win.IsSaved)
                await LoadSeriesAsync();
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int serieId)
            {
                var win = new frmAddEditSerie(serieId);
                win.Owner = Application.Current.MainWindow;
                win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                win.ShowDialog();
                if (win.IsSaved)
                    await LoadSeriesAsync();
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int serieId)
            {
                var result = MessageBox.Show(
                    "This will permanently delete this serie.\n\nDo you want to proceed?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (clsSeries.Delete(serieId))
                    {
                        MessageBox.Show("Serie deleted successfully.",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadSeriesAsync();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Error deleting serie. It might be linked to other records.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
