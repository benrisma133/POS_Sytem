using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;


namespace POS_WPF
{
    /// <summary>
    /// Interaction logic for TestSidebarWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Geometry HamburgerIcon = Geometry.Parse("M4 6H20 M4 12H20 M4 18H20");
        private Geometry CloseIcon = Geometry.Parse("M18.3 5.71L12 12l6.3 6.29-1.41 1.42L10.59 13.4l-6.3 6.31-1.41-1.42L9.17 12 2.88 5.71l1.41-1.42 6.3 6.3 6.29-6.3z");
        private Geometry LeftArrowIcon = Geometry.Parse("M11.7071 4.29289C12.0976 4.68342 12.0976 5.31658 11.7071 5.70711L6.41421 11H20C20.5523 11 21 11.4477 21 12C21 12.5523 20.5523 13 20 13H6.41421L11.7071 18.2929C12.0976 18.6834 12.0976 19.3166 11.7071 19.7071C11.3166 20.0976 10.6834 20.0976 10.2929 19.7071L3.29289 12.7071C3.10536 12.5196 3 12.2652 3 12C3 11.7348 3.10536 11.4804 3.29289 11.2929L10.2929 4.29289C10.6834 3.90237 11.3166 3.90237 11.7071 4.29289Z");

        private FrameworkElement _activeMenu = null;


        private readonly Brush MenuNormalBg = new SolidColorBrush(Color.FromRgb(74, 74, 74));      // #4A4A4A
        private readonly Brush MenuHoverBg  = new SolidColorBrush(Color.FromRgb(92, 92, 92));      // #5C5C5C
        private readonly Brush MenuActiveBg = new SolidColorBrush(Color.FromRgb(92, 92, 92));      // SAME AS HOVER
        private readonly Brush MenuActiveFg = new SolidColorBrush(Color.FromRgb(255, 140, 66));    // #FF8C42
        private readonly Brush MenuNormalFg = new SolidColorBrush(Color.FromRgb(245, 245, 245));   // #F5F5F5

        //Pages.CategoryPage categoryPage = new CategoryPage();
        //Pages.ModelPage modelPage = new ModelPage();

        private void SetActiveMenu(FrameworkElement clickedElement)
        {
            // Reset previous active
            if (_activeMenu is Button oldBtn)
            {
                oldBtn.ClearValue(Button.TagProperty);
            }

            // Set new active
            if (clickedElement is Button newBtn)
            {
                newBtn.Tag = "Active";
                _activeMenu = newBtn;
            }
            else
            {
                _activeMenu = null;
            }
        }

