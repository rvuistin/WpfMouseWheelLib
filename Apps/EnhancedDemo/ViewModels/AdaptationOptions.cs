using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lada.ComponentModel;
using Lada.Windows.Input;
using System.ComponentModel;

namespace Lada.WpfMouseWheel.ViewModels
{
  public class AdaptationOptions : ObservableObject
  {
    #region Fields
    private MouseWheelSmoothing _smoothing;
    private MouseWheelDebouncing _debouncing;
    private bool _nestedMotion;
    #endregion

    #region Initialization
    public AdaptationOptions(MouseWheelOptions parent)
    {
      Parent = parent;
      parent.PropertyChanged += OnParentPropertyChanged;
      _smoothing = GetDefaultSmoothing();
      _debouncing = GetDefaultDebouncing();
      _nestedMotion = GetDefaultNestedMotion();
    }
    #endregion

    #region Queries
    #endregion

    #region Properties
    public MouseWheelOptions Parent { get; private set; }
    public bool            Enhanced { get { return Parent.Enhanced; } }

    public MouseWheelSmoothing Smoothing
    {
      get { return _smoothing; }
      set
      {
        if (_smoothing == value) return;
        _smoothing = value;
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
    public bool NestedMotion
    {
      get { return _nestedMotion; }
      set
      {
        if (_nestedMotion == value) return;
        _nestedMotion = value;
        OnPropertyChanged("NestedMotion");
      }
    }
    #endregion

    #region Helpers
    private MouseWheelSmoothing GetDefaultSmoothing()
    {
      return (MouseWheelSmoothing)MouseWheel.SmoothingProperty.DefaultMetadata.DefaultValue;
    }
    private MouseWheelDebouncing GetDefaultDebouncing()
    {
      return (MouseWheelDebouncing)MouseWheel.DebouncingProperty.DefaultMetadata.DefaultValue;
    }
    private bool GetDefaultNestedMotion()
    {
      return (bool)MouseWheel.NestedMotionProperty.DefaultMetadata.DefaultValue;
    }
    void OnParentPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Enhanced")
        OnPropertyChanged("Enhanced");
    }
    #endregion
  }
}
