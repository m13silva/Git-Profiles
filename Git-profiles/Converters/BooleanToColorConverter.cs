using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Git_profiles.Converters
{
    public class BooleanToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool boolValue || parameter is not string colors)
                return null;

            var colorStrings = colors.Split('|');
            if (colorStrings.Length != 2)
                return null;

            var colorString = boolValue ? colorStrings[1] : colorStrings[0];
            var color = Color.Parse(colorString);
            return new SolidColorBrush(color);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}