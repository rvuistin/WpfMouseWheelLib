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
using Lada.ComponentModel;
using Lada.Maths;
using Lada.Windows.MotionFlow;

namespace Lada.Windows.Input
{
  #region MouseWheelScrollBehavior
  public abstract class MouseWheelScrollBehavior : MouseWheelBehavior
  {
    #region Fields
    private MouseWheelScrollIncrement _scrollIncrement;
    private MouseWheelDebouncing _debouncing;
    #endregion

    #region Initialization
    public MouseWheelScrollBehavior(MouseWheelScrollClient scrollClient, IDisposable manipulator)
      : base(scrollClient, manipulator)
    {
      var element = Client.Controller.Element;
      if (scrollClient.Orientation == Orientation.Vertical)
      {
        NestedMotionEnabled = MouseWheel.GetNestedVScroll(element);
        MouseWheel.NestedVScrollProperty.AddValueChanged(element, OnNestedVScrollChanged);
      }
      else
      {
        NestedMotionEnabled = MouseWheel.GetNestedHScroll(element);
        MouseWheel.NestedHScrollProperty.AddValueChanged(element, OnNestedHScrollChanged);
      }
    }
    #endregion

    #region IDisposable
    public override void Dispose()
    {
      var element = Client.Controller.Element;
      if (ScrollClient.Orientation == Orientation.Vertical)
        MouseWheel.NestedVScrollProperty.RemoveValueChanged(element, OnNestedVScrollChanged);
      else
        MouseWheel.NestedHScrollProperty.RemoveValueChanged(element, OnNestedHScrollChanged);
      if (_scrollIncrement != null)
        _scrollIncrement.PropertyChanged -= OnScrollIncrementPropertyChanged;
      base.Dispose();
    }
    #endregion

    #region MouseWheelBehavior
    protected override double MotionIncrement { get { return ScrollIncrement; } }
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

    #region Queries
    public MouseWheelScrollClient ScrollClient { get { return Client as MouseWheelScrollClient; } }
    #endregion

    #region Properties
    protected MouseWheelScrollIncrement ScrollIncrement
    {
      get { return _scrollIncrement; }
      set
      {
        if (_scrollIncrement == value) return;
        if (_scrollIncrement != null)
          _scrollIncrement.PropertyChanged -= OnScrollIncrementPropertyChanged;
        _scrollIncrement = value;
        _scrollIncrement.PropertyChanged += OnScrollIncrementPropertyChanged;
        InvalidateShaft();
      }
    }
    #endregion

    #region Helpers
    private void OnNestedVScrollChanged(object sender, EventArgs e) { NestedMotionEnabled = MouseWheel.GetNestedVScroll(sender as DependencyObject); }
    private void OnNestedHScrollChanged(object sender, EventArgs e) { NestedMotionEnabled = MouseWheel.GetNestedHScroll(sender as DependencyObject); }
    private void OnScrollIncrementPropertyChanged(object sender, PropertyChangedEventArgs e) { InvalidateShaft(); }
    #endregion
  }
  #endregion

  #region MouseWheelLogicalScrollBehavior
  public class MouseWheelLogicalScrollBehavior : MouseWheelScrollBehavior
  {
    #region Initialization
    public MouseWheelLogicalScrollBehavior(MouseWheelScrollClient scrollClient, IDisposable manipulator)
      : base(scrollClient, manipulator)
    {
      var element = Client.Controller.Element;
      if (scrollClient.Orientation == Orientation.Vertical)
      {
        Debouncing    = MouseWheel.GetLogicalVScrollDebouncing(element);
        ScrollIncrement = MouseWheel.GetLogicalVScrollIncrement(element);
        MouseWheel.LogicalVScrollDebouncingProperty.AddValueChanged(element, OnDebouncingYChanged);
        MouseWheel.LogicalVScrollIncrementProperty.AddValueChanged(element, OnVScrollIncrementChanged);
      }
      else
      {
        Debouncing    = MouseWheel.GetLogicalHScrollDebouncing(element);
        ScrollIncrement = MouseWheel.GetLogicalHScrollIncrement(element);
        MouseWheel.LogicalHScrollDebouncingProperty.AddValueChanged(element, OnDebouncingXChanged);
        MouseWheel.LogicalHScrollIncrementProperty.AddValueChanged(element, OnHScrollIncrementChanged);
      }
    }
    #endregion

