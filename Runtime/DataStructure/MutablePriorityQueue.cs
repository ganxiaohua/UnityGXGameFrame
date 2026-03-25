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
        private readonly IComparer<T> comparer;

        public int Count => list.Count;

        public T this[int index] => list[index];


        public MutablePriorityQueue(IComparer<T> comparer, int capacity = 8)
        {
            this.list = new(capacity);
            this.comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        private int Compare(T a, T b)
        {
            return comparer.Compare(a, b);
        }

        private void Swap(int i, int j)
        {
            var heap = list;
            var idata = heap[i];
            var jdata = heap[j];
            idata.MutableIndex = j;
            jdata.MutableIndex = i;
            heap[i] = jdata;
            heap[j] = idata;
        }

        private void ShiftUp(int index)
        {
            var heap = list;
            var item = heap[index];
            while (index > 0)
            {
                var parent = (index - 1) >> 1;
                if (Compare(item, heap[parent]) >= 0) break;

                var parentItem = heap[parent];
                parentItem.MutableIndex = index;
                heap[index] = parentItem;

                index = parent;
            }

            item.MutableIndex = index;
            heap[index] = item;
        }

        private void ShiftDown(int index)
        {
            var heap = list;
            var size = heap.Count;
            var item = heap[index];

            while (true)
            {
                var child = (index << 1) + 1;
                if (child >= size) break;

                var right = child + 1;
                if (right < size && Compare(heap[right], heap[child]) < 0)
                    child = right;

                if (Compare(item, heap[child]) <= 0) break;

                var childItem = heap[child];
                childItem.MutableIndex = index;
                heap[index] = childItem;

                index = child;
            }

            item.MutableIndex = index;
            heap[index] = item;
        }

        public void Enqueue(T item)
        {
            item.MutableIndex = list.Count;
            list.Add(item);
            ShiftUp(item.MutableIndex);
        }

        public T Dequeue()
        {
            var heap = list;
            var head = heap[0];
            head.MutableIndex = -1;

            var tail = heap.Pop();
            if (heap.Count > 0)
            {
                tail.MutableIndex = 0;
                heap[0] = tail;
                ShiftDown(0);
            }

            return head;
        }

        public bool Contains(T item)
        {
            var index = item.MutableIndex;
            return index >= 0 && index < list.Count && list[index] == item;
        }

        public bool Update(T item)
        {
            var index = item.MutableIndex;
            if (index < 0 || index >= list.Count || list[index] != item)
                return false;

            // 记录原索引用于判断是否真的移动了
            var oldIndex = index;
            ShiftUp(index);
            if (item.MutableIndex == oldIndex)
            {
                ShiftDown(index);
            }

            return item.MutableIndex != oldIndex;
        }

        public bool Remove(T item)
        {
            var index = item.MutableIndex;
            if (index < 0 || index >= list.Count || list[index] != item)
                return false;

            item.MutableIndex = -1;

            var last = list.Pop();
            if (index < list.Count)
            {
                last.MutableIndex = index;
                list[index] = last;

                // 判断需要向上还是向下调整
                if (index > 0 && Compare(last, list[(index - 1) >> 1]) < 0)
                    ShiftUp(index);
                else
                    ShiftDown(index);
            }

            return true;
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
            foreach (var item in list)
            {
                item.MutableIndex = -1;
            }

            list.Clear();
        }
    }
}