using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameFrame.Runtime
{
    /// <summary>
    /// 遍历没有顺序要求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IndexHashSet<T> : IEnumerable<T>, IEnumerable where T : IContinuousID
    {
        private ArrayEx<bool> blocks;

        private List<T> datas;

        private int capacity;

        public int Count => datas.Count;

        public IndexHashSet(int capacity)
        {
            datas = new List<T>(capacity);
            blocks = new(capacity, true);
            SetCapacity(capacity);
        }

        public void SetCapacity(int capacity)
        {
            datas.Capacity = capacity;
            blocks.SetCapacity(capacity);
        }

        public bool Add(T data)
        {
            bool have = blocks[data.ID];
            if (!have)
            {
                datas.Add(data);
                blocks[data.ID] = true;
            }

            return !have;
        }

        public bool Remove(T data)
        {
            bool have = blocks[data.ID];
            if (have)
            {
                datas.RemoveSwapBack(data);
                blocks[data.ID] = false;
            }

            return have;
        }


        public void Clear()
        {
            blocks.Clear();
            datas.Clear();
        }

        public Enumerator GetEnumerator() => new Enumerator(datas);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.datas.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private List<T> Item;
            private int cur;

            internal Enumerator(List<T> item)
            {
                Item = item;
                cur = item.Count;
            }

            public T Current
            {
                get { return Item[cur]; }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                cur = Item.Count;
            }

            public bool MoveNext()
            {
                return --cur >= 0;
            }

            void IEnumerator.Reset()
            {
                cur = Item.Count;
            }
        }
    }
}