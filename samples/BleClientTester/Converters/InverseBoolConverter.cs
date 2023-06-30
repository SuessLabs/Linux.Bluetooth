using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BleClientTester.Converters;

public class InverseBoolConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    try
    {
      return !(bool)value;
    }
    catch
    {
      // Remember, we're inverse
      return true;
    }
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return Convert(value, targetType, parameter, culture);
  }
}
