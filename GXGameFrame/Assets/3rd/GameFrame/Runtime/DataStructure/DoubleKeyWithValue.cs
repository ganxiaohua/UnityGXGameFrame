using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class DoubleKeyWithValue<T, K, V> : IEquatable<DoubleKeyWithValue<T, K, V>>
    {
        private T t;

        private K k;

        private List<V> vlist;

        public void Init(T t, K k)
        {
            this.t = t;
            this.k = k;
            vlist = new List<V>();
        }

        public bool Equals(DoubleKeyWithValue<T, K, V> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<T>.Default.Equals(t, other.t) && EqualityComparer<K>.Default.Equals(k, other.k);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DoubleKeyWithValue<T, K, V>) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(t, k, vlist);
        }
        
        // public void 
        
    }
}