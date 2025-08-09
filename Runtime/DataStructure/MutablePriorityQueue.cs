using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public interface IMutablePriorityQueueItem
    {
        public int MutableIndex { get; set; }
    }

    public sealed class MutablePriorityQueue<T> where T : class, IMutablePriorityQueueItem
    {
        private readonly List<T> list;
        private readonly Func<T, T, int> compare;

        public int Count => list.Count;

        public T this[int index] => list[index];

        public MutablePriorityQueue(Func<T, T, int> compare, int capacity = 8)
        {
            this.list = new(capacity);
            this.compare = compare;
        }

        private void Swap(int i, int j)
        {
            var idata = list[i];
            var jdata = list[j];
            idata.MutableIndex = j;
            jdata.MutableIndex = i;
            list[i] = jdata;
            list[j] = idata;
        }

        private void ShiftUp(int index)
        {
            while (index > 0)
            {
                var parent = (index - 1) >> 1;
                if (compare(list[index], list[parent]) >= 0) break;
                Swap(index, parent);
                index = parent;
            }
        }

        private void ShiftDown(int index)
        {
            var size = list.Count;
            while (true)
            {
                var child = (index << 1) + 1;
                if (child >= size) break;
                if (child + 1 < size && compare(list[child + 1], list[child]) < 0) child++;
                if (compare(list[index], list[child]) <= 0) break;
                Swap(index, child);
                index = child;
            }
        }

        public void Enqueue(T item)
        {
            item.MutableIndex = list.Count;
            list.Add(item);
            ShiftUp(item.MutableIndex);
        }

        public T Dequeue()
        {
            var head = list[0];
            var tail = list.Pop();
            if (head != tail)
            {
                tail.MutableIndex = 0;
                list[0] = tail;
                ShiftDown(0);
            }

            return head;
        }

        public bool Contains(T item)
        {
            var index = item.MutableIndex;
            if (index >= 0 && index < list.Count && list[index] == item)
                return true;
            return false;
        }

        public bool Update(T item)
        {
            var index = item.MutableIndex;
            if (index >= 0 && index < list.Count && list[index] == item)
            {
                ShiftUp(item.MutableIndex);
                ShiftDown(item.MutableIndex);
            }

            return index != item.MutableIndex;
        }

        public bool Remove(T item)
        {
            var index = item.MutableIndex;
            if (index >= 0 && index < list.Count && list[index] == item)
            {
                while (index > 0)
                {
                    var parent = (index - 1) >> 1;
                    Swap(index, parent);
                    index = parent;
                }

                Dequeue();
                return true;
            }

            return false;
        }

        public bool TryDequeue(out T val)
        {
            if (list.Count > 0)
            {
                val = Dequeue();
                return true;
            }

            val = default;
            return false;
        }

        public bool TryPeek(out T val)
        {
            if (list.Count > 0)
            {
                val = list[0];
                return true;
            }

            val = default;
            return false;
        }

        public void Clear()
        {
            list.Clear();
        }
    }
}