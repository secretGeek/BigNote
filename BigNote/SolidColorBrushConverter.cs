namespace BigNote
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    public class SolidColorBrushConverter : IValueConverter
    {
        private BrushConverter bc = new BrushConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = value as SolidColorBrush;
            if (brush != null)
            {
                return brush.Color.ToString();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var colorValue = value as String;
            if (colorValue != null)
            {
                try
                {
                    return bc.ConvertFromString(colorValue);
                }
                catch (FormatException)
                {
                    // swallow this and just return a black brush
                }
            }
            return Brushes.Black;
        }
    }
}
