using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameFrame.Runtime
{
    public class JumpIndexArray<T> : IEnumerable<T>, IEnumerable, IDisposable
    {
        protected T[] Items;

        public List<int> IndexList { get; private set; }

        public int Count => IndexList.Count;

        public bool isInit => IndexList != null;

        private bool isClass;

        public T this[int index]
        {
            get
            {
#if UNITY_EDITOR
                if (index >= Items.Length || index < 0)
                {
                    throw new Exception($"ThrowArgumentOutOfRange {index}");
                }
#endif
                return Items[index];
            }
        }

        public void Init(int arrayMaxCount)
        {
            isClass = typeof(T).IsClass;
            if (Items == null || Items.Length != arrayMaxCount)
            {
                Items = new T[arrayMaxCount];
            }

            IndexList = new List<int>(arrayMaxCount);
        }

        public T Set(int index, T t)
        {
            if (index >= Items.Length)
            {
                var newArray = new T[Items.Length * (index / Items.Length + 1)];
                Array.Copy(Items, 0, newArray, 0, Items.Length);
                Items = newArray;
                Debugger.LogWarning($"{typeof(T).Name} GXArray Expansion!!!");
            }

            if (isClass)
            {
                if (Items[index] == null)
                    IndexList.Add(index);
            }
            else
            {
                if (!IndexList.Contains(index))
                    IndexList.Add(index);
            }

            Items[index] = t;
            return t;
        }

        public T Remove(int index)
        {
            IndexList.RemoveSwapBack(index);
            var t = Items[index];
            Items[index] = default;
            return t;
        }

        public void Clear()
        {
            if (!isInit)
                return;
            int count = IndexList.Count;
            for (int i = 0; i < count; i++)
            {
                Items[IndexList[i]] = default(T);
            }

            IndexList.Clear();
        }

        public bool Contains(int index)
        {
            if (index >= Items.Length || index < 0)
                return false;
            return Items[index] != null;
        }

        public virtual void Dispose()
        {
            IndexList.Clear();
            IndexList = null;
        }

        public Enumerator GetEnumerator() => new Enumerator(IndexList, Items);

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private List<int> indexs;
            private T[] items;
            private int cur;

            internal Enumerator(List<int> indexs, T[] items)
            {
                this.items = items;
                this.indexs = indexs ?? throw new Exception("Linked list is invalid.");
                cur = indexs.Count;
            }

            public T Current
            {
                get { return items[indexs[cur]]; }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                cur = indexs.Count;
            }

            public bool MoveNext()
            {
                return --cur >= 0;
            }

            void IEnumerator.Reset()
            {
                cur = indexs.Count;
            }
        }
    }
}