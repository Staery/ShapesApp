using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ShapesApp.Models;

namespace ShapesApp.Converters
{
    public class RectangleToPointsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Rectangle rectangle)
            {
                PointCollection points = new PointCollection
                {
                    new System.Windows.Point(rectangle.BottomLeft.X, rectangle.BottomLeft.Y),
                    new System.Windows.Point(rectangle.BottomRight.X, rectangle.BottomRight.Y),
                    new System.Windows.Point(rectangle.TopRight.X, rectangle.TopRight.Y),
                    new System.Windows.Point(rectangle.TopLeft.X, rectangle.TopLeft.Y)
                };
                return points;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
