using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class MarkDieList<T>
    {
        public List<T> Data;
        public bool[] DataMark;
        private List<int> waitRemoveIndex;
        public int count => Data.Count;

        public MarkDieList(int cap = 16)
        {
            Data = new List<T>(cap);
            DataMark = new bool[cap];
            waitRemoveIndex = new List<int>(cap);
        }

        public void Add(T t)
        {
            Data.Add(t);
            SetCapacity(Data.Count);
            DataMark[Data.Count - 1] = true;
        }

        public bool Contains(T t)
        {
            return Data.Contains(t);
        }

        public void MarkRemove(T t)
        {
            var index = Data.IndexOf(t);
            MarkRemoveAt(index);
        }

        public void MarkRemoveAt(int index)
        {
            if (index == -1)
                return;
            DataMark[index] = false;
            waitRemoveIndex.Add(index);
            waitRemoveIndex.Sort((p1, p2) => p2.CompareTo(p1));
        }

        public void Remove()
        {
            int cou = waitRemoveIndex.Count;
            if (cou == 0)
                return;
            for (int i = 0; i < cou; i++)
            {
                int index = waitRemoveIndex[i];
                Data.RemoveAt(index);
                Array.Copy(DataMark, index + 1, DataMark, index, DataMark.Length - index - 1);
            }

            waitRemoveIndex.Clear();
        }

        private void SetCapacity(int size)
        {
            if (DataMark.Length >= size)
                return;
            var newArray = new bool[size];
            Array.Copy(DataMark, 0, newArray, 0, DataMark.Length);
            DataMark = newArray;
        }

        public void Clear()
        {
            Data = null;
            DataMark = null;
            waitRemoveIndex = null;
        }
    }
}