using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Lada.ComponentModel;
using System.Text;

namespace Lada.WpfMouseWheel.ViewModels
{
  #region TestItem
  public class TestItem
  {
    public TestItem(int index, string value, int fontSize, int repeatCount = 1)
    {
      Index = index;
      FontSize = fontSize;
      Value = repeatCount == 1 ? value : string.Join(" ", Enumerable.Repeat(value, repeatCount));
    }

    public override string ToString() { return string.Format("[{0}] {1}", Index, Value); }

    public int    Index { get; private set; }
    public string Value { get; private set; }
    public int FontSize { get; private set; }
  }
  #endregion

  #region TestCollectionBuilder
  public class TestCollectionBuilder : ObservableObject
  {
    #region Fields
    private ObservableCollection<TestItem> _items = new ObservableCollection<TestItem>();
    private Random _itemHeightGenerator = new Random();
    #endregion

    #region Initialization
    public TestCollectionBuilder(int count, int minFontSize, int maxFontSize, int itemPartsCount)
    {
      MinFontSize = minFontSize;
      MaxFontSize = maxFontSize;
      ItemPartsCount = itemPartsCount;
      Count = count;
    }
    #endregion

    #region Queries
    public IEnumerable<TestItem> Items { get { return _items; } }
    #endregion

    #region Properties
    public int Count
    {
      get { return _items.Count; }
      set
      {
        if (Count == value) return;
        if (value > Count)
          Add(value - Count);
        else
          Remove(Count - value);
        OnPropertyChanged("Count");
      }
    }
    public int    MinFontSize { get; private set; }
    public int    MaxFontSize { get; private set; }
    public int ItemPartsCount { get; private set; }
    #endregion

    #region Helpers
    private void Add(int count)
    {
      for (int i = 0; i < count; ++i)
        _items.Add(CreateItem(Count));
    }
    private void Remove(int count)
    {
      for (int i = 0; i < count; ++i)
        _items.RemoveAt(_items.Count - 1);
    }
    private TestItem CreateItem(int index)
    {
      return new TestItem(index, "Test item", _itemHeightGenerator.Next(MinFontSize, MaxFontSize + 1), ItemPartsCount);
    }
    #endregion
  }
  #endregion
}
