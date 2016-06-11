using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Lada.ComponentModel;
using Lada.Windows.Input;

namespace Lada.WpfMouseWheel.ViewModels
{
  public class ScrollIncrementSelector : ObservableObject
  {
    #region Constants
    private static readonly MouseWheelScrollIncrement  StarLogicalVScrollIncrement = MouseWheelScrollIncrement.StarLogicalDefaultV.Clone();
    private static readonly MouseWheelScrollIncrement StarPhysicalVScrollIncrement = MouseWheelScrollIncrement.StarPhysicalDefaultV.Clone();
    private static readonly MouseWheelScrollIncrement      LogicalVScrollIncrement = MouseWheelScrollIncrement.LogicalDefaultV.Clone();
    private static readonly MouseWheelScrollIncrement     PhysicalVScrollIncrement = MouseWheelScrollIncrement.PhysicalDefaultV.Clone();

    private static readonly MouseWheelScrollIncrement  StarLogicalHScrollIncrement = MouseWheelScrollIncrement.StarLogicalDefaultH.Clone();
    private static readonly MouseWheelScrollIncrement StarPhysicalHScrollIncrement = MouseWheelScrollIncrement.StarPhysicalDefaultH.Clone();
    private static readonly MouseWheelScrollIncrement      LogicalHScrollIncrement = MouseWheelScrollIncrement.LogicalDefaultH.Clone();
    private static readonly MouseWheelScrollIncrement     PhysicalHScrollIncrement = MouseWheelScrollIncrement.PhysicalDefaultH.Clone();
    #endregion

    #region Fields
    private readonly ReadOnlyCollection<ScrollIncrementItem> _items;
    private MouseWheelScrollIncrement _selectedItem;
    private RelayCommand _launchMouseWheelAppletCommand;
    private int _busyCount;
    #endregion

    #region Initialization
    public ScrollIncrementSelector(ScrollModeOptions parent)
    {
      Parent = parent;
      _items = new ReadOnlyCollection<ScrollIncrementItem>(new ScrollIncrementItem[] {
        GetDefaultScrollIncrementItem(ScrollIncrementScaleType.Star),
        GetDefaultScrollIncrementItem(ScrollIncrementScaleType.Unity)}.ToList());

      SelectedItem = GetDefaultScrollIncrement(ScrollIncrementScaleType.Star);
    }
    #endregion

    #region Object
    public override string ToString() { return Orientation.ToString(); }
    #endregion

    #region Queries
    public IList<ScrollIncrementItem> Items { get { return _items; } }

    public ICommand LaunchMouseWheelAppletCommand
    {
      get
      {
        return _launchMouseWheelAppletCommand ?? (_launchMouseWheelAppletCommand = new RelayCommand(LaunchMouseWheelApplet));
      }
    }
    #endregion

    #region Properties
    public ScrollModeOptions Parent { get; private set; }
    public Orientation  Orientation { get { return Parent.Orientation; } }
    public bool           IsLogical { get { return Parent.IsLogical; } }

    public MouseWheelScrollIncrement SelectedItem
    {
      get { return _selectedItem; }
      set
      {
        if (_selectedItem == value) return;
        _selectedItem = value;
        OnPropertyChanged("SelectedItem");
        foreach (var item in _items)
          item.IsSelected = Equals(value, item.Value);
      }
    }
    public bool IsBusy
    {
      get { return _busyCount > 0; }
      set
      {
        bool wasBusy = IsBusy;
        _busyCount += value ? 1 : -1;
        if (wasBusy != IsBusy)
          OnPropertyChanged("IsBusy");
      }
    }
    #endregion

    #region Helpers
    private MouseWheelScrollIncrement GetDefaultScrollIncrement(ScrollIncrementScaleType scaleType)
    {
      if (Orientation == Orientation.Vertical)
      {
        if (IsLogical)
          return scaleType == ScrollIncrementScaleType.Star ? StarLogicalVScrollIncrement : LogicalVScrollIncrement;
        else
          return scaleType == ScrollIncrementScaleType.Star ? StarPhysicalVScrollIncrement : PhysicalVScrollIncrement;
      }
      else
      {
        if (IsLogical)
          return scaleType == ScrollIncrementScaleType.Star ? StarLogicalHScrollIncrement : LogicalHScrollIncrement;
        else
          return scaleType == ScrollIncrementScaleType.Star ? StarPhysicalHScrollIncrement : PhysicalHScrollIncrement;
      }
    }
    private ScrollIncrementItem GetDefaultScrollIncrementItem(ScrollIncrementScaleType scaleType)
    {
      return new ScrollIncrementItem(this, GetDefaultScrollIncrement(scaleType));
    }
    private void LaunchMouseWheelApplet()
    {
      try
      {
        IsBusy = true;
        Process.Start(new ProcessStartInfo("main.cpl", "@0,3") { UseShellExecute = true });
      }
      finally
      {
        IsBusy = false;
      }
    }
    #endregion
  }
}
