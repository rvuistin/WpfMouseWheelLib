using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Lada.Windows
{
    public static class InputElementExtensions
    {
        public static void RaiseEvent(this IInputElement source, IEnumerable<RoutedEventArgs> args)
        {
            foreach (var e in args)
            {
                source.RaiseEvent (e);
            }
        }
    }
}
