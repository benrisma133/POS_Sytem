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
using POS_WPF.Models;

namespace POS_WPF.Pages
{
    public partial class ModelPage : UserControl
    {
        private bool _isLoadingWarehouseFilter = false;

        public bool IsSidebarToggling { get; set; } = false;
        public ModelPage()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
            this.SizeChanged += UserControl_SizeChanged;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadWarehousesToComboBoxAsync();
            await LoadItemsAsync();
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

        public async Task LoadItemsAsync(int? warehouseID = null)
        {
            try
            {
                if (LoadingOverlay != null)
                    LoadingOverlay.Visibility = Visibility.Visible;

                if (txtTitle != null)
                    txtTitle.Text = "Loading...";

                // Get search text off UI thread
                string filter = (await Task.Run(() => GetSearchText()))?.ToLower() ?? "";

                // Determine warehouse if not provided
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

                // Load models off UI thread
                DataTable dtModels = await Task.Run(() =>
                {
                    if (warehouseID == null)
                        return clsModel.GetAll();
                    else
                        return clsModel.GetByWarehouseID(warehouseID.Value);
                });

                // Filter rows off UI thread
                var filteredRows = await Task.Run(() =>
                    dtModels.AsEnumerable()
                        .Where(row =>
                            row["Name"].ToString().ToLower().Contains(filter) ||
                            row["Description"].ToString().ToLower().Contains(filter))
                        .ToList()
                );

                // Update UI on main thread
                DynamicCardContainer.Dispatcher.Invoke(() =>
                {
                    DynamicCardContainer.Items.Clear();
                    double cardWidth = GetCardWidth();

                    foreach (var row in filteredRows)
                    {
                        int id = Convert.ToInt32(row["ModelID"]);
                        string name = row["Name"].ToString();
                        string description = row["Description"].ToString();
                        string seriesName = row["SeriesName"] != DBNull.Value ? row["SeriesName"].ToString() : "";
                        string brandName = row["BrandName"] != DBNull.Value ? row["BrandName"].ToString() : "";

                        DynamicCardContainer.Items.Add(
                            CreateModelCard(id, name, description, seriesName, brandName,cardWidth)
                        );
                    }

                    // Show message if no models
                    if (NoModelsMessage != null)
                    {
                        NoModelsMessage.Text = $"No models for {warehouseName}";
                        NoModelsMessage.Visibility = filteredRows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                    }

                    // Fix WrapPanel alignment
                    var wrapPanel = FindVisualChild<WrapPanel>(DynamicCardContainer);
                    if (wrapPanel != null)
                    {
                        wrapPanel.Margin = new Thickness(14);
                        wrapPanel.HorizontalAlignment = HorizontalAlignment.Left;
                        wrapPanel.VerticalAlignment = VerticalAlignment.Top;
                    }

                    UpdateCardWidths();

                    // Update title
                    if (txtTitle != null)
                        txtTitle.Text = $"Model Management - {warehouseName}";

                    // Update total models
                    if (txtTotalModels != null)
                        txtTotalModels.Text = $"Total Models: {filteredRows.Count}";
                });
            }
            finally
            {
                if (LoadingOverlay != null)
                    LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private double GetCardWidth()
        {
            var wrapPanel = FindVisualChild<WrapPanel>(DynamicCardContainer);
            if (wrapPanel == null) return 400;
            double availableWidth = wrapPanel.ActualWidth - wrapPanel.Margin.Left - wrapPanel.Margin.Right;
            return availableWidth > 900 ? 420 : availableWidth - 20;
        }

        private Border CreateModelCard(int id, string name, string description, string seriesName, string brandName, double width)
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

            StackPanel textStack = new StackPanel();

            // ── Badge + Name row ──
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
                Background = new SolidColorBrush(Color.FromRgb(16, 185, 129)),
                CornerRadius = new CornerRadius(10)
            };
            badge.Child = new TextBlock
            {
                Text = name.Length > 0 ? name[0].ToString().ToUpper() : "M",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            badgeAndNameStack.Children.Add(badge);

            // Name + pills column
            StackPanel nameAndPills = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            nameAndPills.Children.Add(new TextBlock
            {
                Text = name,
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                FontFamily = new FontFamily("Segoe UI")
            });

            // Pills row (Series + Brand side by side)
            StackPanel pillsRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 4, 0, 0)
            };

