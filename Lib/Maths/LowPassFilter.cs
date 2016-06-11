using System;

namespace Lada.Maths
{
    /// <summary>
    /// Filters high frequencies of an input signal
    /// </summary>
    public class LowPassFilter
    {
        public LowPassFilter()
        {
        }
        public LowPassFilter(double lifetime)
        {
            this.Lifetime = lifetime;
        }
        /// <summary>
        /// Gets or sets the lifetime of the filter
        /// </summary>
        public double Lifetime
        {
            get;
            set;
        }
        /// <summary>
        /// Gets the current input signal value or increment
        /// </summary>
        public double Input
        {
            get; private set;
        }
        /// <summary>
        /// Gets the current output signal value or increment
        /// </summary>
        public double Output
        {
            get; private set;
        }

        /// <summary>
        /// Inputs a new signal increment.
        /// </summary>
        public void NewInputDelta(double delta)
        {
            Input += delta;
        }
        /// <summary>
        /// Computes next output signal increment
        /// </summary>
        public double NextOutputDelta(double t, double dtMin)
        {
            var dt = double.IsNaN (_t0) ? dtMin : t - _t0;
            _t0 = t;
            double gain = Math.Min (1.0, dt / Lifetime);
            Output += gain * (Input - Output);
            Input = 0;
            return Output;
        }

        private double _t0 = double.NaN;
    }
}