    #region IDisposable
    public override void Dispose()
    {
      var element = Client.Controller.Element;
      if (ScrollClient.Orientation == Orientation.Vertical)
      {
        MouseWheel.LogicalVScrollDebouncingProperty.RemoveValueChanged(element, OnDebouncingYChanged);
        MouseWheel.LogicalVScrollIncrementProperty.RemoveValueChanged(element, OnVScrollIncrementChanged);
      }
      else
      {
        MouseWheel.LogicalHScrollDebouncingProperty.RemoveValueChanged(element, OnDebouncingXChanged);
        MouseWheel.LogicalHScrollIncrementProperty.RemoveValueChanged(element, OnHScrollIncrementChanged);
      }
      base.Dispose();
    } 
    #endregion

    #region MouseWheelBehavior
    protected override IMouseWheelShaft GetMotionShaft(MouseWheel wheel, IMouseWheelTransferCase transferCase)
    {
      switch (Debouncing)
      {
        case MouseWheelDebouncing.Auto:   return GetMotionShaftAuto(wheel, transferCase, ScrollIncrement);
        case MouseWheelDebouncing.None:   return transferCase[0];
        case MouseWheelDebouncing.Single: return transferCase[1];
        default: throw new NotImplementedException();
      }
    }
    protected override double CoerceSinkDelta(double sinkDelta) { return Math.Round(sinkDelta); }
    #endregion

    #region Helpers
    private void    OnDebouncingYChanged(object sender, EventArgs e) { Debouncing    = MouseWheel.GetLogicalVScrollDebouncing(sender as DependencyObject); }
    private void    OnDebouncingXChanged(object sender, EventArgs e) { Debouncing    = MouseWheel.GetLogicalHScrollDebouncing(sender as DependencyObject); }
    private void OnVScrollIncrementChanged(object sender, EventArgs e) { ScrollIncrement = MouseWheel.GetLogicalVScrollIncrement(sender as DependencyObject); }
    private void OnHScrollIncrementChanged(object sender, EventArgs e) { ScrollIncrement = MouseWheel.GetLogicalHScrollIncrement(sender as DependencyObject); }
    #endregion
  } 
  #endregion

  #region MouseWheelFlowDocumentPageViewerScrollBehavior
  public class MouseWheelFlowDocumentPageViewerScrollBehavior : MouseWheelBehavior
  {
    #region Fields
    private MouseWheelDebouncing _debouncing;
    #endregion

    #region Initialization
    public MouseWheelFlowDocumentPageViewerScrollBehavior(IMouseWheelClient client)
      : base(client, null)
    {
      var element = Client.Controller.Element;
      NestedMotionEnabled = MouseWheel.GetNestedVScroll(element);
      Debouncing = MouseWheel.GetLogicalVScrollDebouncing(element);
      MouseWheel.NestedVScrollProperty.AddValueChanged(element, OnNestedVScrollChanged);
      MouseWheel.LogicalVScrollDebouncingProperty.AddValueChanged(element, OnDebouncingYChanged);
    }
    #endregion

    #region IDisposable
    public override void Dispose()
    {
      var element = Client.Controller.Element;
      MouseWheel.NestedVScrollProperty.RemoveValueChanged(element, OnNestedVScrollChanged);
      MouseWheel.LogicalVScrollDebouncingProperty.RemoveValueChanged(element, OnDebouncingYChanged);
      base.Dispose();
    }
    #endregion

    #region MouseWheelBehavior
    protected override double CoerceSinkDelta(double sinkDelta) { return Math.Round(sinkDelta); }
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
    private void OnNestedVScrollChanged(object sender, EventArgs e) { NestedMotionEnabled = MouseWheel.GetNestedVScroll(sender as DependencyObject); }
    private void   OnDebouncingYChanged(object sender, EventArgs e) { Debouncing = MouseWheel.GetLogicalVScrollDebouncing(sender as DependencyObject); }
    #endregion
  }
  #endregion

