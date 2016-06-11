using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace Lada.Windows
{
  public static class DependencyPropertyExtensions
  {
    public static DependencyPropertyDescriptor  GetDescriptor(this DependencyProperty property, Type targetType)
    {
      return DependencyPropertyDescriptor.FromProperty(property, targetType);
    }
    public static void                          AddValueChanged(this DependencyProperty property, object component, EventHandler handler)
    {
      property.GetDescriptor(component.GetType()).AddValueChanged(component, handler);
    }
    public static void                          RemoveValueChanged(this DependencyProperty property, object component, EventHandler handler)
    {
      property.GetDescriptor(component.GetType()).RemoveValueChanged(component, handler);
    }
  }
}
