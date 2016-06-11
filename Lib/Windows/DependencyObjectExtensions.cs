using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Lada.Windows
{
    public static class DependencyObjectExtensions
    {
        public static DependencyObject              GetLogicalRoot(this DependencyObject self)
        {
            var parent = LogicalTreeHelper.GetParent (self);
            while (parent != null)
            {
                self = parent;
                parent = LogicalTreeHelper.GetParent (self);
            }
            return self;
        }
        public static IEnumerable<DependencyObject> GetLogicalAncestors(this DependencyObject self)
        {
            var parent = LogicalTreeHelper.GetParent (self);
            while (parent != null)
            {
                yield return parent;
                parent = LogicalTreeHelper.GetParent (parent);
            }
        }
        public static IEnumerable                   GetLogicalChildren(this DependencyObject self)
        {
            return LogicalTreeHelper.GetChildren (self);
        }
        public static IEnumerable                   GetLogicalDescendants(this DependencyObject self)
        {
            foreach (var child in LogicalTreeHelper.GetChildren (self))
            {
                if (child is DependencyObject)
                {
                    foreach (var item in (child as DependencyObject).GetLogicalTree ())
                    {
                        yield return item;
                    }
                }
                else
                {
                    yield return child;
                }
            }
        }
        public static IEnumerable                   GetLogicalTree(this DependencyObject self)
        {
            yield return self;
            foreach (object item in self.GetLogicalDescendants ())
            {
                yield return item;
            }
        }

        public static DependencyObject              GetVisualRoot(this DependencyObject self)
        {
            if (self != null)
            {
                var parent = VisualTreeHelper.GetParent (self);
                while (parent != null)
                {
                    self = parent;
                    parent = VisualTreeHelper.GetParent (self);
                }
            }
            return self;
        }
        public static IEnumerable<DependencyObject> GetVisualAncestors(this DependencyObject self)
        {
            if (self != null)
            {
                var parent = VisualTreeHelper.GetParent (self);
                while (parent != null)
                {
                    yield return parent;
                    parent = VisualTreeHelper.GetParent (parent);
                }
            }
        }
        public static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject self)
        {
            if (self is Visual || self is Visual3D)
            {
                int count = VisualTreeHelper.GetChildrenCount (self);
                for (int i = 0; i < count; i++)
                {
                    yield return VisualTreeHelper.GetChild (self, i);
                }
            }
        }
        public static IEnumerable<DependencyObject> GetVisualDescendants(this DependencyObject self)
        {
            return self == null
                ? Enumerable.Empty<DependencyObject> ()
                : self.GetVisualChildren ().SelectMany (child => child.GetVisualTree ());
        }
        public static IEnumerable<DependencyObject> GetVisualTree(this DependencyObject self)
        {
            if (self != null)
            {
                yield return self;
                foreach (var item in self.GetVisualDescendants ())
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<BindingExpression> EnumerateBindings(this DependencyObject self)
        {
            var enumerator = self.GetLocalValueEnumerator ();
            while (enumerator.MoveNext ())
            {
                var entry = enumerator.Current;
                if (BindingOperations.IsDataBound (self, entry.Property))
                {
                    yield return entry.Value as BindingExpression;
                }
            }
        }
    }
}
