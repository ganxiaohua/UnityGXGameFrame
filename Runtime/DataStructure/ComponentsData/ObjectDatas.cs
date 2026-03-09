using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class ObjectDatas : SingletonInit<ObjectDatas>, IDisposable, IDatasBase
    {
        private List<object> arrayList = new List<object>(64);
        private Queue<int> arrayDatasListIndex = new Queue<int>(64);

        public void OnInitialize()
        {
            ContainerDatasManager.Instance.Add(this);
        }

        public int AddData(object obj)
        {
            var succ = arrayDatasListIndex.TryDequeue(out var index);
            if (succ)
            {
                arrayList[index] = obj;
                return index;
            }
            else
            {
                arrayList.Add(string.Empty);
                return arrayList.Count - 1;
            }
        }

        public object GetData(int index)
        {
            return arrayList[index];
        }

        public void SetData(int index, string data)
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