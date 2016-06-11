using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Lada.Windows.MotionFlow
{
    public class MotionTransferGroup : MotionElement, IMotionTransfer
    {
        public                          MotionTransferGroup(IMotionInput input, IMotionTransferOutput output)
        {
            Input = input;
            Output = output;
        }
        public IMotionInfo              MotionInfo
        {
            get
            {
                return Output.MotionInfo;
            }
        }
        public double                   Remainder
        {
            get
            {
                return Output.Remainder;
            }
        }
        public void                     Transmit(IMotionInfo info, double delta, IMotionOutput source)
        {
            Input.Transmit (info, delta, source);
        }
        public void                     OnCoupledTransfer(IMotionInfo info, double delta, IMotionTransferOutput source)
        {
            Input.OnCoupledTransfer (info, delta, source);
        }
        public void                     Reset()
        {
            Input.Reset ();
        }
        public bool                     Transfer(IMotionTarget target, object context)
        {
            return Output.Transfer (target, context);
        }
        public override string          ToString()
        {
            return string.Format ("Input={{{0}}}, Output={{{1}}}", Input, Output);
        }

        protected IMotionInput          Input
        {
            get; private set;
        }
        protected IMotionTransferOutput Output
        {
            get; private set;
        }
    }

    public class NativeMotionTransferGroup : MotionElement, INativeMotionTransfer
    {
        public                                  NativeMotionTransferGroup(INativeMotionInput input, INativeMotionTransferOutput output)
        {
            if (input == null)
                throw new ArgumentNullException ("input");
            if (output == null)
                throw new ArgumentNullException ("output");

            input.SetParent (this);
            Input = input;
            Output = output;
        }
        public IMotionInfo                      MotionInfo
        {
            get
            {
                return Output.MotionInfo;
            }
        }
        public double                           Remainder
        {
            get
            {
                return Output.Remainder;
            }
        }
        public int                              NativeRemainder
        {
            get
            {
                return Output.NativeRemainder;
            }
        }
        public void                             Transmit(IMotionInfo info, int nativeDelta, INativeMotionOutput source)
        {
            Input.Transmit (info, nativeDelta, source);
        }
        public void                             OnCoupledTransfer(IMotionInfo info, int nativeDelta, INativeMotionTransferOutput source)
        {
            Input.OnCoupledTransfer (info, nativeDelta, source);
        }
        public void                             Reset()
        {
            Input.Reset ();
        }
        public bool                             Transfer(IMotionTarget target, object context)
        {
            return Output.Transfer (target, context);
        }
        public bool                             Transfer(INativeMotionTarget target, object context)
        {
            return Output.Transfer (target, context);
        }
        public override string                  ToString()
        {
            return string.Format ("Input={{{0}}}, Output={{{1}}}", Input, Output);
        }

        protected INativeMotionInput            Input
        {
            get; private set;
        }
        protected INativeMotionTransferOutput   Output
        {
            get; private set;
        }
    }
}
