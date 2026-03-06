using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class ArrayDatas<T> : SingletonInit<ArrayDatas<T>>, DatasBase
    {
        private List<T[]> intArrayList = new List<T[]>(64);
        private Queue<int> arrayDatasListIndex = new Queue<int>(64);

        public void OnInitialize()
        {
            ContainerDatasManager.Instance.Add(this);
        }

        public int AddArrayDatas(int size)
        {
            T[] t = new T[size];
            var succ = arrayDatasListIndex.TryDequeue(out var index);
            if (succ)
            {
                intArrayList[index] = t;
                return index;
            }
            else
            {
                intArrayList.Add(t);
                return intArrayList.Count - 1;
            }
        }

        public T[] GetArrayDatas(int index)
        {
            return intArrayList[index];
        }

        public void RemoveDatas(int index)
        {
            intArrayList[index] = null;
            arrayDatasListIndex.Enqueue(index);
        }

        public void Dispose()
        {
            intArrayList.Clear();
            arrayDatasListIndex.Clear();
        }
    }
}