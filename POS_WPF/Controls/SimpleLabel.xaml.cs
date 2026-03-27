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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POS_WPF.Controls
{
    /// <summary>
    /// Interaction logic for SimpleLabel.xaml
    /// </summary>
    public partial class SimpleLabel : UserControl
    {
        public SimpleLabel()
        {
            InitializeComponent();
        }

        #region Dependency Property

        #region Text Property

        // 1️⃣ Register DependencyProperty
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",                 // اسم الخاصية
                typeof(string),          // النوع
                typeof(SimpleLabel),     // المالك
                new PropertyMetadata(
                    string.Empty,        // القيمة الافتراضية
                    OnTextChanged         // callback
                )
            );

        // 2️⃣ CLR Wrapper (باش نسهلو الاستعمال)
        public string Text
        {
            get => GetValue(TextProperty) as string ?? string.Empty;
            set => SetValue(TextProperty, value ?? string.Empty);
        }

        // 3️⃣ Callback
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SimpleLabel)d;
            string newText = e.NewValue as string ?? string.Empty;

            control.MyLabel.Text = newText;
        }


        #endregion

        #region TextColor Property

        public static readonly DependencyProperty TextColorProperty =
            DependencyProperty.Register(
                "TextColor",
                typeof(Brush),
                typeof(SimpleLabel), new PropertyMetadata(
                    Brushes.Black,
                    OnTextColorChanged
                    )
                );

        public Brush TextColor
        {
            get => (Brush)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        private static void OnTextColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SimpleLabel)d;
            control.MyLabel.Foreground = (Brush)e.NewValue;
        }

        #endregion

        #region FontSize Property

        public static readonly DependencyProperty LabelFontSizeProperty =
            DependencyProperty.Register(
                "LabelFontSize",
                typeof(double),
                typeof(SimpleLabel),
                new PropertyMetadata(14.0, OnFontSizeChanged));

        public double LabelFontSize
        {
            get => (double)GetValue(LabelFontSizeProperty);
            set => SetValue(LabelFontSizeProperty, value);
        }

        private static void OnFontSizeChanged(DependencyObject d ,DependencyPropertyChangedEventArgs e)
        {
            var control = (SimpleLabel)d;
            control.MyLabel.FontSize = (double)e.NewValue;
        }

        #endregion

        #region FontWeight Property

        public static readonly DependencyProperty LabelFontWeightProperty =
            DependencyProperty.Register(
                "LabelFontWeight",
                typeof(FontWeight),
                typeof(SimpleLabel),
                new PropertyMetadata(FontWeights.Normal, OnFontWeightChanged));

        public FontWeight LabelFontWeight
        {
            get => (FontWeight)GetValue(LabelFontWeightProperty);
            set => SetValue(LabelFontWeightProperty, value);
        }

        private static void OnFontWeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SimpleLabel)d;
            control.MyLabel.FontWeight = (FontWeight)e.NewValue;
        }

        #endregion

        #region IsBold Property

        public static readonly DependencyProperty IsBoldProperty =
            DependencyProperty.Register(
                "IsBold",
                typeof(bool),
                typeof(SimpleLabel),
                new PropertyMetadata(false, OnIsBoldChanged)
            );

        public bool IsBold
        {
            get => (bool)GetValue(IsBoldProperty);
            set => SetValue(IsBoldProperty, value);
        }

        private static void OnIsBoldChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SimpleLabel)d;
            control.MyLabel.FontWeight =
                (bool)e.NewValue ? FontWeights.Bold : FontWeights.Normal;
        }


        #endregion

        #endregion

    }
}
