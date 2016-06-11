using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Lada.WpfMouseWheel.ViewModels
{
  public class ScrollIncrementItemTemplateSelector : DataTemplateSelector
  {
    public DataTemplate NotAvailable { get; set; }
    public DataTemplate      Logical { get; set; }
    public DataTemplate     Physical { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      var smi = item as ScrollIncrementItem;
      if (smi == null)
        return null;
      if (smi.Value == null)
        return NotAvailable;
      else if (smi.IsLogical)
        return Logical;
      else
        return Physical;
    }
  }
}
