using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lada.Windows.MotionFlow
{
    public interface INativeMotionOutput
    {
        INativeMotionInput Next
        {
            get; set;
        }
    }

    public interface IMotionOutput
    {
        IMotionInput Next
        {
            get; set;
        }
    }
}
