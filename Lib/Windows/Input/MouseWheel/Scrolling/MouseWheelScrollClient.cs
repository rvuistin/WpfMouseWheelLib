using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Lada.ComponentModel;
using Lada.Maths;
using Lada.Windows.Controls;
using Lada.Windows.MotionFlow;

namespace Lada.Windows.Input
{
    public class MouseWheelScrollClient : MouseWheelClient
    {
        public MouseWheelScrollClient(IMouseWheelController controller, Orientation orientation)
          : base (controller)
        {
            _orientation = orientation;
            _scrollAreaOrientation = ScrollViewer.GetScrollAreaOrientation ();
        }

        public ScrollViewer                         ScrollViewer
        {
            get
            {
                return Controller.Element as ScrollViewer;
            }
        }
        public Orientation                          ScrollAreaOrientation
        {
            get
            {
                return _scrollAreaOrientation;
            }
        }
        public Orientation                          Orientation
        {
            get
            {
                return _orientation;
            }
        }
        public bool                                 CanContentScroll
        {
            get
            {
                return ScrollViewer.CanContentScroll;
            }
            set
            {
                if (CanContentScroll == value)
                    return;
                ScrollViewer.CanContentScroll = value;
            }
        }
        public override MouseWheelSmoothing         Smoothing
        {
            get
            {
                return _smoothing;
            }
            protected set
            {
                if (_smoothing == value)
                    return;
                _smoothing = value;
                InvalidateBehavior ();
            }
        }
        public override ModifierKeys                Modifiers
        {
            get
            {
                return _modifiers;
            }
        }
        public override bool IsActive(MouseWheelInputEventArgs e)
        {
            if (e.Orientation == Orientation.Horizontal)
            {
                EnsureLoaded ();
                return Orientation == Orientation.Horizontal;
            }
            else
            {
                return base.IsActive (e);
            }
        }
        public override bool                        CanMove(IMotionInfo info, object context)
        {
            return !DoubleEx.IsZero (GetScrollableDisplacement (info.Direction));
        }
        public override double                      Coerce(IMotionInfo info, object context, double delta)
        {
            var direction = info.Direction;
            var scrollableDelta = GetScrollableDisplacement (direction);
            return direction < 0 ? Math.Max (scrollableDelta, delta) : Math.Min (scrollableDelta, delta);
        }
        public override void                        Move(IMotionInfo info, object context, double delta)
        {
            ScrollViewer.Scroll (Orientation, delta);
        }

        protected override void                     OnLoading()
        {
            base.OnLoading ();
            var element = Controller.Element;
            if (Orientation == Orientation.Vertical)
            {
                _scrollMode = MouseWheel.GetVScrollMode (element);
                _smoothing = MouseWheel.GetVScrollSmoothing (element);
                _modifiers = MouseWheel.GetVScrollModifiers (element);
                MouseWheel.VScrollModeProperty.AddValueChanged (element, OnVScrollModeChanged);
                MouseWheel.VScrollSmoothingProperty.AddValueChanged (element, OnVSmoothingChanged);
                MouseWheel.VScrollModifiersProperty.AddValueChanged (element, OnVModifiersChanged);

                MouseWheel.GetLogicalVScrollIncrement (element).SetOrientation (Orientation.Vertical);
                MouseWheel.GetPhysicalVScrollIncrement (element).SetOrientation (Orientation.Vertical);
            }
            else
            {
                _scrollMode = MouseWheel.GetHScrollMode (element);
                _smoothing = MouseWheel.GetHScrollSmoothing (element);
                _modifiers = MouseWheel.GetHScrollModifiers (element);
                MouseWheel.HScrollModeProperty.AddValueChanged (element, OnHScrollModeChanged);
                MouseWheel.HScrollSmoothingProperty.AddValueChanged (element, OnHSmoothingChanged);
                MouseWheel.HScrollModifiersProperty.AddValueChanged (element, OnHModifiersChanged);

                MouseWheel.GetLogicalHScrollIncrement (element).SetOrientation (Orientation.Horizontal);
                MouseWheel.GetPhysicalHScrollIncrement (element).SetOrientation (Orientation.Horizontal);
            }
        }
        protected override void                     OnUnloading()
        {
            var element = Controller.Element;
            if (Orientation == Orientation.Vertical)
            {
                MouseWheel.VScrollSmoothingProperty.RemoveValueChanged (element, OnVSmoothingChanged);
                MouseWheel.VScrollModeProperty.RemoveValueChanged (element, OnVScrollModeChanged);
                MouseWheel.VScrollModifiersProperty.RemoveValueChanged (element, OnVModifiersChanged);
            }
            else
            {
                MouseWheel.HScrollSmoothingProperty.RemoveValueChanged (element, OnHSmoothingChanged);
                MouseWheel.HScrollModeProperty.RemoveValueChanged (element, OnHScrollModeChanged);
                MouseWheel.HScrollModifiersProperty.RemoveValueChanged (element, OnHModifiersChanged);
            }
            base.OnUnloading ();
        }
        protected override IInputElement            GetExitElement()
        {
            // skip mouse wheel event implementors (TextBoxBase subclasses)
            var implementor = GetMouseWheelEventImplementor ((Controller as MouseWheelFrameworkLevelController).FrameworkLevelElement);
            if (implementor != null)
            {
                return implementor.GetVisualAncestors ().OfType<IInputElement> ().FirstOrDefault ();
            }
            else
            {
                return base.GetExitElement ();
            }
        }
        protected override IMouseWheelInputListener CreateBehavior()
        {
            if (Enhanced)
            {
                switch (ScrollMode)
                {
                    case MouseWheelScrollMode.Auto:
                        return CreateEnhancedAutoBehavior ();
                    case MouseWheelScrollMode.Physical:
                        return CreateEnhancedPhysicalBehavior ();
                    default:
                        throw new NotSupportedException ();
                }
            }
            else
            {
                switch (ScrollMode)
                {
                    case MouseWheelScrollMode.Auto:
                        return CreateNativeAutoBehavior ();
                    case MouseWheelScrollMode.Physical:
                        return CreateNativePhysicalBehaviorItem ();
                    default:
                        throw new NotSupportedException ();
                }
            }
        }

