using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Lada.Windows.MotionFlow;

namespace Lada.Windows.Input
{
  #region MouseWheelZoomBehavior
  public class MouseWheelZoomBehavior : MouseWheelBehavior
  {
    #region Fields
    private MouseWheelDebouncing _debouncing;
    #endregion

    #region Initialization
    public MouseWheelZoomBehavior(IMouseWheelClient client)
      : base(client)
    {
      var element = Client.Controller.Element;
      NestedMotionEnabled = MouseWheel.GetNestedZoom(element);
      Debouncing = MouseWheel.GetZoomDebouncing(element);
      MouseWheel.NestedZoomProperty.AddValueChanged(element, OnNestedZoomChanged);
      MouseWheel.ZoomDebouncingProperty.AddValueChanged(element, OnDebouncingChanged);
    }
    #endregion

    #region IDisposable
    public override void Dispose()
    {
      var element = Client.Controller.Element;
      MouseWheel.NestedZoomProperty.RemoveValueChanged(element, OnNestedZoomChanged);
      MouseWheel.ZoomDebouncingProperty.RemoveValueChanged(element, OnDebouncingChanged);
      base.Dispose();
    }
    #endregion

    #region IMouseWheelBehavior
    public override bool NestedMotionEnabled { get; protected set; }

    public override MouseWheelDebouncing Debouncing
    {
      get { return _debouncing; }
      protected set
      {
        if (_debouncing == value) return;
        _debouncing = value;
        InvalidateShaft();
      }
    }
    #endregion

    #region Helpers
    private void OnNestedZoomChanged(object sender, EventArgs e) { NestedMotionEnabled = MouseWheel.GetNestedZoom(sender as DependencyObject); }
    private void OnDebouncingChanged(object sender, EventArgs e) { Debouncing = MouseWheel.GetZoomDebouncing(sender as DependencyObject); }
    #endregion
  }
  #endregion

  #region MouseWheelSmoothZoomBehavior
  public class MouseWheelSmoothZoomBehavior : MouseWheelZoomBehavior
  {
    #region Fields
    private readonly MotionSmoothingTarget _motionSmoothing;
    #endregion

    #region Initialization
    public MouseWheelSmoothZoomBehavior(IMouseWheelClient client, IMotionFilter motionFilter)
      : base(client)
    {
      _motionSmoothing = new MotionSmoothingTarget(motionFilter) { Next = this };
    }
    #endregion

    #region IDisposable
    public override void Dispose()
    {
      _motionSmoothing.Dispose();
      base.Dispose();
    }
    #endregion

    #region MouseWheelBehavior
    protected override IMotionTarget MotionInput { get { return _motionSmoothing; } }
    #endregion
  }
  #endregion
}
