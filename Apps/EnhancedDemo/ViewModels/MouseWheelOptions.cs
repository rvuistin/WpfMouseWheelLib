using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lada.ComponentModel;
using Lada.Windows.Input;
using System.Collections.ObjectModel;

namespace Lada.WpfMouseWheel.ViewModels
{
  public class MouseWheelOptions : ObservableObject
  {
    #region Fields
    private readonly ObservableCollection<MouseWheel> _mouseWheels = new ObservableCollection<MouseWheel>();
    private readonly ScrollOptions _scrollOptions;
    private readonly ZoomOptions _zoomOptions;
    private readonly AdaptationOptions _adaptationOptions;
    private bool _enhanced = true;
    #endregion

    #region Initialization
    public MouseWheelOptions()
    {
      MouseWheel.NewWheel += (sender, e) => _mouseWheels.Add(sender as MouseWheel);
      _scrollOptions = new ScrollOptions(this);
      _zoomOptions = new ZoomOptions(this);
      _adaptationOptions = new AdaptationOptions(this);
    }
    #endregion

    #region Queries
    public IEnumerable<MouseWheel> MouseWheels { get { return _mouseWheels; } }
    public ScrollOptions         ScrollOptions { get { return _scrollOptions; } }
    public ZoomOptions             ZoomOptions { get { return _zoomOptions; } }
    public AdaptationOptions AdaptationOptions { get { return _adaptationOptions; } }

    public bool Enhanced
    {
      get { return _enhanced; }
      set
      {
        if (_enhanced == value) return;
        _enhanced = value;
        OnPropertyChanged("Enhanced");
      }
    }
    #endregion
  }
}
