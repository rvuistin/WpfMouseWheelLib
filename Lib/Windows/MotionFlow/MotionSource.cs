using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace Lada.Windows.MotionFlow
{
    public interface INativeMotionSourceInput : INativeMotionInput
    {
        void    Transmit(int timeStamp, int nativeDelta);
    }

    public interface INativeMotionConverter
    {
        int     NativeResolutionFrequency
        {
            get;
        }
        double  NativeToNormalized(int value);
        int     NormalizedToNative(double value);
    }

    public interface INativeMotionSource : IMotionInfo, INativeMotionSourceInput, INativeMotionOutput, INativeMotionConverter
    {
    }

    public abstract class NativeMotionSource : MotionElementLink, INativeMotionSource
    {
        public                      NativeMotionSource() => this.Next = NativeMotionTerminal.Current;

        public IMotionInfo          Source           => this;
        public TimeSpan             Time             => TimeSpan.FromMilliseconds(timeStamp ?? 0);
        public double               Speed            => Math.Abs(this.Velocity);
        public int                  Direction        => -this.nativeDirection;
        public bool                 DirectionChanged => this.previousNativeDirection != this.nativeDirection;
        public TimeSpan             Delay    { get; private set; }
        public double               Velocity { get; private set; }
        public int                  NativeDirection
        {
            get
            {
                return this.nativeDirection;
            }
            private set
            {
                if (this.nativeDirection != value)
                {
                    this.previousNativeDirection = this.nativeDirection;
                    this.nativeDirection = value;
                }
            }
        }
        public void                 Transmit(int timeStamp, int nativeDelta)
        {
            var info = this.PreTransmit (timeStamp, nativeDelta);
            this.Transmit (info, nativeDelta, null);
        }
        public void                 Transmit(IMotionInfo info, int nativeDelta, INativeMotionOutput source)
        {
            this.Next.Transmit (info, nativeDelta, this);
        }
        public void                 OnCoupledTransfer(IMotionInfo info, int nativeDelta, INativeMotionTransferOutput source)
        {
            this.Next.OnCoupledTransfer (info, nativeDelta, source);
        }
        public void                 Reset()
        {
            this.Next.Reset ();
        }
        public INativeMotionInput   Next
        {
            [DebuggerStepThrough]
            get => this.GetNext() as INativeMotionInput;
            [DebuggerStepThrough]
            set => this.SetNext(value);
        }

        public abstract int         NativeResolutionFrequency { get; }
        public abstract double      NativeToNormalized(int value);
        public abstract int         NormalizedToNative(double value);


        public virtual IMotionInfo  PreTransmit(int timeStamp, int nativeDelta)
        {
            if (nativeDelta != 0)
            {
                this.NativeDirection = Math.Sign (nativeDelta);

                this.UpdateTimings (timeStamp);

                var delta = this.NativeToNormalized (nativeDelta);
                this.UpdateVelocity (delta);
            }
            return this;
        }

        private void                UpdateTimings(int timeStamp)
        {
            if (this.timeStamp.HasValue)
            {
                // fix timestamp wrapping issue (http://msdn.microsoft.com/en-us/library/ms644939(VS.85).aspx)
                var delta = unchecked(timeStamp - this.timeStamp.Value);
                this.Delay = TimeSpan.FromMilliseconds (delta);
            }
            else
            {
                this.timeStamp = timeStamp;
            }
        }
        private void                UpdateVelocity(double delta)
        {
            if (this.Delay != TimeSpan.Zero)
            {
                this.Velocity = delta / this.Delay.TotalSeconds;
            }
        }

        private int?                timeStamp;
        private int                 previousNativeDirection;
        private int                 nativeDirection;
    }
}
