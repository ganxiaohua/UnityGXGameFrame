using System.Collections;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    /// <summary>
    /// TODO: SHOULD OPT IT
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GXHashSet<T> : IEnumerable<T>, IEnumerable where T : IContinuousID
    {
        private ArrayEx<bool> blocks;

        private StrongList<T> datas;

        private int capacity;

        public int Count => datas.Count;

        public GXHashSet(int capacity)
        {
            datas = new StrongList<T>(capacity);
            blocks = new(capacity, true);
            SetCapacity(capacity);
        }

        public void SetCapacity(int capacity)
        {
            datas.SetCapacity(capacity);
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
                datas.Remove(data);
                blocks[data.ID] = false;
            }

            return have;
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