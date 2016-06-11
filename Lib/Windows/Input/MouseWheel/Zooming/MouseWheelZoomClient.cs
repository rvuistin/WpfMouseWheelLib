using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Lada.Windows.Input;
using Lada.Windows.MotionFlow;

namespace Lada.Windows.Input
{
  public class MouseWheelZoomClient : MouseWheelClient
  {
    #region Fields
    private MouseWheelSmoothing _smoothing;
    private ModifierKeys _modifiers;
    #endregion

    #region Initialization
    public MouseWheelZoomClient(IMouseWheelController controller)
      : base(controller)
    {
    } 
    #endregion

    #region IMouseWheelClient
    public override double MotionIncrement { get { return ZoomElement.ZoomIncrement; } }
    #endregion

    #region MouseWheelClient
    protected override void OnLoading()
    {
      base.OnLoading();
      var element = Controller.Element;
      _smoothing = MouseWheel.GetZoomSmoothing(element);
      _modifiers = MouseWheel.GetZoomModifiers(element);
      MouseWheel.ZoomSmoothingProperty.AddValueChanged(element, OnSmoothingChanged);
      MouseWheel.ZoomModifiersProperty.AddValueChanged(element, OnModifiersChanged);
    }
    protected override void OnUnloading()
    {
      var element = Controller.Element;
      MouseWheel.ZoomModifiersProperty.RemoveValueChanged(element, OnModifiersChanged);
      MouseWheel.ZoomSmoothingProperty.RemoveValueChanged(element, OnSmoothingChanged);
      base.OnUnloading();
    }
    protected override IMouseWheelInputListener CreateBehavior()
    {
      if (Enhanced)
      {
        switch (Smoothing)
        {
          case MouseWheelSmoothing.None:   return new MouseWheelZoomBehavior(this);
          case MouseWheelSmoothing.Linear: return new MouseWheelSmoothZoomBehavior(this, new MouseWheelLinearFilter());
          case MouseWheelSmoothing.Smooth: return new MouseWheelSmoothZoomBehavior(this, new MouseWheelSmoothingFilter());
          default: throw new NotSupportedException();
        }
      }
      else
        return new MouseWheelNativeBehavior(this);
    }
    #endregion

    #region IMouseWheelClient
    public override MouseWheelSmoothing Smoothing
    {
      get { return _smoothing; }
      protected set
      {
        if (_smoothing == value) return;
        _smoothing = value;
        InvalidateBehavior();
      }
    }
    public override ModifierKeys Modifiers
    {
      get { return _modifiers; }
    }
    #endregion

    #region IMotionTarget
    public override bool CanMove(IMotionInfo info, object context)
    {
      var z = ZoomElement;
      return info.Direction < 0 ? z.CanIncreaseZoom : z.CanDecreaseZoom;
    }
    public override double Coerce(IMotionInfo info, object context, double delta)
    {
      var z = ZoomElement;
      int direction = info.Direction;
      if (direction < 0)
      {
        // increasing zoom
        var zoomableDelta = z.Zoom - z.MaxZoom;
        return Math.Max(zoomableDelta, delta);
      }
      else
      {
        // decreasing zoom
        var zoomableDelta = z.Zoom - z.MinZoom;
        return Math.Min(zoomableDelta, delta);
      }
    }
    public override void Move(IMotionInfo info, object context, double delta)
    {
      ZoomElement.Zoom -= delta;
    }
    #endregion

    #region Helpers
    private IZoomElement ZoomElement { get { return (Controller as MouseWheelFrameworkLevelController).FrameworkLevelElement as IZoomElement; } }

    private void OnSmoothingChanged(object sender, EventArgs e) { Smoothing  = MouseWheel.GetZoomSmoothing(sender as DependencyObject); }
    private void OnModifiersChanged(object sender, EventArgs e) { _modifiers = MouseWheel.GetZoomModifiers(sender as DependencyObject); }
    #endregion
  }
}
