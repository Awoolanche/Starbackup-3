using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Starbackup_3.Services;

public class LogLevelToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LogLevel level)
        {
            return level switch
            {
                LogLevel.Error => Brushes.Red,
                LogLevel.Success => Brushes.Green,
                LogLevel.Warning => Brushes.Orange,
                _ => Brushes.White
            };
        }
        return Brushes.White;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}