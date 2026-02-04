using System.Globalization;
using System.Windows.Data;

namespace FancyTab.Converters;

/// <summary>
/// 枚举值到布尔值的转换器
/// </summary>
public class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return false;
        return value.Equals(parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter != null)
        {
            return parameter;
        }
        return Binding.DoNothing;
    }
}
