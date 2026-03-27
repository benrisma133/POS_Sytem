using POS_BLL;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace POS_WPF.Controls
{
    public partial class ProductCard : UserControl
    {
        private readonly Brush mainColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF8C42"));
        private readonly Brush hoverColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA15C"));

        public ProductCard()
        {
            InitializeComponent();
            AttachLinkEvents(txtUpdate);
            AttachLinkEvents(txtTransfer);
        }

        public void LoadProduct(int productID, int warehouseID)
        {
            // Use your existing BLL method to get the product for this warehouse
            var product = clsProduct.FindByIDInWarehouse(productID, warehouseID);
            if (product == null)
            {
                // message box to show product not found
                MessageBox.Show("Product not found in the specified warehouse.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Populate UI
            txtProductName.Text = product.ProductName;
            txtCategory.Text = "   " + product.Category?.Name ?? "-";
            txtModel.Text = "   " + product.Model?.Name ?? "-";
            txtWarehouse.Text = product.Warehouse?.Name ?? "-";
            txtStock.Text = "  " + product.Quantity.ToString();
            txtPrice.Text = "  " + product.Price.ToString("C"); // Currency format
            txtDescription.Text = "     " + product.Description ?? "-";
        }

        private void AttachLinkEvents(TextBlock tb)
        {
            tb.MouseEnter += (s, e) =>
            {
                tb.Foreground = hoverColor;
                AnimateScale(tb, 1.05, 1.05, 100);
            };

            tb.MouseLeave += (s, e) =>
            {
                tb.Foreground = mainColor;
                AnimateScale(tb, 1.0, 1.0, 100);
            };

            tb.MouseLeftButtonDown += (s, e) =>
            {
                AnimateScale(tb, 0.95, 0.95, 50);
            };

            tb.MouseLeftButtonUp += (s, e) =>
            {
                AnimateScale(tb, 1.05, 1.05, 50);
                // Here you can call your actual Update/Transfer logic
                string action = tb.Tag.ToString();
                if (action == "Update")
                    OnUpdateClicked();
                else if (action == "Transfer")
                    OnTransferClicked();
            };
        }

        private void AnimateScale(TextBlock tb, double scaleX, double scaleY, double durationMs)
        {
            if (tb.RenderTransform is ScaleTransform st)
            {
                st.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(scaleX, TimeSpan.FromMilliseconds(durationMs)));
                st.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(scaleY, TimeSpan.FromMilliseconds(durationMs)));
            }
        }

        // Replace with your actual logic
        private void OnUpdateClicked()
        {
            MessageBox.Show("Update clicked!");
        }

        private void OnTransferClicked()
        {
            MessageBox.Show("Transfer clicked!");
        }
    }
}
