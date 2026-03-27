using System.Windows;
using System.Windows.Controls;

public static class TextBoxHelper
{
    public static readonly DependencyProperty HasBeenTouchedProperty =
        DependencyProperty.RegisterAttached(
            "HasBeenTouched",
            typeof(bool),
            typeof(TextBoxHelper),
            new FrameworkPropertyMetadata(false));

    public static bool GetHasBeenTouched(DependencyObject obj) => (bool)obj.GetValue(HasBeenTouchedProperty);
    public static void SetHasBeenTouched(DependencyObject obj, bool value) => obj.SetValue(HasBeenTouchedProperty, value);
}
