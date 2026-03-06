using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class ReferenceDatas<T> : SingletonInit<ReferenceDatas<T>>, DatasBase where T : class, IDisposable
    {
        private List<T> arrayList = new List<T>(64);
        private Queue<int> arrayDatasListIndex = new Queue<int>(64);

        public void OnInitialize()
        {
            ContainerDatasManager.Instance.Add(this);
        }

        public int AddArrayDatas(int size)
        {
            T t = ReferencePool.Acquire<T>();
            var succ = arrayDatasListIndex.TryDequeue(out var index);
            if (succ)
            {
                arrayList[index] = t;
                return index;
            }
            else
            {
                arrayList.Add(t);
                return arrayList.Count - 1;
            }
        }

        public T GetArrayDatas(int index)
        {
            return arrayList[index];
        }

        public void RemoveDatas(int index)
        {
            ReferencePool.Release(arrayList[index]);
            arrayList[index] = null;
            arrayDatasListIndex.Enqueue(index);
        }

        public void Dispose()
        {
            arrayList.Clear();
            arrayDatasListIndex.Clear();
        }
    }
}