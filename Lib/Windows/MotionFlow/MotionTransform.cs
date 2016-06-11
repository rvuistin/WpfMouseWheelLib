using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using Lada.Maths;

namespace Lada.Windows.MotionFlow
{
    public interface IMotionTransform : IMotionInput, IMotionOutput
    {
    }

    public interface INativeMotionTransform : INativeMotionInput, INativeMotionOutput
    {
    }

    public class NativeMotionTransform : MotionElementLink, INativeMotionTransform
    {
        public                              NativeMotionTransform()
        {
            Next = NativeMotionTerminal.Current;
        }
        public virtual void                 Transmit(IMotionInfo info, int nativeDelta, INativeMotionOutput source)
        {
            var nativeDeltaY = Transform (info, nativeDelta);
            //Debug.WriteLine("{0}{1}(delta = {2,4}) --> {3,4}", Name, TransmitMethodSuffix(info, nativeDelta), nativeDelta, nativeDeltaY);
            Next.Transmit (info, nativeDeltaY, this);
        }
        public virtual void                 OnCoupledTransfer(IMotionInfo info, int nativeDelta, INativeMotionTransferOutput source)
        {
            Next.OnCoupledTransfer (info, nativeDelta, source);
        }
        public virtual void                 Reset()
        {
            Next.Reset ();
        }
        public virtual INativeMotionInput   Next
        {
            get
            {
                return GetNext () as INativeMotionInput;
            }
            set
            {
                SetNext (value);
            }
        }
        protected virtual int               Transform(IMotionInfo info, int nativeDelta)
        {
            return nativeDelta;
        }
    }

    public class NativeDebouncedMotionTransformBase : NativeMotionTransform
    {
        public                                              NativeDebouncedMotionTransformBase(Int32DifferentialFunctionPatternModulator debouncingFunction)
        {
            _debouncingFunction = debouncingFunction;
        }
        public Int32DifferentialFunctionPatternModulator    DebouncingFunction
        {
            [DebuggerStepThrough]
            get
            {
                return _debouncingFunction;
            }
        }
        public override void                                Transmit(IMotionInfo info, int nativeDelta, INativeMotionOutput source)
        {
            var nativeDeltaY = Transform (info, nativeDelta);
            //Debug.WriteLine("{0}{1}(delta = {2,4}) --> {3,4}", Name, TransmitMethodSuffix(info, nativeDelta), nativeDelta, nativeDeltaY);
            Next.Transmit (info, nativeDeltaY, source);
        }
        public override void                                Reset()
        {
            _debouncingFunction.Reset ();
            base.Reset ();
        }
        protected override int                              Transform(IMotionInfo info, int nativeDelta)
        {
            return DebouncingFunction.DF (nativeDelta);
        }

        private readonly Int32DifferentialFunctionPatternModulator _debouncingFunction;
    }

    public class NativeDebouncedMotionTransform : NativeDebouncedMotionTransformBase
    {
        public                          NativeDebouncedMotionTransform(Int32DifferentialFunctionPatternModulator debouncingFunction)
          : base (debouncingFunction)
        {
            Initialize ();
        }
        public override void            OnCoupledTransfer(IMotionInfo info, int nativeDelta, INativeMotionTransferOutput source)
        {
            if (source != _transferSource)
                CompensationInput.Transmit (info, -nativeDelta, this);
        }
        public override void            Reset()
        {
            if (_compensationInput != null)
                _compensationInput.Reset ();
            base.Reset ();
        }

        private INativeMotionInput      CompensationInput
        {
            get
            {
                if (_compensationInput == null)
                {
                    _compensationInput = new CompensationTransform (DebouncingFunction.Clone ());
                    _compensationInput.Next = Next;
                }
                return _compensationInput;
            }
        }
        private void                    Initialize()
        {
            Name = Id.ToString ("'R'00");
            AddHandler (NativeMotionTransfer.TransferingEvent, new NativeMotionTransferEventHandler (OnMotionTransfering));
        }
        private void                    OnMotionTransfering(object sender, NativeMotionTransferEventArgs e)
        {
            _transferSource = e.Source;
        }
        private INativeMotionTransform  _compensationInput;
        private object                  _transferSource;

        private class CompensationTransform : NativeDebouncedMotionTransformBase
        {
            public                              CompensationTransform(Int32DifferentialFunctionPatternModulator debouncingFunction)
              : base (debouncingFunction)
            {
                Name = Id.ToString ("'r'00");
            }
            public override INativeMotionInput  Next
            {
                get
                {
                    return GetNext (false) as INativeMotionInput;
                }
                set
                {
                    SetNext (value, false);
                }
            }
        }
    }
}
