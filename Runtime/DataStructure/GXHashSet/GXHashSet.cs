using System.Collections;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    /// <summary>
    /// TODO: SHOULD OPT IT
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GXHashSet<T> : IEnumerable<T>,IEnumerable
    {
        private HashSet<T> blocks;

        private StrongList<T> datas;

        private int capacity;

        public int Count => datas.Count;

        public GXHashSet(int capacity)
        {
            datas = new StrongList<T>(capacity);
            blocks = new HashSet<T>(1);
            SetCapacity(capacity);
        }

        public void SetCapacity(int capacity)
        {
            datas.SetCapacity(capacity);
            blocks.EnsureCapacity(capacity);
        }

        public bool Add(T data)
        {
            bool succ = blocks.Add(data);
            if (succ)
                datas.Add(data);

            return false;
        }

        public bool Remove(T data)
        {
            var succ = blocks.Remove(data);
            if (succ)
            {
                datas.Remove(data);
                return true;
            }

            return false;
        }


        public void Clear()
        {
            blocks.Clear();
            datas.Clear();
        }

        public IEnumerator<T> GetEnumerator() => this.datas.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}