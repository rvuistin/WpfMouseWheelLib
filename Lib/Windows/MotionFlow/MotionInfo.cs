using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lada.Windows.MotionFlow
{
    public interface IMotionInfo
    {
        IMotionInfo Source
        {
            get;
        }
        TimeSpan    Time
        {
            get;
        }
        TimeSpan    Delay
        {
            get;
        }
        double      Velocity
        {
            get;
        }
        double      Speed
        {
            get;
        }
        int         NativeDirection
        {
            get;
        }
        int         Direction
        {
            get;
        }
        bool        DirectionChanged
        {
            get;
        }
    }
}
