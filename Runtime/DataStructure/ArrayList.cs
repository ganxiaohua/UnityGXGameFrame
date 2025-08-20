using System;

namespace GameFrame.Runtime
{
    public class ArrayList<T>
    {
        public T[] Data { get; private set; }

        public int Count { get; private set; }

        public T this[int index]
        {
            get
            {
                if (index >= (uint) Count)
                    throw new IndexOutOfRangeException();
                return Data[index];
            }
            set
            {
                if (index >= (uint) Count)
                    EnsureSize(Count * 2);
                Data[index] = value;
            }
        }

        public ArrayList(int capacity = 0)
        {
            Data = new T[capacity == 0 ? 4 : capacity];
        }

        public int Add(T item)
        {
            if (Count == Data.Length)
                SetCapacity(Data.Length * 2);
            Data[Count] = item;
            return Count++;
        }

        public bool Remove(T item)
        {
            var index = Array.IndexOf(Data, item, 0, Count);
            if (index < 0)
                return false;
            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if ((uint) index >= (uint) Count)
                throw new IndexOutOfRangeException();
            Count--;
            if (index < Count)
                Array.Copy(Data, index + 1, Data, index, Count - index);
            Data[Count] = default;
        }

        public bool RemoveSwapBack(T item)
        {
            var index = Array.IndexOf(Data, item, 0, Count);
            if (index < 0)
                return false;
            RemoveAtSwapBack(index);
            return true;
        }

        public void RemoveAtSwapBack(int index)
        {
            var tail = Count - 1;
            if (index != tail)
                Data[index] = Data[tail];
            Data[--Count] = default;
        }

        public bool Contains(T item)
        {
            return Array.IndexOf(Data, item, 0, Count) >= 0;
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(Data, item, 0, Count);
        }

        public T Peek()
        {
            return Data[Count - 1];
        }

        public bool TryPeek(out T item)
        {
            if (Count > 0)
            {
                item = Data[Count - 1];
                return true;
            }

            item = default;
            return false;
        }

        public T Pop()
        {
            var item = Data[--Count];
            Data[Count] = default;
            return item;
        }

        public bool TryPop(out T item)
        {
            if (Count > 0)
            {
                item = Data[--Count];
                Data[Count] = default;
                return true;
            }

            item = default;
            return false;
        }

        public void EnsureSize(int size, T defaultValue = default)
        {
            if (Data.Length < size)
                SetCapacity(size);
            for (var i = Count; i < size; i++)
                Add(defaultValue);
        }

        public void Resize(int size, T defaultValue = default)
        {
            var data = Data;
            Array.Resize(ref data, size);
            Data = data;
            for (var i = Count; i < size; i++)
                data[i] = defaultValue;
            Count = size;
        }

        public void Trim()
        {
            Resize(Count);
        }

        public void Clear(bool clearMemory = true)
        {
            if (Count > 0)
            {
                if (clearMemory)
                    Array.Clear(Data, 0, Count);
                Count = 0;
            }
        }

        private void SetCapacity(int size)
        {
            if (Count > size)
                throw new ArgumentOutOfRangeException();
            if (Data.Length == size)
                return;
            var newArray = new T[size];
            if (Count > 0)
                Array.Copy(Data, 0, newArray, 0, Count);
            Data = newArray;
        }
    }
}