using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GameFrame.Runtime
{
    public partial class JumpIndexArray<T> : IEnumerable<T>, IEnumerable, IDisposable
    {
        protected T[] Items;
        protected bool[] RealItemIndex;
        public List<int> IndexList { get; private set; }

        public int Count => IndexList.Count;

        public int AllCount => Items.Length;

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
            if (Items == null || Items.Length != arrayMaxCount)
            {
                Items = new T[arrayMaxCount];
            }

            IndexList = new List<int>(arrayMaxCount);
            RealItemIndex = new bool[arrayMaxCount];
        }

        public T Set(int index, T t)
        {
            Expansion(index);
            if (!RealItemIndex[index])
                IndexList.Add(index);
            RealItemIndex[index] = true;
            Items[index] = t;
            return t;
        }

        protected void Expansion(int index)
        {
            if (index < Items.Length) return;
            int newSize = Items.Length * (index / Items.Length + 1);
            var newArray = new T[newSize];
            var newRealItemIndex = new bool[newSize];
            Array.Copy(Items, 0, newArray, 0, Items.Length);
            Array.Copy(RealItemIndex, 0, newRealItemIndex, 0, RealItemIndex.Length);
            Items = newArray;
            RealItemIndex = newRealItemIndex;
            Debugger.LogWarning($"{typeof(T).Name} GXArray Expansion!!!");
        }

        public T Remove(int index)
        {
            IndexList.Remove(index);
            var t = Items[index];
            RealItemIndex[index] = false;
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

            count = RealItemIndex.Length;
            for (int i = 0; i < count; i++)
            {
                RealItemIndex[i] = false;
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
            Items = null;
            RealItemIndex = null;
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