        private static IEnumerable<Type>            MouseWheelEventImplementorTypes
        {
            get
            {
                yield return typeof (TextBoxBase);
                yield return typeof (FlowDocumentScrollViewer);
                yield return typeof (FlowDocumentPageViewer);
            }
        }
        private static bool                         ImplementsMouseWheelEvent(DependencyObject obj)
        {
            if (obj == null)
                return false;
            var objType = obj.GetType ();
            return MouseWheelEventImplementorTypes.Any (t => t.IsAssignableFrom (objType));
        }
        private static DependencyObject             GetMouseWheelEventImplementor(IFrameworkLevelElement element)
        {
            var templatedParent = element.TemplatedParent;
            return ImplementsMouseWheelEvent (templatedParent) ? templatedParent : null;
        }

        private bool                                LogicalScrollEnabled
        {
            get
            {
                return ScrollViewer.CanContentScroll && ScrollAreaOrientation == Orientation;
            }
        }
        private bool                                HostImplementsMouseWheelEvent
        {
            get
            {
                return ImplementsMouseWheelEvent (ScrollViewer.TemplatedParent);
            }
        }

        private MouseWheelScrollMode                ScrollMode
        {
            get
            {
                return _scrollMode;
            }
            set
            {
                if (_scrollMode == value)
                    return;
                _scrollMode = value;
                InvalidateBehavior ();
            }
        }
        private IDisposable                         CreateScrollViewerManipulator(bool canContentScroll)
        {
            if (Orientation == ScrollAreaOrientation && canContentScroll != ScrollViewer.CanContentScroll && !HostImplementsMouseWheelEvent)
                return new ScrollViewerManipulator (ScrollViewer, canContentScroll);
            return null;
        }
        private double                              GetScrollableDisplacement(int direction)
        {
            return ScrollViewer.GetScrollableDisplacement (Orientation, direction);
        }

