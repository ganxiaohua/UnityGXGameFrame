using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class ObjectDatas<T> : SingletonInit<ObjectDatas<T>>, IDisposable, IDatasBase where T : class
    {
        private List<T> arrayList = new List<T>(64);
        private Queue<int> arrayDatasListIndex = new Queue<int>(64);

        public void OnInitialize()
        {
            ContainerDatasManager.Instance.Add(this);
        }

        public int AddData(T obj)
        {
            var succ = arrayDatasListIndex.TryDequeue(out var index);
            if (succ)
            {
                SetData(index, obj);
                return index;
            }
            else
            {
                arrayList.Add(obj);
                return arrayList.Count - 1;
            }
        }

        public T GetData(int index)
        {
            return arrayList[index];
        }

        public void SetData(int index, T data)
        {
            arrayList[index] = data;
        }

        public void RemoveData(int index)
        {
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