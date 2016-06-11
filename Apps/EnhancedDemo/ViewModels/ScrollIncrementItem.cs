using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lada.ComponentModel;
using Lada.Windows.Input;

namespace Lada.WpfMouseWheel.ViewModels
{
  public class ScrollIncrementItem : ObservableObject
  {
    #region Constants
    public static readonly ScrollIncrementItem Empty = new ScrollIncrementItem(null, null);
    #endregion

    #region Fields
    private bool _isSelected;
    #endregion

    #region Initialization
    public ScrollIncrementItem(ScrollIncrementSelector parent, MouseWheelScrollIncrement value)
    {
      Parent = parent;
      Value = value;
    }
    #endregion  

    #region Properties
    public ScrollIncrementSelector  Parent { get; private set; }
    public MouseWheelScrollIncrement Value { get; private set; }
    public bool                  IsLogical { get { return Parent.IsLogical; } }

    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        if (_isSelected == value) return;
        _isSelected = value;
        if (value)
          Parent.SelectedItem = Value;
        OnPropertyChanged("IsSelected");
      }
    }
    #endregion
  }
}
