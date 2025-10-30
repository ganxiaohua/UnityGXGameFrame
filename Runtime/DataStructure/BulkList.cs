using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class BulkListIndex : IDisposable
    {
        public int Index { get; internal set; }
        public int key { get; internal set; }

        public virtual void Dispose()
        {
            Index = 0;
            key = 0;
        }
    }

    /// <summary>
    /// 散装容器
    /// </summary>
    public class BulkList<T> where T : BulkListIndex
    {
        private T[] datas;
        public int Count { get; private set; }

        public T this[int index]
        {
            get { return datas[index]; }
            set { datas[index] = value; }
        }

        public BulkList(int count)
        {
            datas = new T[count];
            Count = count;
        }

        public bool Add(int key, T t)
        {
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i] != null) continue;
                datas[i] = t;
                t.Index = i;
                t.key = key;
                return true;
            }

            return false;
        }

        public T GetAt(int index)
        {
            return datas[index];
        }

        public void Get(int key, ref List<T> list)
        {
            list.Clear();
            for (int i = 0; i < datas.Length; i++)
            {
                var item = datas[i];
                if (item != null && datas[i].key == key)
                {
                    list.Add(datas[i]);
                }
            }
        }

        public T RemoveAt(int index)
        {
            Assert.IsTrue(index < datas.Length, $"array out {index}");
            var t = datas[index];
            datas[index] = null;
            return t;
        }
    }
}