        public Geometry HamburgerPathData
        {
            get { return isSidebarOpen ? CloseIcon : HamburgerIcon; }
        }
        private void UpdateHamburgerIcon()
        {
            if (isSidebarOpen)
            {
                // Sidebar open → show left arrow (close)
                HamburgerPath.Data = LeftArrowIcon;
                HamburgerBtn.HorizontalAlignment = HorizontalAlignment.Right;
                HamburgerBtn.Margin = new Thickness(0, 10, 6, 0);
            }
            else
            {
                // Sidebar closed → show hamburger icon
                HamburgerPath.Data = HamburgerIcon;
                HamburgerBtn.HorizontalAlignment = HorizontalAlignment.Center;
                HamburgerBtn.Margin = new Thickness(0, 10, 0, 0);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateHamburgerIcon();
            SetActiveMenu(DashboardButton); // give Dashboard x:Name="DashboardButton"
        }

        private bool isSidebarOpen = true;

        private bool userClosedSidebar = false; // track if user manually closed

        private bool shouldRestoreDeptExpanded = false;

        private void HamburgerBtn_Click(object sender, RoutedEventArgs e)
        {
            // Sidebar is OPEN and user is closing it
            if (isSidebarOpen)
            {
                // Remember submenu state
                shouldRestoreDeptExpanded = isDeptExpanded;

                // Collapse submenu visually if open
                if (isDeptExpanded)
                {
                    CollapseDepartmentsSubmenu();
                }
            }
            else
            {
                // Sidebar is CLOSED and user is opening it
                ToggleSidebar();

                // Restore submenu if needed
                if (shouldRestoreDeptExpanded)
                {
                    ExpandDepartmentsSubmenu();
                }

                return;
            }

            ToggleSidebar();
            userClosedSidebar = !isSidebarOpen;
        }

        private void CollapseDepartmentsSubmenu()
        {
            double height = DepartmentsSubmenu.ActualHeight;

            var animation = new DoubleAnimation
            {
                From = height,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            DepartmentsSubmenuContainer.BeginAnimation(Border.HeightProperty, animation);

            CatalogArrow.Data = Geometry.Parse("M6 9L12 15L18 9"); // Down
            isDeptExpanded = false;
        }

        private void ExpandDepartmentsSubmenu()
        {
            double height = DepartmentsSubmenu.ActualHeight;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = height,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            animation.Completed += (s, _) =>
            {
                DepartmentsSubmenuContainer.Height = double.NaN;
            };

            DepartmentsSubmenuContainer.BeginAnimation(Border.HeightProperty, animation);

            CatalogArrow.Data = Geometry.Parse("M6 15L12 9L18 15"); // Up
            isDeptExpanded = true;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth < 1100)
            {
                HamburgerBtn.Visibility = Visibility.Collapsed;

                if (isSidebarOpen)
                    ToggleSidebar(forceCollapse: true); // force collapse
            }
            else
            {
                HamburgerBtn.Visibility = Visibility.Visible;

                // Only expand if user didn't manually close before
                if (!isSidebarOpen && !userClosedSidebar)
                    ToggleSidebar(forceCollapse: false);
            }
        }

        private void ToggleSidebar(bool forceCollapse = false)
        {
            // Tell pages: sidebar is moving
            //categoryPage.IsSidebarToggling = true;
            //modelPage.IsSidebarToggling = true;

            bool targetState = forceCollapse ? false : !isSidebarOpen;

            long Timing = 190;

            // -------- Profile --------
            ProfileTextStack.Visibility = targetState ? Visibility.Visible : Visibility.Collapsed;

            // Animate profile icon size
            ProfileIcon.BeginAnimation(WidthProperty,
                new DoubleAnimation(targetState ? 50 : 40, TimeSpan.FromMilliseconds(Timing)));
            ProfileIcon.BeginAnimation(HeightProperty,
                new DoubleAnimation(targetState ? 50 : 40, TimeSpan.FromMilliseconds(Timing)));

            // -------- Sidebar width --------
            double fromWidth = isSidebarOpen ? 280 : 60;
            double toWidth = targetState ? 280 : 60;

            var sidebarAnim = new DoubleAnimation
            {
                From = fromWidth,
                To = toWidth,
                Duration = TimeSpan.FromMilliseconds(Timing),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            Sidebar.BeginAnimation(WidthProperty, sidebarAnim);

            // -------- Main content margin --------
            var mainGrid = ((Grid)this.Content).Children[1] as Grid;
            if (mainGrid != null)
            {
                var marginAnim = new ThicknessAnimation
                {
                    From = mainGrid.Margin,
                    To = new Thickness(toWidth, 0, 0, 0),
                    Duration = TimeSpan.FromMilliseconds(Timing),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };
                mainGrid.BeginAnimation(Grid.MarginProperty, marginAnim);
            }

            // -------- Show/hide button texts --------
            foreach (var child in Sidebar.ChildOfType<StackPanel>().First().Children)
            {
                if (child is Button btn)
                {
                    foreach (var gridChild in btn.ContentOfType<Grid>())
                    {
                        if (gridChild.ColumnDefinitions.Count > 1)
                        {
                            var textBlock = gridChild.Children.OfType<TextBlock>().FirstOrDefault();
                            if (textBlock != null)
                                textBlock.Visibility = targetState ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                }
            }

            // -------- End of animation --------
            sidebarAnim.Completed += (s, e) =>
            {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Loaded,
                    new Action(() =>
                    {
                        //categoryPage.IsSidebarToggling = false;
                        //modelPage.IsSidebarToggling = false;
                    })
                );
            };


            // Update state
            isSidebarOpen = targetState;

            // Update hamburger icon
            UpdateHamburgerIcon();
        }

        private void Warehouse_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainContent.Children.Clear();
            MainContent.Children.Add(new Pages.WarehousePage());
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            // Comming soon - for demo, just change title
            MainContent.Children.Clear();
            MainContent.Children.Add(new TextBlock
            {
                Text = "Dashboard - Coming Soon",
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        private void Client_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            // Comming soon - for demo, just change title
            MainContent.Children.Clear();
            MainContent.Children.Add(new TextBlock
            {
                Text = "Client Page - Coming Soon",
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        private void NewSale_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            // Comming soon - for demo, just change title
            MainContent.Children.Clear();
            MainContent.Children.Add(new TextBlock
            {
                Text = "New Sale Page - Coming Soon",
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        private void Brands_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainContent.Children.Clear();
            MainContent.Children.Add(new Pages.BrandPage());
        }

        private void Series_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainContent.Children.Clear();
            MainContent.Children.Add(new Pages.SeriePage());
        }

        private void Product_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainContent.Children.Clear();
            MainContent.Children.Add(new Pages.ProductPage());
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainContent.Children.Clear();
            MainContent.Children.Add(new Pages.CategoryPage());
        }

        private bool isDeptExpanded = false;

        private void _ShowItems()
        {
            if (!isSidebarOpen)
                ToggleSidebar();

            if (!isDeptExpanded)
                ExpandDepartmentsSubmenu();
            else
                CollapseDepartmentsSubmenu();
        }

        private void Catalog_Click(object sender, RoutedEventArgs e)
        {
            _ShowItems();
        }

        private void Model_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainContent.Children.Clear();
            MainContent.Children.Add(new Pages.ModelPage());
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            // Comming soon - for demo, just change title
            MainContent.Children.Clear();
            MainContent.Children.Add(new Pages.SettingsPage());
        }

        private bool isProfileActive = false; // track if profile is selected

        private void ProfileBorder_Click(object sender, MouseButtonEventArgs e)
        {
            // Mark profile as active
            isProfileActive = true;

            // Reset other menu buttons
            SetActiveMenu(sender as Button); // optional if you want to clear button active states

            // Set profile colors like active button
            ProfileBorder.Background = MenuActiveBg;

            var path = ProfileIcon.ChildOfType<Path>().FirstOrDefault();
            if (path != null) path.Fill = MenuActiveFg;

            foreach (var tb in ProfileTextStack.Children.OfType<TextBlock>())
                tb.Foreground = MenuActiveFg;

            // Load content
            MainContent.Children.Clear();
            MainContent.Children.Add(new TextBlock
            {
                Text = "Profile Page - Coming Soon",
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        private void ProfileBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            var border = sender as Border;

            // Hover color
            border.Background = MenuHoverBg;

            var path = ProfileIcon.ChildOfType<Path>().FirstOrDefault();
            if (path != null) path.Fill = MenuActiveFg;

            foreach (var tb in ProfileTextStack.Children.OfType<TextBlock>())
                tb.Foreground = MenuActiveFg;
        }

        private void ProfileBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            // Only reset if not active
            if (!isProfileActive)
            {
                ProfileBorder.Background = MenuNormalBg;

                var path = ProfileIcon.ChildOfType<Path>().FirstOrDefault();
                if (path != null) path.Fill = MenuNormalFg;

                foreach (var tb in ProfileTextStack.Children.OfType<TextBlock>())
                    tb.Foreground = MenuNormalFg;
            }
        }

        private void Department_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            // Load content
            MainContent.Children.Clear();
            MainContent.Children.Add(new TextBlock
            {
                Text = "Department Page - Coming Soon",
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var viewer = (ScrollViewer)sender;

            // VERY SMALL STEP (try 10, 15, 20)
            double step = 15;

            if (e.Delta > 0)
                viewer.ScrollToVerticalOffset(viewer.VerticalOffset - step);
            else
                viewer.ScrollToVerticalOffset(viewer.VerticalOffset + step);

            e.Handled = true;
        }

    }
}
