using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Lada.Windows.MotionFlow
{
    public class NativeMotionTerminal : MotionElement, INativeMotionInput, IMotionInput
    {
        public static readonly NativeMotionTerminal Current = new NativeMotionTerminal();

        public void Transmit(IMotionInfo info, int nativeDelta, INativeMotionOutput source)
        {
        }
        public void OnCoupledTransfer(IMotionInfo info, int nativeDelta, INativeMotionTransferOutput source)
        {
        }
        public void Reset()
        {
        }
        public void Transmit(IMotionInfo info, double delta, IMotionOutput source)
        {
        }
        public void OnCoupledTransfer(IMotionInfo info, double delta, IMotionTransferOutput source)
        {
        }
    }
}
