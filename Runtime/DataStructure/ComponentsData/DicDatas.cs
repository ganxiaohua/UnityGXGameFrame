using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class DicDatas<K, T> : Singleton<DicDatas<K, T>>, DatasBase
    {
        private List<Dictionary<K, T>> intArrayList = new List<Dictionary<K, T>>(64);
        private Queue<int> arrayDatasListIndex = new Queue<int>(64);

        public void OnInitialize()
        {
            ContainerDatasManager.Instance.Add(this);
        }

        public int AddArrayDatas(int size = 64)
        {
            Dictionary<K, T> t = DictPool<K, T>.Get(size);
            t.Clear();
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

        public Dictionary<K, T> GetArrayDatas(int index)
        {
            return intArrayList[index];
        }

        public void RemoveDatas(int index)
        {
            DictPool<K, T>.Release(intArrayList[index]);
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