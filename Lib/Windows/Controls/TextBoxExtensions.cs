using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Lada.Windows.Controls
{
  public static class TextBoxExtensions
  {
    public static int GetScrollableLines(this TextBox source, int direction)
    {
      if (direction < 0)
        return -source.GetFirstVisibleLineIndex();
      else
        return source.LineCount - 1 - source.GetLastVisibleLineIndex();
    }
  }
}
