using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Lada.Diagnostics
{
  public class TimeBase
  {
    #region Constants
    public static readonly TimeBase Current = new TimeBase();
    #endregion

    #region Fields
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    #endregion

    #region Queries
    public TimeSpan Elapsed { get { return _stopwatch.Elapsed; } }
    #endregion

    #region Methods
    public void Start() { }
    #endregion
  }
}
