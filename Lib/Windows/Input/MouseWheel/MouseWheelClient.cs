using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Lada.Windows.MotionFlow;
using System.Windows.Input;

namespace Lada.Windows.Input
{
    public interface IMouseWheelClient : IMouseWheelInputListener, IMotionTarget, IDisposable
    {
        IMouseWheelController   Controller
        {
            get;
        }
        double                  MotionIncrement
        {
            get;
        }
        MouseWheelSmoothing     Smoothing
        {
            get;
        }
        IInputElement           ExitElement
        {
            get;
        }
        bool                    IsActive(MouseWheelInputEventArgs e);
        void                    Unload();
    }

    public abstract class MouseWheelClient : IMouseWheelClient
    {
        public                                      MouseWheelClient(IMouseWheelController controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException ("controller");
            }
            this.Controller = controller;
        }
        public IMouseWheelController                Controller
        {
            get;
        }
        public virtual double                       MotionIncrement => 1.0;
        public virtual MouseWheelSmoothing          Smoothing
        {
            get
            {
                return MouseWheelSmoothing.None;
            }
            protected set
            {
                throw new NotImplementedException ();
            }
        }
        public abstract ModifierKeys                Modifiers
        {
            get;
        }
        public IInputElement                        ExitElement
        {
            get
            {
                if (_exitElement == null)
                {
                    _exitElement = GetExitElement ();
                }
                return _exitElement;
            }
        }
        public virtual bool                         IsActive(MouseWheelInputEventArgs e)
        {
            EnsureLoaded ();
            return Modifiers == Keyboard.Modifiers;
        }
        public void                                 Unload()
        {
            if (_loaded)
            {
                _loaded = false;
                OnUnloading ();
                InvalidateBehavior ();
            }
        }
        public void                                 OnPreviewInput(object sender, MouseWheelInputEventArgs e)
        {
            Behavior.OnPreviewInput (sender, e);
        }
        public void                                 OnInput(object sender, MouseWheelInputEventArgs e)
        {
            Behavior.OnInput (sender, e);
        }
        public virtual bool                         CanMove(IMotionInfo info, object context) => true;
        public virtual double                       Coerce(IMotionInfo info, object context, double delta) => delta;
        public virtual void                         Move(IMotionInfo info, object context, double delta)
        {
        }
        public virtual void                         Dispose() => Unload ();

        protected IMouseWheelInputListener          Behavior
        {
            get
            {
                if (_behavior == null)
                {
                    EnsureLoaded ();
                    _behavior = CreateBehavior ();
                }
                return _behavior;
            }
        }
        protected bool                              Enhanced
        {
            get
            {
                return _enhanced;
            }
            set
            {
                if (_enhanced == value)
                    return;
                _enhanced = value;
                InvalidateBehavior ();
            }
        }
        protected void                              InvalidateBehavior()
        {
            DisposeBehavior ();
            _behavior = null;
        }
        protected abstract IMouseWheelInputListener CreateBehavior();
        protected virtual IInputElement             GetExitElement()
        {
            return Controller.Element.GetVisualAncestors ().OfType<IInputElement> ().FirstOrDefault ();
        }
        protected virtual void                      OnLoading()
        {
            _enhanced = MouseWheel.GetEnhanced (Controller.Element);
            MouseWheel.EnhancedProperty.AddValueChanged (Controller.Element, OnEnhancedChanged);
        }
        protected virtual void                      OnUnloading()
        {
            MouseWheel.EnhancedProperty.RemoveValueChanged (Controller.Element, OnEnhancedChanged);
        }
        protected void                              EnsureLoaded()
        {
            if (!_loaded)
            {
                _loaded = true;
                OnLoading ();
            }
        }
        private void                                DisposeBehavior()
        {
            if (_behavior is IDisposable)
                (_behavior as IDisposable).Dispose ();
        }
        private void                                OnEnhancedChanged(object sender, EventArgs e)
        {
            Enhanced = MouseWheel.GetEnhanced (sender as DependencyObject);
        }

        private IMouseWheelInputListener            _behavior;
        private bool                                _enhanced;
        private bool                                _loaded;
        private IInputElement                       _exitElement;

    }
}
