using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lada.Windows.MotionFlow;
using System.Windows;

namespace Lada.Windows.Input
{
    public interface IMouseWheelBehavior : IMouseWheelInputListener, IMotionSink, INativeMotionTarget, IDisposable
    {
        bool                    NestedMotionEnabled
        {
            get;
        }
        MouseWheelDebouncing    Debouncing
        {
            get;
        }
    }

    public abstract class MouseWheelBehavior : IMouseWheelBehavior
    {
        public                                  MouseWheelBehavior(IMouseWheelClient client, IDisposable manipulator = null)
        {
            _client = client;
            _manipulator = manipulator;
        }

        public IMouseWheelController            Controller
        {
            get
            {
                return Client.Controller;
            }
        }
        public IMouseWheelClient                Client
        {
            get
            {
                return _client;
            }
        }
        public abstract bool                    NestedMotionEnabled
        {
            get; protected set;
        }
        public abstract MouseWheelDebouncing    Debouncing
        {
            get; protected set;
        }
        public virtual void                     OnPreviewInput(object sender, MouseWheelInputEventArgs e)
        {
            e.Handled = GetMotionShaft (e.Wheel) == null;
        }
        public virtual void                     OnInput(object sender, MouseWheelInputEventArgs e)
        {
            e.Handled = TransferMotion (e);
        }
        public bool                             CanMove(IMotionInfo info, object context)
        {
            return Client.CanMove (info, context);
        }
        public double                           Coerce(IMotionInfo info, object context, double delta)
        {
            var sinkDelta = NormalizedToSink (delta);
            if (DoubleEx.IsZero (sinkDelta))
                return 0;
            var sinkCoerced = Client.Coerce (info, context, CoerceSinkDelta (sinkDelta));
            if (DoubleEx.IsZero (sinkCoerced))
                return 0;
            return SinkToNormalized (sinkCoerced);
        }
        public void                             Move(IMotionInfo info, object context, double delta)
        {
            var sinkDelta = NormalizedToSink (delta);
            if (DoubleEx.IsZero (sinkDelta))
                return;
            Client.Move (info, context, sinkDelta);
        }
        public virtual double                   SinkToNormalized(double value)
        {
            return value / MotionIncrement;
        }
        public virtual double                   NormalizedToSink(double value)
        {
            return value * MotionIncrement;
        }
        public virtual void                     Dispose()
        {
            if (_manipulator != null)
                _manipulator.Dispose ();
            if (_wheel != null)
                _wheel.ActiveTransferCaseChanged -= OnWheelActiveTransferCaseChanged;
        }

        bool                                    INativeMotionTarget.CanMove(IMotionInfo info, object context)
        {
            return true;
        }
        int                                     INativeMotionTarget.Coerce(IMotionInfo info, object context, int nativeDelta)
        {
            return nativeDelta;
        }
        void                                    INativeMotionTarget.Move(IMotionInfo info, object context, int nativeDelta)
        {
            (context as MouseWheelInputEventArgs).RaiseNativeEvent (nativeDelta);
        }

        protected void                          InvalidateShaft()
        {
            _shaft = null;
        }
        protected bool                          TransferMotionNative(MouseWheelInputEventArgs e)
        {
            return GetMotionShaft (e.Wheel).Transfer (this as INativeMotionTarget, e);
        }
        protected bool                          TransferMotion(MouseWheelInputEventArgs e)
        {
            var shaft = GetMotionShaft (e.Wheel);
            if (shaft.Transfer (MotionInput, e))
                return true; // client can still move - stop event propagation
            if (NestedMotionEnabled)
                return false; // let the mouse wheel event propagate up the visual tree
                              // empty the shaft transfer staging area and stop event propagation
            return shaft.Transfer (NativeMotionTarget.Terminal, e);
        }

        protected virtual IMotionTarget         MotionInput
        {
            get
            {
                return this;
            }
        }
        protected virtual double                MotionIncrement
        {
            get
            {
                return Client.MotionIncrement;
            }
        }
        protected virtual double                CoerceSinkDelta(double sinkDelta)
        {
            return sinkDelta;
        }

        protected virtual IMouseWheelShaft      GetMotionShaft(MouseWheel wheel, IMouseWheelTransferCase transferCase)
        {
            switch (Debouncing)
            {
                case MouseWheelDebouncing.Auto:
                    return GetMotionShaftAuto (wheel, transferCase, -1);
                case MouseWheelDebouncing.None:
                    return transferCase[0];  // no debouncing
                case MouseWheelDebouncing.Single:
                    return transferCase[1];  // one debouncing cell per click - same as a standard resolution notch
                default:
                    throw new NotImplementedException ();
            }
        }
        protected IMouseWheelShaft              GetMotionShaftAuto(MouseWheel wheel, IMouseWheelTransferCase transferCase, double debouncingCellCount)
        {
            var resolution = wheel.Resolution;
            if (DoubleEx.AreClose (resolution, (int) resolution))
                return transferCase[debouncingCellCount];  // the most granular debouncing
            else
                return transferCase[0];   // no debouncing if wheel resolution not integral
        }
        protected IMouseWheelShaft              GetMotionShaftAuto(MouseWheel wheel, IMouseWheelTransferCase transferCase, int debouncingCellCount)
        {
            var resolution = wheel.Resolution;
            if (DoubleEx.AreClose (resolution, (int) resolution))
                return transferCase[debouncingCellCount];  // the most granular debouncing
            else
                return transferCase[0];   // no debouncing if wheel resolution not integral
        }

        private MouseWheel                      Wheel
        {
            get
            {
                return _wheel;
            }
            set
            {
                if (_wheel == value)
                    return;
                if (_wheel != null)
                    _wheel.ActiveTransferCaseChanged -= OnWheelActiveTransferCaseChanged;
                _wheel = value;
                _wheel.ActiveTransferCaseChanged += OnWheelActiveTransferCaseChanged;
                _shaft = null;
            }
        }
        private IMouseWheelShaft                GetMotionShaft(MouseWheel wheel)
        {
            Wheel = wheel;
            if (_shaft == null)
                _shaft = GetMotionShaft (wheel, wheel.ActiveTransferCase);
            wheel.ActiveTransferCase.ActiveShaft = _shaft;
            return _shaft;
        }
        private void                            OnWheelActiveTransferCaseChanged(object sender, EventArgs e)
        {
            InvalidateShaft ();
        }

        private readonly IMouseWheelClient      _client;
        private readonly IDisposable            _manipulator;
        private IMouseWheelShaft                _shaft;
        private MouseWheel                      _wheel;
    }
    
    public class MouseWheelNativeBehavior : MouseWheelBehavior
    {
        public                                  MouseWheelNativeBehavior(IMouseWheelClient client, IDisposable manipulator = null) : base (client, manipulator) { }
        public override bool                    NestedMotionEnabled
        {
            get
            {
                return false;
            }
            protected set
            {
                throw new NotSupportedException ();
            }
        }
        public override MouseWheelDebouncing    Debouncing
        {
            get
            {
                return MouseWheelDebouncing.None;
            }
            protected set
            {
                throw new NotSupportedException ();
            }
        }
        public override void                    OnInput(object sender, MouseWheelInputEventArgs e)
        {
            e.Handled = TransferMotionNative (e);
        }

        protected override IMouseWheelShaft     GetMotionShaft(MouseWheel wheel, IMouseWheelTransferCase transferCase)
        {
            return transferCase[0];
        }
    }
}