            // Series pill
            bool hasSeries = !string.IsNullOrWhiteSpace(seriesName);
            Border seriesPill = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(232, 244, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(66, 153, 225)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 2, 8, 2),
                Margin = new Thickness(0, 0, 6, 0)
            };
            seriesPill.Child = new TextBlock
            {
                Text = hasSeries ? seriesName : "No Series",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 80, 160)),
                FontFamily = new FontFamily("Segoe UI"),
                FontStyle = hasSeries ? FontStyles.Normal : FontStyles.Italic
            };
            pillsRow.Children.Add(seriesPill);

            // Brand pill
            bool hasBrand = !string.IsNullOrWhiteSpace(brandName);
            Border brandPill = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 243, 232)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(255, 140, 66)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 2, 8, 2)
            };
            brandPill.Child = new TextBlock
            {
                Text = hasBrand ? brandName : "No Brand",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 80, 0)),
                FontFamily = new FontFamily("Segoe UI"),
                FontStyle = hasBrand ? FontStyles.Normal : FontStyles.Italic
            };
            pillsRow.Children.Add(brandPill);

            nameAndPills.Children.Add(pillsRow);
            badgeAndNameStack.Children.Add(nameAndPills);
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

            // ── Action buttons ──
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

            // ── Entrance animation (fade + slide) ──
            cardBorder.Loaded += (s, e) =>
            {
                var fadeAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                cardBorder.BeginAnimation(Border.OpacityProperty, fadeAnim);

                ((TranslateTransform)cardBorder.RenderTransform).BeginAnimation(
                    TranslateTransform.YProperty,
                    new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(400))
                    {
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    });
            };

            // ── Hover ──
            cardBorder.MouseEnter += (s, e) => Card_MouseEnterAnimations(cardBorder, buttonStack);
            cardBorder.MouseLeave += (s, e) => Card_MouseLeaveAnimations(cardBorder, buttonStack);

            // ── Scroll passthrough ──
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

        private void Card_MouseEnterAnimations(Border cardBorder, StackPanel buttonStack)
        {
            // Background color
            var colorAnim = new ColorAnimation(
                Color.FromRgb(255, 255, 255),
                TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            cardBorder.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

            // Shadow
            if (cardBorder.Effect is DropShadowEffect shadow)
            {
                shadow.BlurRadius = 20;
                shadow.ShadowDepth = 4;
                shadow.Opacity = 0.25;
            }

            // Scale
            if (!(cardBorder.RenderTransform is ScaleTransform))
            {
                cardBorder.RenderTransform = new ScaleTransform(1, 1);
                cardBorder.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            var scale = (ScaleTransform)cardBorder.RenderTransform;
            var scaleAnim = new DoubleAnimation(1.01, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);

            // Border color
            var borderAnim = new ColorAnimation(
                Color.FromRgb(155, 155, 155),
                TimeSpan.FromMilliseconds(350))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            ((SolidColorBrush)cardBorder.BorderBrush)
                .BeginAnimation(SolidColorBrush.ColorProperty, borderAnim);

            // Buttons
            buttonStack.Visibility = Visibility.Visible;
            buttonStack.BeginAnimation(
                StackPanel.OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)));
        }

        private void Card_MouseLeaveAnimations(Border cardBorder, StackPanel buttonStack)
        {
            // Background reset
            var colorAnim = new ColorAnimation(
                Color.FromRgb(250, 251, 252),
                TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new QuadraticEase()
            };
            cardBorder.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

            // Scale back
            if (cardBorder.RenderTransform is ScaleTransform scale)
            {
                var scaleAnim = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(250))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
            }

            // Border reset
            var borderAnim = new ColorAnimation(
                Color.FromRgb(223, 230, 233),
                TimeSpan.FromMilliseconds(350))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            ((SolidColorBrush)cardBorder.BorderBrush)
                .BeginAnimation(SolidColorBrush.ColorProperty, borderAnim);

            // Buttons fade out
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(220));
            fadeOut.Completed += (s, e) =>
                buttonStack.Visibility = Visibility.Collapsed;

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
            double availableWidth = CardScrollViewer.ActualWidth - wrapPanel.Margin.Left - wrapPanel.Margin.Right - 16;

            foreach (var child in wrapPanel.Children)
            {
                if (child is Border card)
                {
                    if (cardCount == 1)
                        card.Width = availableWidth;
                    else if (cardCount == 2)
                        card.Width = (availableWidth / 2) - 16;
                    else
                        card.Width = availableWidth > 760 ? (availableWidth / 2) - 16 : availableWidth - 20;
                }
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateCardWidths();

        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtPlaceholder.Visibility = string.IsNullOrEmpty(txtSearch.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            await LoadItemsAsync();
            UpdateCardWidths();
        }

        private async void BtnAddModel_Click(object sender, RoutedEventArgs e)
        {
            frmAddEditModel addModelWindow = new frmAddEditModel(); // implement your add/edit form
            addModelWindow.Owner = Application.Current.MainWindow;
            addModelWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addModelWindow.ShowDialog();
            if (addModelWindow.IsSaved)
            {
                await LoadItemsAsync();
            }
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int modelId = (int)btn.Tag;
                frmAddEditModel frmAddEditModel = new frmAddEditModel(modelId); // implement edit constructor
                frmAddEditModel.Owner = Application.Current.MainWindow;
                frmAddEditModel.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                frmAddEditModel.ShowDialog();
                if (frmAddEditModel.IsSaved)
                {
                    await LoadItemsAsync();
                }
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int modelID = (int)btn.Tag;

                // Inform the user clearly about deletion scope
                var result = MessageBox.Show(
                    "This action will permanently delete this model from ALL warehouses.\n\n" +
                    "If you only want to remove it from the selected warehouse, please unselect it instead.\n\n" +
                    "Do you want to proceed?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (clsModel.DeleteCompletely(modelID)) // your delete logic
                    {
                        MessageBox.Show(
                            "Model has been deleted from all warehouses successfully.",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        await LoadItemsAsync(); // reload models after deletion
                    }
                    else
                    {
                        MessageBox.Show(
                            "Error deleting model. It might be linked to other records.",
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

            // Let LoadItemsAsync handle the warehouse ID and title
            await LoadItemsAsync();
        }

        private void cmbWarehouse_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //cmbWarehouse.IsDropDownOpen = !cmbWarehouse.IsDropDownOpen;
        }

    }

}
