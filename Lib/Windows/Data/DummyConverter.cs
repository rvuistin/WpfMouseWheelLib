using System;
using System.Windows.Data;

namespace Lada.Windows.Data
{
  public class DummyConverter : IValueConverter
  {
    #region IValueConverter
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return value;
    }
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return value;
    }
    #endregion
  }
}
