using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Git_profiles.ViewModels
{
    public class StringToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string hexColor)
            {
                return null;
            }

            try
            {
                if (hexColor.StartsWith("#"))
                {
                    string hex = hexColor.TrimStart('#');
                    if (hex.Length == 6)
                    {
                        byte r = System.Convert.ToByte(hex.Substring(0, 2), 16);
                        byte g = System.Convert.ToByte(hex.Substring(2, 2), 16);
                        byte b = System.Convert.ToByte(hex.Substring(4, 2), 16);
                        return Color.FromRgb(r, g, b);
                    }
                    else if (hex.Length == 8)
                    {
                        byte a = System.Convert.ToByte(hex.Substring(0, 2), 16);
                        byte r = System.Convert.ToByte(hex.Substring(2, 2), 16);
                        byte g = System.Convert.ToByte(hex.Substring(4, 2), 16);
                        byte b = System.Convert.ToByte(hex.Substring(6, 2), 16);
                        return Color.FromArgb(a, r, g, b);
                    }
                }
            }
            catch (Exception)
            {
                // En caso de error, devolver un color por defecto
            }

            // Color por defecto si hay un error o el valor no es válido
            return Colors.Red;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}