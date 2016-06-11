using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Lada.Windows.MotionFlow;

namespace Lada.Windows.Input
{
  #region MouseWheelAdaptationBehavior
  public class MouseWheelAdaptationBehavior : MouseWheelBehavior
  {
    #region Fields
    private MouseWheelDebouncing _debouncing;
    #endregion

    #region Initialization
    public MouseWheelAdaptationBehavior(IMouseWheelClient client)
      : base(client)
    {
      var element = Client.Controller.Element;
      NestedMotionEnabled = MouseWheel.GetNestedMotion(element);
      Debouncing = MouseWheel.GetDebouncing(element);
      MouseWheel.NestedMotionProperty.AddValueChanged(element, OnNestedMotionChanged);
      MouseWheel.DebouncingProperty.AddValueChanged(element, OnDebouncingChanged);
    }
    #endregion

    #region IDisposable
    public override void Dispose()
    {
      var element = Client.Controller.Element;
      MouseWheel.NestedMotionProperty.RemoveValueChanged(element, OnNestedMotionChanged);
      MouseWheel.DebouncingProperty.RemoveValueChanged(element, OnDebouncingChanged);
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
    private void OnNestedMotionChanged(object sender, EventArgs e) { NestedMotionEnabled = MouseWheel.GetNestedMotion(sender as DependencyObject); }
    private void   OnDebouncingChanged(object sender, EventArgs e) { Debouncing          = MouseWheel.GetDebouncing  (sender as DependencyObject); }
    #endregion
  }
  #endregion

  #region MouseWheelSmoothAdaptationBehavior
  public class MouseWheelSmoothAdaptationBehavior : MouseWheelAdaptationBehavior
  {
    #region Fields
    private readonly MotionSmoothingTarget _motionSmoothing;
    #endregion

    #region Initialization
    public MouseWheelSmoothAdaptationBehavior(IMouseWheelClient client, IMotionFilter motionFilter)
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
