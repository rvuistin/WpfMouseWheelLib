using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Lada.Maths;
using Lada.Windows.MotionFlow;

namespace Lada.Windows.Input
{
    public class MouseWheelLinearFilter : IMotionFilter
    {

        public void                                 NewInputDelta(TimeSpan t, double delta, IMotionInfo info)
        {
            var resolution = GetResolution (info);
            Debug.Assert (resolution >= 1.0);
            var minSpeed = MinSpeed / resolution;
            var speed = info.Speed;
            NewInputDelta (t.TotalSeconds, delta, resolution, minSpeed, speed);
        }
        public double                               NextOutputDelta(TimeSpan t)
        {
            return NextOutputDelta (t.TotalSeconds);
        }

        protected virtual void                      NewInputDelta(double t, double delta, double resolution, double minSpeed, double speed)
        {
            _linearFilter.NewInputDelta (t, delta, Math.Max (minSpeed, speed));
        }
        protected virtual double                    NextOutputDelta(double t1)
        {
            return _linearFilter.NextOutputDelta (t1);
        }

        protected const double                      StdRes      = 1;
        protected const double                      HiRes       = 8;
        protected const double                      MinSpeed    = 0.8;  // [click / s]

        private static double                       GetResolution(IMotionInfo info)
        {
            var wheel = info as MouseWheel;
            var resolution = wheel.ActiveTransferCase.ActiveShaft.Resolution;
            if (resolution == 0)
                return wheel.Resolution;
            return resolution;
        }
        private readonly DifferentialLinearFilter   _linearFilter = new DifferentialLinearFilter();
    }

    public class MouseWheelSmoothingFilter : MouseWheelLinearFilter
    {
        protected override void                     NewInputDelta(double t, double delta, double resolution, double minSpeed, double speed)
        {
            base.NewInputDelta (t, delta, resolution, minSpeed, speed);
            AdaptLowPassFilterLifetime (resolution, minSpeed, speed);
        }
        protected override double                   NextOutputDelta(double t1)
        {
            var dy = base.NextOutputDelta (t1);
            _lowPassFilter.NewInputDelta (dy);
            return _lowPassFilter.NextOutputDelta (t1, MinLifetime);
        }

        private static readonly AffineFunction      MaxLifetimeVersusResolution = new AffineFunction(StdRes, StdResMaxLifetime, HiRes, HiResMaxLifetime);
        private void                                AdaptLowPassFilterLifetime(double resolution, double minSpeed, double speed)
        {
            var maxLifetime = MaxLifetimeVersusResolution.Y (resolution);
            var lifetimeVersusSpeed = new AffineFunction (minSpeed, maxLifetime, MaxSpeed, MinLifetime);
            var lifetime = Math.Max (MinLifetime, Math.Min (maxLifetime, lifetimeVersusSpeed.Y (speed)));
            _lowPassFilter.Lifetime = lifetime;
            //Debug.WriteLine(string.Format("AdaptLifetime(resolution={0,3:F1}, minSpeed={1,3:F1}, speed={2,5:F1}) => maxLifetime={3,5:F3}, lifetime={4,5:F3}", resolution, minSpeed, speed, maxLifetime, lifetime));
        }

        private const double                        StdResMaxLifetime  = 0.200;  // [s]
        private const double                        HiResMaxLifetime   = 0.080;  // [s]
        private const double                        MaxSpeed           = 80;     // [click / s]
        private const double                        MinLifetime        = 0.017;  // [s] WPF rendering period

        private readonly LowPassFilter              _lowPassFilter = new LowPassFilter();
    }
}
