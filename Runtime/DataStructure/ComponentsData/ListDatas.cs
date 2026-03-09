using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class ListDatas<T> : SingletonInit<ListDatas<T>>, IDatasBase
    {
        private List<List<T>> intArrayList = new List<List<T>>(64);
        private Queue<int> arrayDatasListIndex = new Queue<int>(64);

        public void OnInitialize()
        {
            ContainerDatasManager.Instance.Add(this);
        }

        public int AddArrayDatas(int size = 64)
        {
            List<T> t = ListPool<T>.Get(size);
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

        public List<T> GetArrayDatas(int index)
        {
            return intArrayList[index];
        }

        public void RemoveDatas(int index)
        {
            ListPool<T>.Release(intArrayList[index]);
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