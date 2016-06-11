using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Lada.Windows.MotionFlow;

namespace Lada.Windows.Input
{
  public class MouseWheelAdaptationClient : MouseWheelClient
  {
    #region Fields
    private MouseWheelSmoothing _smoothing;
    private ModifierKeys _modifiers;
    private double _minimum;
    private double _maximum;
    #endregion

    #region Initialization
    public MouseWheelAdaptationClient(IMouseWheelController controller)
      : base(controller)
    {
    } 
    #endregion

    #region MouseWheelClient
    protected override void OnLoading()
    {
      base.OnLoading();
      var element = Controller.Element;
      _smoothing = MouseWheel.GetSmoothing(element);
      _modifiers = MouseWheel.GetModifiers(element);
      _minimum = MouseWheel.GetMinimum(element);
      _maximum = MouseWheel.GetMaximum(element);
      MouseWheel.SmoothingProperty.AddValueChanged(element, OnSmoothingChanged);
      MouseWheel.ModifiersProperty.AddValueChanged(element, OnModifiersChanged);
      MouseWheel.MinimumProperty.AddValueChanged(element, OnMinimumChanged);
      MouseWheel.MaximumProperty.AddValueChanged(element, OnMaximumChanged);
    }
    protected override void OnUnloading()
    {
      var element = Controller.Element;
      MouseWheel.MinimumProperty.RemoveValueChanged(element, OnMinimumChanged);
      MouseWheel.MaximumProperty.RemoveValueChanged(element, OnMaximumChanged);
      MouseWheel.ModifiersProperty.RemoveValueChanged(element, OnModifiersChanged);
      MouseWheel.SmoothingProperty.RemoveValueChanged(element, OnSmoothingChanged);
      base.OnUnloading();
    }
    protected override IMouseWheelInputListener CreateBehavior()
    {
      if (Enhanced)
      {
        switch (Smoothing)
        {
          case MouseWheelSmoothing.None:   return new MouseWheelAdaptationBehavior(this);
          case MouseWheelSmoothing.Linear: return new MouseWheelSmoothAdaptationBehavior(this, new MouseWheelLinearFilter());
          case MouseWheelSmoothing.Smooth: return new MouseWheelSmoothAdaptationBehavior(this, new MouseWheelSmoothingFilter());
          default: throw new NotSupportedException();
        }
      }
      else
        return new MouseWheelNativeBehavior(this);
    }
    #endregion

    #region IMouseWheelClient
    public override double MotionIncrement
    {
      get
      {
        return MouseWheel.GetIncrement(Controller.Element);
      }
    }
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
      var element = Controller.Element;
      var value = MouseWheel.GetValue(element);
      return info.Direction < 0 ? value > _minimum : value < _maximum;
    }
    public override double Coerce(IMotionInfo info, object context, double delta)
    {
      var element = Controller.Element;
      var value = MouseWheel.GetValue(element);
      int direction = info.Direction;
      if (direction > 0)
      {
        var movableDelta = _maximum - value;
        return Math.Min(movableDelta, delta);
      }
      else
      {
        var movableDelta = _minimum - value;
        return Math.Max(movableDelta, delta);
      }
    }
    public override void Move(IMotionInfo info, object context, double delta)
    {
      var element = Controller.Element;
      MouseWheel.SetValue(element, MouseWheel.GetValue(element) + delta);
    }
    #endregion

    #region Helpers
    private void OnSmoothingChanged(object sender, EventArgs e) { Smoothing  = MouseWheel.GetSmoothing(sender as DependencyObject); }
    private void OnModifiersChanged(object sender, EventArgs e) { _modifiers = MouseWheel.GetModifiers(sender as DependencyObject); }
    private void   OnMinimumChanged(object sender, EventArgs e) { _minimum   = MouseWheel.GetMinimum  (sender as DependencyObject); }
    private void   OnMaximumChanged(object sender, EventArgs e) { _maximum   = MouseWheel.GetMaximum  (sender as DependencyObject); }
    #endregion
  }
}
