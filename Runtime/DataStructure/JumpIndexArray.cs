using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameFrame.Runtime
{
    /// <summary>
    /// 仅支持无序遍历，删除时使用末尾索引填补空位，不改变元素的外部 ID。
    /// 遍历期间只支持删除当前元素，不支持删除其他元素、新增元素或清空集合。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class JumpIndexArray<T> : IEnumerable<T>, IEnumerable, IDisposable
    {
        protected T[] Items;
        protected bool[] RealItemIndex;
        private int[] indexPositions;

        /// <summary>
        /// 当前有效的外部 ID，顺序不固定。仅供读取，请通过 Set、Remove、Clear 修改集合。
        /// </summary>
        public List<int> IndexList { get; private set; }

        public int Count => IndexList.Count;

        public int AllCount => Items.Length;

        public bool isInit => IndexList != null;


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
            else
            {
                Array.Clear(Items, 0, Items.Length);
            }

            IndexList = new List<int>(arrayMaxCount);
            RealItemIndex = new bool[arrayMaxCount];
            indexPositions = new int[arrayMaxCount];
        }

        public T Set(int index, T t)
        {
            Expansion(index);
            if (!RealItemIndex[index])
            {
                indexPositions[index] = IndexList.Count;
                IndexList.Add(index);
            }
            RealItemIndex[index] = true;
            Items[index] = t;
            return t;
        }

        protected void Expansion(int index)
        {
            if (index < Items.Length) return;
            EnsureCapacity(Math.Max(Items.Length * 2, index + 1));
        }

        public void EnsureCapacity(int capacity)
        {
            if (capacity <= Items.Length) return;
            var newArray = new T[capacity];
            var newRealItemIndex = new bool[capacity];
            var newIndexPositions = new int[capacity];
            Array.Copy(Items, 0, newArray, 0, Items.Length);
            Array.Copy(RealItemIndex, 0, newRealItemIndex, 0, RealItemIndex.Length);
            Array.Copy(indexPositions, 0, newIndexPositions, 0, indexPositions.Length);
            Items = newArray;
            RealItemIndex = newRealItemIndex;
            indexPositions = newIndexPositions;
            Debugger.LogWarning($"{typeof(T).Name} GXArray Expansion!!!");
        }

        public T Remove(int index)
        {
            if (!RealItemIndex[index])
                return default;

            var t = Items[index];
            int position = indexPositions[index];
            int lastPosition = IndexList.Count - 1;
            if (position != lastPosition)
            {
                int lastIndex = IndexList[lastPosition];
                IndexList[position] = lastIndex;
                indexPositions[lastIndex] = position;
            }

            IndexList.RemoveAt(lastPosition);
            RealItemIndex[index] = false;
            indexPositions[index] = 0;
            Items[index] = default;
            return t;
        }

        public virtual void Clear()
        {
            if (!isInit)
                return;
            foreach (int index in IndexList)
            {
                Items[index] = default;
                RealItemIndex[index] = false;
                indexPositions[index] = 0;
            }
            IndexList.Clear();
        }

        public bool Contains(int index)
        {
            return RealItemIndex[index];
        }

        public virtual void Dispose()
        {
            IndexList.Clear();
            IndexList = null;
            Items = null;
            RealItemIndex = null;
            indexPositions = null;
        }

        public Enumerator GetEnumerator() => new Enumerator(IndexList, this);

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
            private readonly JumpIndexArray<T> owner;
            private int cur;

            internal Enumerator(List<int> indexs, JumpIndexArray<T> owner)
            {
                this.owner = owner;
                this.indexs = indexs ?? throw new Exception("Linked list is invalid.");
                cur = indexs.Count;
            }

            public T Current
            {
                get { return owner.Items[indexs[cur]]; }
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
