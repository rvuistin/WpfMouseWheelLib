using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lada.Windows.MotionFlow
{
  public interface IMotionFilter
  {
    void    NewInputDelta(TimeSpan t, double delta, IMotionInfo info);
    double  NextOutputDelta(TimeSpan t);
  }
}
