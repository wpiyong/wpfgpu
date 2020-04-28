using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ImageProcessing.Models
{
    public static class DrawCanvas
    {
        public static Line NewLine(double x1, double y1, double x2, double y2, bool refPoint, Canvas cv)
        {

            Line line = new Line()
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = refPoint ? Brushes.Green : Brushes.Red,
                StrokeThickness = refPoint ? 50 : 50
            };

            line.SnapsToDevicePixels = true;
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            cv.Children.Add(line);

            return line;
        }

        public static Ellipse Circle(double x, double y, int width, int height, bool refPoint, Canvas cv, bool fill = false, double opacity = 1.0)
        {

            Ellipse circle = new Ellipse()
            {
                Width = width,
                Height = height,
                Stroke = refPoint ? Brushes.Green : Brushes.Red,
                Fill = fill ? Brushes.Green : null,
                Opacity = opacity,
                StrokeThickness = refPoint ? 50 : 50
            };

            cv.Children.Add(circle);

            circle.SetValue(Canvas.LeftProperty, x - width / 2.0);
            circle.SetValue(Canvas.TopProperty, y - height / 2.0);

            return circle;
        }

        public static TextBlock Text(double x, double y, string text, bool refPoint, SolidColorBrush color, Canvas cv, bool shift = true)
        {

            TextBlock textBlock = new TextBlock();

            textBlock.Text = text;

            textBlock.Foreground = color == null ? refPoint ? Brushes.Green : Brushes.Red : color;
            textBlock.FontSize = refPoint ? 36 : 24;
            textBlock.FontWeight = refPoint ? FontWeights.Bold : FontWeights.Normal;

            Canvas.SetLeft(textBlock, x - (shift ? (refPoint ? 10 : 7.5) : 0));
            Canvas.SetTop(textBlock, y - (shift ? (refPoint ? 58 : 40) : 10));

            cv.Children.Add(textBlock);

            return textBlock;
        }

        public static Rectangle Rect(double x, double y, int width, int height, SolidColorBrush color, Canvas cv)
        {
            Rectangle rect = new Rectangle()
            {
                Width = width,
                Height = height,
                Fill = color,
                Stroke = color
            };

            cv.Children.Add(rect);
            Canvas.SetTop(rect, y);
            Canvas.SetLeft(rect, x);

            return rect;
        }
    }
}
