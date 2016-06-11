using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lada.Windows.MotionFlow
{
    public interface IMotionSinkConverter
    {
        double SinkToNormalized(double value);
        double NormalizedToSink(double value);
    }

    public interface IMotionSink : IMotionTarget, IMotionSinkConverter
    {
    }
}
