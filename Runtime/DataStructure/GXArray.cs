using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace GameFrame
{
    public class GXArray<T> : IDisposable where T : class, IDisposable
    {
        private T[] items;

        public List<int> IndexList { get; private set; }

        public T this[int index]
        {
            get
            {
                if (index >= items.Length || index < 0)
                {
                    throw new Exception($"ThrowArgumentOutOfRange {index}");
                }

                return items[index];
            }
        }

        public void Init(int arrayMaxCount)
        {
            if (items == null || items.Length != arrayMaxCount)
            {
                items = new T[arrayMaxCount];
            }

            IndexList = new List<int>(arrayMaxCount);
            IndexList.Clear();
        }

        public Q Add<Q>(int index) where Q : T
        {
            return (Q)Add(index, typeof(Q));
        }

        public T Add(int index, Type type)
        {
            if (index >= items.Length)
            {
                var newArray = new T[items.Length * (index / items.Length + 1)];
                Array.Copy(items, 0, newArray, 0, items.Length);
                items = newArray;
                Debugger.LogWarning($"{type.Name} GXArray Expansion!!!");
            }

            if (items[index] != null)
            {
                return items[index];
            }

            var t = (T) ReferencePool.Acquire(type);
            IndexList.Add(index);
            items[index] = t;
            return t;
        }

        public bool Remove(int index)
        {
            if (items[index] == null)
            {
                return false;
            }

            ReferencePool.Release(items[index]);
            items[index] = null;
            IndexList.RemoveSwapBack(index);
            return true;
        }

        public bool Contains(int index)
        {
            if (index >= items.Length || index < 0)
                return false;
            return items[index] != null;
        }

        public void Dispose()
        {
            foreach (var index in IndexList)
            {
                ReferencePool.Release(items[index]);
                items[index] = default(T);
            }

            IndexList.Clear();
        }

        public GXEnumerator GetEnumerator() => new GXEnumerator(IndexList, items);

        [StructLayout(LayoutKind.Auto)]
        public struct GXEnumerator : IEnumerator<T>
        {
            private List<int>.Enumerator indexEnumerator;
            private T[] items;

            internal GXEnumerator(List<int> indexs, T[] items)
            {
                if (indexs == null)
                {
                    throw new Exception("Linked list is invalid.");
                }

                this.items = items;
                indexEnumerator = indexs.GetEnumerator();
            }

            public T Current
            {
                get { return items[indexEnumerator.Current]; }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                indexEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                return indexEnumerator.MoveNext();
            }

            void IEnumerator.Reset()
            {
                ((IEnumerator<int>) indexEnumerator).Reset();
            }
        }
    }
}