using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Lada.Windows.MotionFlow
{
    public delegate void MotionTransferEventHandler(object sender, MotionTransferEventArgs e);

    public class MotionTransferEventArgs : RoutedEventArgs
    {
        public                  MotionTransferEventArgs(IMotionInfo info, double delta)
        {
            RoutedEvent = MotionTransfer.TransferedEvent;
            MotionInfo = info;
            Delta = delta;
        }
        public IMotionInfo      MotionInfo
        {
            get; private set;
        }
        public double           Delta
        {
            get; private set;
        }

        protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
        {
            var handler = (MotionTransferEventHandler) genericHandler;
            handler (genericTarget, this);
        }
    }
    public delegate void NativeMotionTransferEventHandler(object sender, NativeMotionTransferEventArgs e);

    public class NativeMotionTransferEventArgs : RoutedEventArgs
    {
        public                  NativeMotionTransferEventArgs(IMotionInfo info, int nativeDelta)
        {
            MotionInfo = info;
            NativeDelta = nativeDelta;
        }
        public IMotionInfo      MotionInfo
        {
            get; private set;
        }
        public int              NativeDelta
        {
            get; private set;
        }

        protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
        {
            var handler = (NativeMotionTransferEventHandler) genericHandler;
            handler (genericTarget, this);
        }
    }
}
