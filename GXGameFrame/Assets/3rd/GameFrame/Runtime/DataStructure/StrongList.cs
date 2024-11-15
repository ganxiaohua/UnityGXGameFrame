using System;
using System.Collections;
using System.Collections.Generic;

namespace GameFrame
{
    public class StrongList<T> : IEnumerator<T>, IEnumerable<T>
    {
        private readonly List<T> dataList;
        private int currentIndex;
        private T currentElement;

        private bool Lock => currentIndex >= 0;

        public T Current => currentElement;

        object IEnumerator.Current => currentElement;

        public IReadOnlyList<T> DataList => dataList;

        private bool keepOrder;

        public StrongList(int capacity = 0, bool keepOrder = false)
        {
            dataList = new List<T>(capacity);
            currentIndex = -1;
            currentElement = default;
            this.keepOrder = keepOrder;
        }

        public void Add(T val)
        {
            dataList.Add(val);
        }

        public bool Remove(T val)
        {
            if (Lock)
            {
                int index = dataList.IndexOf(val);
                if (index == -1)
                    return false;

                if (index > currentIndex - 1)
                {
                    if (!keepOrder)
                        dataList.RemoveAtSwapBack(index);
                    else
                    {
                        dataList.RemoveAt(index);
                    }
                }
                else if (index < currentIndex - 1)
                {
                    dataList[index] = dataList[currentIndex - 1];
                    if (!keepOrder)
                        dataList.RemoveAtSwapBack(currentIndex - 1);
                    else
                    {
                        dataList.RemoveAt(index);
                    }

                    currentIndex--;
                }
                else
                {
                    if (!keepOrder)
                        dataList.RemoveAtSwapBack(index);
                    else
                    {
                        dataList.RemoveAt(index);
                    }

                    currentIndex--;
                }

                return true;
            }
            else
            {
                if (!keepOrder)
                    return dataList.RemoveSwapBack(val);
                else
                {
                    return dataList.Remove(val);
                }
            }
        }

        public bool Contains(T val)
        {
            return dataList.Contains(val);
        }

        public bool MoveNext()
        {
            if (currentIndex == dataList.Count)
            {
                currentElement = default;
                return false;
            }
            else
            {
                currentElement = dataList[currentIndex++];
                return true;
            }
        }

        public void Reset()
        {
            if (Lock) throw new Exception($"Enumerator locked");
            currentIndex = 0;
            currentElement = default;
        }

        public void Dispose()
        {
            currentIndex = -1;
            currentElement = default;
        }

        public IEnumerator<T> GetEnumerator()
        {
            Reset();
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Reset();
            return this;
        }
    }
}