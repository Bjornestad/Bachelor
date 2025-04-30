using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Platform;
using System;
using System.Globalization;

namespace Bachelor.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrEmpty(path))
            {
                try
                {
                    var uri = new Uri(path);
                    return new Avalonia.Media.Imaging.Bitmap(AssetLoader.Open(uri));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load image {path}: {ex.Message}");
                    return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}