using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;


public static class UIHelper
{
    public static IEnumerable<T> ChildOfType<T>(this DependencyObject depObj) where T : DependencyObject
    {
        if (depObj == null) yield break;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);
            if (child is T t) yield return t;
            foreach (var childOfChild in ChildOfType<T>(child)) yield return childOfChild;
        }
    }

    public static IEnumerable<T> ContentOfType<T>(this object content) where T : DependencyObject
    {
        if (content is T t) yield return t;
        if (content is ContentControl cc && cc.Content != null)
        {
            foreach (var c in ContentOfType<T>(cc.Content)) yield return c;
        }
        if (content is Panel panel)
        {
            foreach (var c in panel.Children.OfType<DependencyObject>())
                foreach (var child in ContentOfType<T>(c)) yield return child;
        }
    }
}
