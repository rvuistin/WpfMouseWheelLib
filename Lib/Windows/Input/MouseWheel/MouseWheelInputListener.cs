using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Lada.Windows.Input
{
    public interface IMouseWheelInputListener
    {
        void OnPreviewInput(object sender, MouseWheelInputEventArgs e);
        void OnInput(object sender, MouseWheelInputEventArgs e);
    }
}
