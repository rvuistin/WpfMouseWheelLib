using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Lada.Windows.Input
{
  public static class MouseDeviceExtensions
  {
    public static MouseWheel GetWheel(this MouseDevice source)
    {
      return MouseWheel.Wheels.GetOrAdd(source, mouseDevice => new MouseWheel(mouseDevice, MouseWheel.Wheels.Count));
    }
  }
}
