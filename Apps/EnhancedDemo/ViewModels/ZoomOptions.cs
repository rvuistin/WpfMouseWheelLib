using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Lada.ComponentModel;
using Lada.Windows.Input;

namespace Lada.WpfMouseWheel.ViewModels
{
  public class ZoomOptions : ObservableObject
  {
    #region Fields
    private MouseWheelSmoothing _motionSmoothing;
    private MouseWheelDebouncing _debouncing;
    private bool _nestedZoom;
    #endregion

    #region Initialization
    public ZoomOptions(MouseWheelOptions parent)
    {
      Parent = parent;
      parent.PropertyChanged += OnParentPropertyChanged;
      _motionSmoothing = GetDefaultSmoothing();
      _debouncing = GetDefaultDebouncing();
      _nestedZoom = GetDefaultNestedZoom();
    }
    #endregion

    #region Queries
    #endregion

    #region Properties
    public MouseWheelOptions Parent { get; private set; }
    public bool            Enhanced { get { return Parent.Enhanced; } }

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
    public MouseWheelDebouncing Debouncing
    {
      get { return _debouncing; }
      set
      {
        if (_debouncing == value) return;
        _debouncing = value;
        OnPropertyChanged("Debouncing");
      }
    }
    public bool NestedZoom
    {
      get { return _nestedZoom; }
      set
      {
        if (_nestedZoom == value) return;
        _nestedZoom = value;
        OnPropertyChanged("NestedZoom");
      }
    }
    #endregion

    #region Helpers
    private MouseWheelSmoothing GetDefaultSmoothing()
    {
      return (MouseWheelSmoothing)MouseWheel.ZoomSmoothingProperty.DefaultMetadata.DefaultValue;
    }
    private MouseWheelDebouncing GetDefaultDebouncing()
    {
      return (MouseWheelDebouncing)MouseWheel.ZoomDebouncingProperty.DefaultMetadata.DefaultValue;
    }
    private bool GetDefaultNestedZoom()
    {
      return (bool)MouseWheel.NestedZoomProperty.DefaultMetadata.DefaultValue;
    }
    void OnParentPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Enhanced")
        OnPropertyChanged("Enhanced");
    }
    #endregion
  }
}
