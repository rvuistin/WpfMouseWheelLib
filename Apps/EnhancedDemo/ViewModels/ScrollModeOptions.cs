using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Lada.ComponentModel;
using Lada.Windows.Input;

namespace Lada.WpfMouseWheel.ViewModels
{
  public class ScrollModeOptions : ObservableObject
  {
    #region Fields
    private readonly ScrollIncrementSelector _scrollIncrementSelector;
    private MouseWheelDebouncing _debouncing;
    #endregion

    #region Initialization
    public ScrollModeOptions(ScrollOrientationOptions parent, bool isLogical)
    {
      Parent = parent;
      IsLogical = isLogical;
      _scrollIncrementSelector = new ScrollIncrementSelector(this);
      _debouncing = GetDefaultDebouncing();
    }

    #endregion

    #region Properties
    public ScrollOrientationOptions                 Parent { get; private set; }
    public Orientation                         Orientation { get { return Parent.Orientation; } }
    public bool                                  IsLogical { get; private set; }
    public bool                                 IsPhysical { get { return !IsLogical; } }
    public ScrollIncrementSelector ScrollIncrementSelector { get { return _scrollIncrementSelector; } }

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
    #endregion

    #region Helpers
    private MouseWheelDebouncing GetDefaultDebouncing()
    {
      if (Orientation == Orientation.Vertical)
      {
        if (IsLogical)
          return (MouseWheelDebouncing)MouseWheel.LogicalVScrollDebouncingProperty.DefaultMetadata.DefaultValue;
        else
          return (MouseWheelDebouncing)MouseWheel.PhysicalVScrollDebouncingProperty.DefaultMetadata.DefaultValue;
      }
      else
      {
        if (IsLogical)
          return (MouseWheelDebouncing)MouseWheel.LogicalHScrollDebouncingProperty.DefaultMetadata.DefaultValue;
        else
          return (MouseWheelDebouncing)MouseWheel.PhysicalHScrollDebouncingProperty.DefaultMetadata.DefaultValue;
      }
    }
    #endregion
  }
}
