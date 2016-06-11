using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lada
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, Func<TKey, TValue> factory)
        {
            bool created;
            return self.GetOrAdd (key, factory, out created);
        }
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, Func<TKey, TValue> factory, out bool created)
        {
            created = false;
            TValue value;
            if (!self.TryGetValue (key, out value))
            {
                value = factory (key);
                if (!object.Equals (value, default (TValue)))
                {
                    created = true;
                    self[key] = value;
                }
            }
            return value;
        }
    }
}