  #region MouseWheelPhysicalScrollBehavior
  public class MouseWheelPhysicalScrollBehavior : MouseWheelScrollBehavior
  {
    #region Initialization
    public MouseWheelPhysicalScrollBehavior(MouseWheelScrollClient scrollClient, IDisposable manipulator)
      : base(scrollClient, manipulator)
    {
      var element = Client.Controller.Element;
      if (scrollClient.Orientation == Orientation.Vertical)
      {
        Debouncing = MouseWheel.GetPhysicalVScrollDebouncing(element);
        ScrollIncrement = MouseWheel.GetPhysicalVScrollIncrement(element);
        MouseWheel.PhysicalVScrollDebouncingProperty.AddValueChanged(element, OnDebouncingYChanged);
        MouseWheel.PhysicalVScrollIncrementProperty.AddValueChanged(element, OnVScrollIncrementChanged);
      }
      else
      {
        Debouncing = MouseWheel.GetPhysicalHScrollDebouncing(element);
        ScrollIncrement = MouseWheel.GetPhysicalHScrollIncrement(element);
        MouseWheel.PhysicalHScrollDebouncingProperty.AddValueChanged(element, OnDebouncingXChanged);
        MouseWheel.PhysicalHScrollIncrementProperty.AddValueChanged(element, OnHScrollIncrementChanged);
      }
    }
    #endregion

    #region IDisposable
    public override void Dispose()
    {
      var element = Client.Controller.Element;
      if (ScrollClient.Orientation == Orientation.Vertical)
      {
        MouseWheel.PhysicalVScrollDebouncingProperty.RemoveValueChanged(element, OnDebouncingYChanged);
        MouseWheel.PhysicalVScrollIncrementProperty.RemoveValueChanged(element, OnVScrollIncrementChanged);
      }
      else
      {
        MouseWheel.PhysicalHScrollDebouncingProperty.RemoveValueChanged(element, OnDebouncingXChanged);
        MouseWheel.PhysicalHScrollIncrementProperty.RemoveValueChanged(element, OnHScrollIncrementChanged);
      }
      base.Dispose();
    }
    #endregion

    #region Helpers
    private void    OnDebouncingYChanged(object sender, EventArgs e) { Debouncing    = MouseWheel.GetPhysicalVScrollDebouncing(sender as DependencyObject); }
    private void    OnDebouncingXChanged(object sender, EventArgs e) { Debouncing    = MouseWheel.GetPhysicalHScrollDebouncing(sender as DependencyObject); }
    private void OnVScrollIncrementChanged(object sender, EventArgs e) { ScrollIncrement = MouseWheel.GetPhysicalVScrollIncrement(sender as DependencyObject); }
    private void OnHScrollIncrementChanged(object sender, EventArgs e) { ScrollIncrement = MouseWheel.GetPhysicalHScrollIncrement(sender as DependencyObject); }
    #endregion
  } 
  #endregion

  #region MouseWheelSmoothScrollBehavior
  public class MouseWheelSmoothScrollBehavior : MouseWheelPhysicalScrollBehavior
  {
    #region Fields
    private readonly MotionSmoothingTarget _motionSmoothing;
    #endregion

    #region Initialization
    public MouseWheelSmoothScrollBehavior(MouseWheelScrollClient scrollClient, IDisposable manipulator, IMotionFilter motionFilter)
      : base(scrollClient, manipulator)
    {
      _motionSmoothing = new MotionSmoothingTarget(motionFilter) { Next = this, Precision = SinkToNormalized(0.1) };
    }
    #endregion

    #region IDisposable
    public override void Dispose()
    {
      _motionSmoothing.Dispose();
      base.Dispose();
    }
    #endregion

    #region IMouseWheelInputListener
    protected override IMotionTarget MotionInput { get { return _motionSmoothing; } }
    #endregion
  }
  #endregion
}
