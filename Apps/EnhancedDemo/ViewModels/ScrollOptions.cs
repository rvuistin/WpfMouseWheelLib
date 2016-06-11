using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Lada.ComponentModel;

namespace Lada.WpfMouseWheel.ViewModels
{
  public class ScrollOptions : ObservableObject
  {
    #region Fields
    private readonly ScrollOrientationOptions _optionsY;
    private readonly ScrollOrientationOptions _optionsX;
    #endregion

    #region Initialization
    public ScrollOptions(MouseWheelOptions parent)
    {
      Parent = parent;
      _optionsY = new ScrollOrientationOptions(this, Orientation.Vertical);
      _optionsX = new ScrollOrientationOptions(this, Orientation.Horizontal);
      parent.PropertyChanged += OnParentPropertyChanged;
    }
    #endregion

    #region Queries
    public ScrollOrientationOptions Y { get { return _optionsY; } }
    public ScrollOrientationOptions X { get { return _optionsX; } }
    #endregion

    #region Properties
    public MouseWheelOptions Parent { get; private set; }
    public bool            Enhanced { get { return Parent.Enhanced; } }
    #endregion

    #region Helpers
    void OnParentPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Enhanced")
        OnPropertyChanged("Enhanced");
    }
    #endregion
  }
}
