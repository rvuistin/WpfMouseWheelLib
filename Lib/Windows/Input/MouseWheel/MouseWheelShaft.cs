using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lada.Windows.MotionFlow;

namespace Lada.Windows.Input
{
    public interface IMouseWheelShaft : INativeMotionTransfer
    {
        int Resolution
        {
            get;
        }
    }

    public class MouseWheelShaft : NativeMotionTransfer, IMouseWheelShaft
    {
        // TODO: still to implement some resource management with parent transfer case.
        public      MouseWheelShaft(int resolution)
        {
            Resolution = resolution;
        }
        public int  Resolution
        {
            get; private set;
        }
    }
}
