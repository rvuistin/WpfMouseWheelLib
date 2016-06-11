using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Lada.Windows.Controls
{
    public static class ScrollViewerExtensions
    {
        public static void Scroll(this ScrollViewer self, Orientation orientation, double scrollLength)
        {
            if (orientation == Orientation.Vertical)
            {
                self.ScrollToVerticalOffset (self.VerticalOffset + scrollLength);
            }
            else
            {
                self.ScrollToHorizontalOffset (self.HorizontalOffset + scrollLength);
            }
        }
        public static double GetScrollableDisplacement(this ScrollViewer self, Orientation orientation, int direction)
        {
            if (orientation == Orientation.Vertical)
            {
                if (!DoubleEx.IsZero (self.ScrollableHeight))
                {
                    if (direction < 0)
                    {
                        return -self.VerticalOffset;
                    }
                    else
                    {
                        return self.ExtentHeight - self.ViewportHeight - self.VerticalOffset;
                    }
                }
            }
            else if (!DoubleEx.IsZero (self.ScrollableWidth))
            {
                if (direction < 0)
                {
                    return -self.HorizontalOffset;
                }
                else
                {
                    return self.ExtentWidth - self.ViewportWidth - self.HorizontalOffset;
                }
            }
            return 0;
        }
        public static Orientation GetScrollAreaOrientation(this ScrollViewer self)
        {
            var vsp = self.GetVisualDescendants ().OfType<VirtualizingStackPanel> ().FirstOrDefault ();
            if (vsp != null)
            {
                return vsp.Orientation;
            }
            var sp = self.GetVisualDescendants ().OfType<StackPanel> ().FirstOrDefault ();
            if (sp != null)
            {
                return sp.Orientation;
            }
            return Orientation.Vertical;
        }
        public static bool HasNestedScrollFrames(this ScrollViewer scrollViewer)
        {
            var nsv = scrollViewer.GetVisualDescendants ().OfType<ScrollViewer> ().FirstOrDefault ();
            return nsv != null && nsv.TemplatedParent != scrollViewer;
        }
        public static bool ObjectIsDescendantOfNestedScrollViewer(this ScrollViewer self, DependencyObject obj)
        {
            if (obj != null && obj != self)
            {
                if (obj is Visual || obj is Visual3D)
                {
                    var scrollFrame = obj.GetVisualAncestors ().OfType<ScrollViewer> ().FirstOrDefault ();
                    if (scrollFrame != null && scrollFrame != self)
                    {
                        return true;
                    }
                }
                else
                {
                    var scrollFrame = obj.GetLogicalAncestors ().OfType<ScrollViewer> ().FirstOrDefault ();
                    if (scrollFrame != null && scrollFrame != self)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
