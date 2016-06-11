using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using Lada.Windows.MotionFlow;
using System.Diagnostics;
using System.Windows.Controls;

namespace Lada.Windows.Input
{
    public class MouseWheelInputEventArgs : MouseWheelEventArgs
    {
        public                          MouseWheelInputEventArgs(IMouseWheelController controller, MouseWheel wheel, int timestamp, int delta, Orientation orientation)
          : base (wheel.MouseDevice, Math.Max(0, timestamp), delta)
        {
            Controller  = controller;
            Wheel       = wheel;
            Orientation = orientation;
        }
        public MouseWheel               Wheel
        {
            get;
        }
        public Orientation              Orientation
        {
            get;
        }
        public IMouseWheelController    Controller
        {
            get;
            set;
        }
        public void                     RaiseNativeEvent(int nativeDelta)
        {
            Controller.InputElement.RaiseEvent (NativeDeltaToNativeEventArgs (nativeDelta));
        }
        public void                     EndCommand()
        {
            if (Handled)
                Controller.ExitElement?.RaiseEvent (CreateNativeEventArgs (Timestamp, Delta));
            else
                Handled = Wheel.Transfer (MouseWheelNativeMotionTarget.Current, this);
        }
        public override string          ToString() => $"{Orientation}, Delta = {Delta}, Timestamp = {Timestamp}";

        protected override void         InvokeEventHandler(Delegate genericHandler, object genericTarget)
        {
            var handler = (MouseWheelInputEventHandler) genericHandler;
            handler (genericTarget, this);
        }

        private MouseWheelEventArgs                 CreateNativeEventArgs(int timestamp, int delta)
        {
            return new MouseWheelEventArgs (Wheel.MouseDevice, timestamp, delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Handled = Handled
            };
        }
        private IEnumerable<MouseWheelEventArgs>    NormalizedDeltaToNativeEventArgs(int normalizedDelta)
        {
            if (normalizedDelta == 0)
                yield break;
            int clickDisplacement = Wheel.NormalizedToNative (Math.Sign (normalizedDelta));
            for (int i = 0; i < Math.Abs (normalizedDelta); ++i)
                yield return CreateNativeEventArgs (Timestamp + i * 10, clickDisplacement);
        }
        private MouseWheelEventArgs                 NativeDeltaToNativeEventArgs(int nativeDelta)
        {
            return nativeDelta == 0 ? null : CreateNativeEventArgs (Timestamp, nativeDelta);
        }

        private class MouseWheelNativeMotionTarget : INativeMotionTarget
        {

            public static INativeMotionTarget   Current
            {
                get
                {
                    return _current;
                }
            }
            public bool                         CanMove(IMotionInfo info, object context)
            {
                return true;
            }
            public int                          Coerce(IMotionInfo info, object context, int nativeDelta)
            {
                return nativeDelta;
            }
            public void                         Move(IMotionInfo info, object context, int nativeDelta)
            {
                var e = context as MouseWheelInputEventArgs;
                var args = e.NativeDeltaToNativeEventArgs (nativeDelta);
                if (args != null)
                    e.Controller.ExitElement.RaiseEvent (args);
            }

            private static readonly INativeMotionTarget _current = new MouseWheelNativeMotionTarget();
        }
    }

    public delegate void MouseWheelInputEventHandler(object sender, MouseWheelInputEventArgs e);
}
