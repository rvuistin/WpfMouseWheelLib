using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Lada.Windows.Input
{
    public class MouseWheelFrameworkLevelController : MouseWheelController
    {
        public                          MouseWheelFrameworkLevelController(IFrameworkLevelElement frameworkLevelElement)
          : base (frameworkLevelElement)
        {
            frameworkLevelElement.Unloaded += OnElementUnloaded;
        }
        public IFrameworkLevelElement   FrameworkLevelElement
        {
            get
            {
                return InputLevelElement as IFrameworkLevelElement;
            }
        }
        public override void            Dispose()
        {
            FrameworkLevelElement.Unloaded -= OnElementUnloaded;
            base.Dispose ();
        }

        private void                    OnElementUnloaded(object sender, RoutedEventArgs e)
        {
            Unload ();
        }
    }
}