        private IMouseWheelInputListener            CreateNativeAutoBehavior()
        {
            if (LogicalScrollEnabled)
                return CreateNativeLogicalBehaviorItem ();
            else
                return CreateNativePhysicalBehaviorItem ();
        }
        private IMouseWheelInputListener            CreateNativeLogicalBehavior()
        {
            if (LogicalScrollEnabled)
                return CreateNativeLogicalBehaviorItem ();
            else
                return CreateNativePhysicalBehaviorItem ();
        }
        private IMouseWheelInputListener            CreateNativePhysicalBehaviorItem()
        {
            return new MouseWheelNativeBehavior (this, CreateScrollViewerManipulator (false));
        }
        private IMouseWheelInputListener            CreateNativeLogicalBehaviorItem()
        {
            return new MouseWheelNativeBehavior (this, CreateScrollViewerManipulator (true));
        }
        private IMouseWheelInputListener            CreateEnhancedAutoBehavior()
        {
            if (LogicalScrollEnabled)
            {
                if (ScrollViewer.HasNestedScrollFrames () || HostImplementsMouseWheelEvent)
                    return CreateEnhancedPhysicalBehavior ();
                else
                    return CreateEnhancedLogicalBehaviorItem ();
            }
            else
                return CreateEnhancedPhysicalBehavior ();
        }
        private IMouseWheelInputListener            CreateEnhancedLogicalBehavior()
        {
            if (LogicalScrollEnabled)
            {
                if (HostImplementsMouseWheelEvent)
                    return CreateEnhancedPhysicalBehaviorItem ();
                else
                    return CreateEnhancedLogicalBehaviorItem ();
            }
            else
                return CreateEnhancedPhysicalBehaviorItem ();
        }
        private IMouseWheelInputListener            CreateEnhancedPhysicalBehavior()
        {
            switch (Smoothing)
            {
                case MouseWheelSmoothing.None:
                    return CreateEnhancedPhysicalBehaviorItem ();
                case MouseWheelSmoothing.Linear:
                    return CreateEnhancedLinearBehaviorItem ();
                case MouseWheelSmoothing.Smooth:
                    return CreateEnhancedSmoothBehaviorItem ();
                default:
                    throw new NotImplementedException ();
            }
        }
        private IMouseWheelInputListener            CreateEnhancedSmoothingBehavior()
        {
            if (Smoothing == MouseWheelSmoothing.Linear)
                return CreateEnhancedLinearBehaviorItem ();
            else
                return CreateEnhancedSmoothBehaviorItem ();
        }
        private IMouseWheelInputListener            CreateEnhancedLogicalBehaviorItem()
        {
            return new MouseWheelLogicalScrollBehavior (this, CreateScrollViewerManipulator (true));
        }
        private IMouseWheelInputListener            CreateEnhancedPhysicalBehaviorItem()
        {
            return new MouseWheelPhysicalScrollBehavior (this, CreateScrollViewerManipulator (false));
        }
        private IMouseWheelInputListener            CreateEnhancedLinearBehaviorItem()
        {
            return new MouseWheelSmoothScrollBehavior (this, CreateScrollViewerManipulator (false), new MouseWheelLinearFilter ());
        }
        private IMouseWheelInputListener            CreateEnhancedSmoothBehaviorItem()
        {
            return new MouseWheelSmoothScrollBehavior (this, CreateScrollViewerManipulator (false), new MouseWheelSmoothingFilter ());
        }

        private void                                OnVSmoothingChanged(object sender, EventArgs e)
        {
            Smoothing = MouseWheel.GetVScrollSmoothing (sender as DependencyObject);
        }
        private void                                OnHSmoothingChanged(object sender, EventArgs e)
        {
            Smoothing = MouseWheel.GetHScrollSmoothing (sender as DependencyObject);
        }
        private void                                OnVScrollModeChanged(object sender, EventArgs e)
        {
            ScrollMode = MouseWheel.GetVScrollMode (sender as DependencyObject);
        }
        private void                                OnHScrollModeChanged(object sender, EventArgs e)
        {
            ScrollMode = MouseWheel.GetHScrollMode (sender as DependencyObject);
        }
        private void                                OnVModifiersChanged(object sender, EventArgs e)
        {
            _modifiers = MouseWheel.GetVScrollModifiers (sender as DependencyObject);
        }
        private void                                OnHModifiersChanged(object sender, EventArgs e)
        {
            _modifiers = MouseWheel.GetHScrollModifiers (sender as DependencyObject);
        }

        private readonly Orientation                _scrollAreaOrientation;
        private readonly Orientation                _orientation;
        private MouseWheelScrollMode                _scrollMode;
        private MouseWheelSmoothing                 _smoothing;
        private ModifierKeys                        _modifiers;

        private class ScrollViewerManipulator : IDisposable
        {
            public                          ScrollViewerManipulator(ScrollViewer scrollViewer, bool canContentScroll)
            {
                _scrollViewer = scrollViewer;
                _canContentScroll = scrollViewer.CanContentScroll;
                scrollViewer.CanContentScroll = canContentScroll;
            }
            public void                     Dispose()
            {
                _scrollViewer.CanContentScroll = _canContentScroll;
            }

            private readonly ScrollViewer   _scrollViewer;
            private readonly bool           _canContentScroll;
        }
    }
}
