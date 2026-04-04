using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace POS_WPF.UI
{
    public static class CardButtonsFactory
    {
        public static Button CreateEditButton(RoutedEventHandler clickEvent, int id)
        {
            Button btn = CreateBaseButton("#1ABC9C", "#48C9B0", clickEvent, id);

            btn.Content = new Path
            {
                Data = Geometry.Parse(
                    "M20.1498 7.93997L8.27978 19.81C7.21978 20.88 4.04977 21.3699 3.32977 20.6599C2.60977 19.9499 3.11978 16.78 4.17978 15.71L16.0498 3.84Z"),
                Fill = Brushes.White,
                Stretch = Stretch.Uniform,
                Width = 16,
                Height = 16
            };

            return btn;
        }

        public static Button CreateEditButton(RoutedEventHandler clickEvent, object tag)
        {
            Button btn = CreateBaseButton("#1ABC9C", "#48C9B0", clickEvent, tag);

            btn.Content = new Path
            {
                Data = Geometry.Parse(
                    "M20.1498 7.93997L8.27978 19.81C7.21978 20.88 4.04977 21.3699 3.32977 20.6599C2.60977 19.9499 3.11978 16.78 4.17978 15.71L16.0498 3.84Z"),
                Fill = Brushes.White,
                Stretch = Stretch.Uniform,
                Width = 16,
                Height = 16
            };

            return btn;
        }

        public static Button CreateDeleteButton(RoutedEventHandler clickEvent, int id)
        {
            Button btn = CreateBaseButton("#E74C3C", "#EC7063", clickEvent, id);

            Viewbox viewbox = new Viewbox { Width = 16, Height = 16 };
            Canvas canvas = new Canvas { Width = 24, Height = 24 };

            void AddPath(string data)
            {
                canvas.Children.Add(new Path
                {
                    Data = Geometry.Parse(data),
                    Stroke = Brushes.White,
                    StrokeThickness = 2,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                });
            }

            AddPath("M6 10V18C6 19.6569 7.34315 21 9 21H15C16.6569 21 18 19.6569 18 18V10");
            AddPath("M9 5C9 3.89543 9.89543 3 11 3H13C14.1046 3 15 3.89543 15 5V7H9V5Z");
            AddPath("M10 12V17");
            AddPath("M14 12V17");
            AddPath("M4 7H20");

            viewbox.Child = canvas;
            btn.Content = viewbox;

            return btn;
        }

        public static Button CreateDeleteButton(RoutedEventHandler clickEvent, object tag)
        {
            Button btn = CreateBaseButton("#E74C3C", "#EC7063", clickEvent, tag);

            Viewbox viewbox = new Viewbox { Width = 16, Height = 16 };
            Canvas canvas = new Canvas { Width = 24, Height = 24 };

            void AddPath(string data)
            {
                canvas.Children.Add(new Path
                {
                    Data = Geometry.Parse(data),
                    Stroke = Brushes.White,
                    StrokeThickness = 2,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                });
            }

            AddPath("M6 10V18C6 19.6569 7.34315 21 9 21H15C16.6569 21 18 19.6569 18 18V10");
            AddPath("M9 5C9 3.89543 9.89543 3 11 3H13C14.1046 3 15 3.89543 15 5V7H9V5Z");
            AddPath("M10 12V17");
            AddPath("M14 12V17");
            AddPath("M4 7H20");

            viewbox.Child = canvas;
            btn.Content = viewbox;

            return btn;
        }

        public static Button CreateTransferButton(RoutedEventHandler clickEvent, int id) 
        {
            Button btn = CreateBaseButton("#00A9FF", "#336DFF", clickEvent, id);

            Viewbox viewbox = new Viewbox
            {
                Width = 24,
                Height = 24
            };

            Canvas canvas = new Canvas { Width = 24, Height = 24 };

            // Main base
            canvas.Children.Add(new Path
            {
                Data = Geometry.Parse("M4 15.2433C4.41421 15.2433 4.75 15.5791 4.75 15.9933V19C4.75 19.1381 4.86193 19.25 5 19.25H19C19.1381 19.25 19.25 19.1381 19.25 19V15.9933C19.25 15.5791 19.5858 15.2433 20 15.2433C20.4142 15.2433 20.75 15.5791 20.75 15.9933V19C20.75 19.9665 19.9665 20.75 19 20.75H5C4.0335 20.75 3.25 19.9665 3.25 19V15.9933C3.25 15.5791 3.58579 15.2433 4 15.2433Z"),
                Fill = Brushes.White
            });

            // Arrow
            canvas.Children.Add(new Path
            {
                Data = Geometry.Parse("M19.8697 10.8932C20.1696 11.1789 20.1811 11.6536 19.8953 11.9535L16.8477 15.1517C16.7061 15.3003 16.5099 15.3843 16.3047 15.3843C16.0995 15.3843 15.9033 15.3003 15.7618 15.1517L12.7142 11.9535C12.4284 11.6536 12.4399 11.1789 12.7397 10.8932C13.0396 10.6074 13.5143 10.6189 13.8001 10.9187L16.3047 13.5471L18.8094 10.9187C19.0951 10.6189 19.5699 10.6074 19.8697 10.8932Z"),
                Fill = Brushes.White
            });

            // Top shape
            canvas.Children.Add(new Path
            {
                Data = Geometry.Parse("M11.3524 4.75C9.03146 4.75 7.14999 6.63147 7.14999 8.95238V13.9582C7.14999 14.3724 6.81421 14.7082 6.39999 14.7082C5.98578 14.7082 5.64999 14.3724 5.64999 13.9582V8.95238C5.64999 5.80305 8.20304 3.25 11.3524 3.25C14.5017 3.25 17.0548 5.80304 17.0548 8.95238V13.9582C17.0548 14.3724 16.719 14.7082 16.3048 14.7082C15.8905 14.7082 15.5548 14.3724 15.5548 13.9582V8.95238C15.5548 6.63147 13.6733 4.75 11.3524 4.75Z"),
                Fill = Brushes.White
            });

            viewbox.Child = canvas;
            btn.Content = viewbox;

            return btn;
        }

        public static Button CreateTransferButton(RoutedEventHandler clickEvent, object tag)
        {
            Button btn = CreateBaseButton("#00A9FF", "#336DFF", clickEvent, tag);

            Viewbox viewbox = new Viewbox
            {
                Width = 24,
                Height = 24
            };

            Canvas canvas = new Canvas { Width = 24, Height = 24 };

            // Main base
            canvas.Children.Add(new Path
            {
                Data = Geometry.Parse("M4 15.2433C4.41421 15.2433 4.75 15.5791 4.75 15.9933V19C4.75 19.1381 4.86193 19.25 5 19.25H19C19.1381 19.25 19.25 19.1381 19.25 19V15.9933C19.25 15.5791 19.5858 15.2433 20 15.2433C20.4142 15.2433 20.75 15.5791 20.75 15.9933V19C20.75 19.9665 19.9665 20.75 19 20.75H5C4.0335 20.75 3.25 19.9665 3.25 19V15.9933C3.25 15.5791 3.58579 15.2433 4 15.2433Z"),
                Fill = Brushes.White
            });

            // Arrow
            canvas.Children.Add(new Path
            {
                Data = Geometry.Parse("M19.8697 10.8932C20.1696 11.1789 20.1811 11.6536 19.8953 11.9535L16.8477 15.1517C16.7061 15.3003 16.5099 15.3843 16.3047 15.3843C16.0995 15.3843 15.9033 15.3003 15.7618 15.1517L12.7142 11.9535C12.4284 11.6536 12.4399 11.1789 12.7397 10.8932C13.0396 10.6074 13.5143 10.6189 13.8001 10.9187L16.3047 13.5471L18.8094 10.9187C19.0951 10.6189 19.5699 10.6074 19.8697 10.8932Z"),
                Fill = Brushes.White
            });

            // Top shape
            canvas.Children.Add(new Path
            {
                Data = Geometry.Parse("M11.3524 4.75C9.03146 4.75 7.14999 6.63147 7.14999 8.95238V13.9582C7.14999 14.3724 6.81421 14.7082 6.39999 14.7082C5.98578 14.7082 5.64999 14.3724 5.64999 13.9582V8.95238C5.64999 5.80305 8.20304 3.25 11.3524 3.25C14.5017 3.25 17.0548 5.80304 17.0548 8.95238V13.9582C17.0548 14.3724 16.719 14.7082 16.3048 14.7082C15.8905 14.7082 15.5548 14.3724 15.5548 13.9582V8.95238C15.5548 6.63147 13.6733 4.75 11.3524 4.75Z"),
                Fill = Brushes.White
            });

            viewbox.Child = canvas;
            btn.Content = viewbox;

            return btn;
        }


        private static Button CreateBaseButton(string normalColor, string hoverColor, RoutedEventHandler clickEvent, int id)
        {
            Button btn = new Button
            {
                Width = 32,
                Height = 32,
                Tag = id,
                Cursor = Cursors.Hand,
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString(normalColor),
                BorderThickness = new Thickness(0)
            };

            btn.Click += clickEvent;

            btn.MouseEnter += (s, e) =>
                btn.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(hoverColor);

            btn.MouseLeave += (s, e) =>
                btn.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(normalColor);

            ControlTemplate template = new ControlTemplate(typeof(Button));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));

            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(presenter);
            template.VisualTree = border;
            btn.Template = template;

            return btn;
        }


        private static Button CreateBaseButton(string normalColor, string hoverColor, RoutedEventHandler clickEvent, object tag)
        {
            Button btn = new Button
            {
                Width = 32,
                Height = 32,
                Tag = tag,
                Cursor = Cursors.Hand,
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString(normalColor),
                BorderThickness = new Thickness(0)
            };

            btn.Click += clickEvent;

            btn.MouseEnter += (s, e) =>
                btn.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(hoverColor);

            btn.MouseLeave += (s, e) =>
                btn.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(normalColor);

            ControlTemplate template = new ControlTemplate(typeof(Button));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));

            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(presenter);
            template.VisualTree = border;
            btn.Template = template;

            return btn;
        }

    }
}
