using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace FancyTab.Avalonia.Converters;

/// <summary>
/// 枚举到布尔值转换器
/// </summary>
public class EnumToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        return value.Equals(parameter);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true && parameter != null)
            return parameter;

        return AvaloniaProperty.UnsetValue;
    }
}
