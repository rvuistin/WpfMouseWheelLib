using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Data;

namespace Lada.Windows.Data
{
  public class BooleanToWaitCursorConverter : IValueConverter
  {
    #region IValueConverter
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value != null && ((bool)value))
        return Cursors.Wait;
      else
        return null;
    }
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    } 
    #endregion
  }
}
