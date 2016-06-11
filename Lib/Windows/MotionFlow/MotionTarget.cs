using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Lada.Diagnostics;

namespace Lada.Windows.MotionFlow
{
    public interface INativeMotionTarget
    {
        bool    CanMove(IMotionInfo info, object context);
        int     Coerce(IMotionInfo info, object context, int nativeDelta);
        void    Move(IMotionInfo info, object context, int nativeDelta);
    }

    public interface IMotionTarget
    {
        bool    CanMove(IMotionInfo info, object context);
        double  Coerce(IMotionInfo info, object context, double delta);
        void    Move(IMotionInfo info, object context, double delta);
    }

    public class NativeMotionTarget : INativeMotionTarget
    {
        public static readonly NativeMotionTarget Terminal = new NativeMotionTarget();

        public virtual bool     CanMove(IMotionInfo info, object context)
        {
            return true;
        }
        public virtual int      Coerce(IMotionInfo info, object context, int nativeDelta)
        {
            return nativeDelta;
        }
        public virtual void     Move(IMotionInfo info, object context, int nativeDelta)
        {
        }
    }

    public class MotionTarget : IMotionTarget
    {
        public virtual bool     CanMove(IMotionInfo info, object context)
        {
            return !DoubleEx.IsZero (Coerce (info, context, info.Direction));
        }
        public virtual double   Coerce(IMotionInfo info, object context, double delta)
        {
            return delta;
        }
        public virtual void     Move(IMotionInfo info, object context, double delta)
        {
        }
    }

    public class MotionTargetLink : MotionTarget
    {
        public IMotionTarget    Next
        {
            get; set;
        }
        public override bool    CanMove(IMotionInfo info, object context)
        {
            return Next.CanMove (info, context);
        }
        public override double  Coerce(IMotionInfo info, object context, double delta)
        {
            return Next.Coerce (info, context, delta);
        }
        public override void    Move(IMotionInfo info, object context, double delta)
        {
            Next.Move (info, context, delta);
        }
    }

    public class MotionSmoothingTarget : MotionTargetLink, IDisposable
    {
        public                  MotionSmoothingTarget(IMotionFilter filter)
        {
            _filter = filter;
        }
        public double           Precision
        {
            get;
            set;
        }
        public override void    Move(IMotionInfo info, object context, double delta)
        {
            var t = TimeBase.Current.Elapsed;
            _filter.NewInputDelta (t, delta, info);
            CompositionTarget.Rendering -= OnRendering;
            CompositionTarget.Rendering += OnRendering;
        }
        public void             Dispose()
        {
            CompositionTarget.Rendering -= OnRendering;
        }

        private void            OnRendering(object sender, EventArgs e)
        {
            var t = TimeBase.Current.Elapsed;
            double delta = _remainder + _filter.NextOutputDelta (t);
            if (DoubleEx.GreaterThanOrClose (Math.Abs (delta), Precision))
            {
                _remainder = 0;
                Next.Move (null, null, delta);
            }
            else if (DoubleEx.IsZero (delta))
                CompositionTarget.Rendering -= OnRendering;
            else
                _remainder += delta;
        }

        private readonly IMotionFilter  _filter;
        private double                  _remainder;
    }
}
