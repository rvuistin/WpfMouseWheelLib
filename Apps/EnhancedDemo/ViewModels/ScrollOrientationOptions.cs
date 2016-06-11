using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Lada.ComponentModel;
using Lada.Windows.Input;

namespace Lada.WpfMouseWheel.ViewModels
{
  public class ScrollOrientationOptions : ObservableObject
  {
    #region Fields
    private MouseWheelScrollMode _scrollMode;
    private MouseWheelSmoothing _motionSmoothing;
    private bool _nestedScroll;
    private readonly ScrollModeOptions _logicalOptions;
    private readonly ScrollModeOptions _physicalOptions;
    #endregion

    #region Initialization
    public ScrollOrientationOptions(ScrollOptions parent, Orientation orientation)
    {
      Parent = parent;
      Parent.PropertyChanged += OnParentPropertyChanged;
      Orientation = orientation;
      _scrollMode = GetDefaultScrollMode();
      _motionSmoothing = GetDefaultSmoothing();
      _nestedScroll = GetDefaultNestedScroll();
      _logicalOptions = new ScrollModeOptions(this, true);
      _physicalOptions = new ScrollModeOptions(this, false);
    }
    #endregion

    #region Object
    public override string ToString() { return Orientation.ToString(); }
    #endregion

    #region Properties
    public bool           Enhanced { get { return Parent.Enhanced; } }
    public Orientation Orientation { get; private set; }
    public ScrollOptions    Parent { get; private set; }

    public MouseWheelScrollMode ScrollMode
    {
      get { return _scrollMode; }
      set
      {
        if (_scrollMode == value) return;
        _scrollMode = value;
        OnPropertyChanged("ScrollMode");
      }
    }
    public MouseWheelSmoothing Smoothing
    {
      get { return _motionSmoothing; }
      set
      {
        if (_motionSmoothing == value) return;
        _motionSmoothing = value;
        OnPropertyChanged("Smoothing");
      }
    }
    public bool NestedScroll
    {
      get { return _nestedScroll; }
      set
      {
        if (_nestedScroll == value) return;
        _nestedScroll = value;
        OnPropertyChanged("NestedScroll");
      }
    }
    public ScrollModeOptions  Logical { get { return _logicalOptions; } }
    public ScrollModeOptions Physical { get { return _physicalOptions; } }
    #endregion

    #region Helpers
    private MouseWheelScrollMode GetDefaultScrollMode()
    {
      if (Orientation == Orientation.Vertical)
        return (MouseWheelScrollMode)MouseWheel.VScrollModeProperty.DefaultMetadata.DefaultValue;
      else
        return (MouseWheelScrollMode)MouseWheel.HScrollModeProperty.DefaultMetadata.DefaultValue;
    }
    private MouseWheelSmoothing GetDefaultSmoothing()
    {
      if (Orientation == Orientation.Vertical)
        return (MouseWheelSmoothing)MouseWheel.VScrollSmoothingProperty.DefaultMetadata.DefaultValue;
      else
        return (MouseWheelSmoothing)MouseWheel.HScrollSmoothingProperty.DefaultMetadata.DefaultValue;
    }
    private bool GetDefaultNestedScroll()
    {
      if (Orientation == Orientation.Vertical)
        return (bool)MouseWheel.NestedVScrollProperty.DefaultMetadata.DefaultValue;
      else
        return (bool)MouseWheel.NestedHScrollProperty.DefaultMetadata.DefaultValue;
    }
    private void OnParentPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Enhanced")
        OnPropertyChanged("Enhanced");
    }
    #endregion
  }
}
