using System;

namespace GameFrame.Runtime
{
    public class JumpIndexArrayEx<T> : JumpIndexArray<T> where T : class, IDisposable
    {
        public Q Add<Q>(int index) where Q : T
        {
            return (Q) Add(index, typeof(Q));
        }

        public T Add(int index, Type type)
        {
            if (index >= Items.Length)
            {
                var newArray = new T[Items.Length * (index / Items.Length + 1)];
                Array.Copy(Items, 0, newArray, 0, Items.Length);
                Items = newArray;
                Debugger.LogWarning($"{type.Name} GXArray Expansion!!!");
            }

            if (Items[index] != null)
            {
                return Items[index];
            }

            var t = (T) ReferencePool.Acquire(type);
            IndexList.Add(index);
            Items[index] = t;
            return t;
        }

        public new bool Remove(int index)
        {
            if (Items[index] == null)
            {
                return false;
            }

            ReferencePool.Release(Items[index]);
            IndexList.RemoveSwapBack(index);
            Items[index] = null;
            return true;
        }

        public override void Dispose()
        {
            foreach (var index in IndexList)
            {
                ReferencePool.Release(Items[index]);
                Items[index] = default(T);
            }
        }
    }
}