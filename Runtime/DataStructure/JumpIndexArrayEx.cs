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
            Expansion(index);

            if (RealItemIndex[index])
            {
                return Items[index];
            }

            var t = (T) ReferencePool.Acquire(type);
            IndexList.Add(index);
            Items[index] = t;
            RealItemIndex[index] = true;
            return t;
        }

        public new bool Remove(int index)
        {
            if (Items[index] == null)
            {
                return false;
            }

            ReferencePool.Release(Items[index]);
            IndexList.Remove(index);
            Items[index] = null;
            RealItemIndex[index] = false;
            return true;
        }

        public override void Dispose()
        {
            foreach (var index in IndexList)
            {
                ReferencePool.Release(Items[index]);
                Items[index] = null;
            }

            base.Dispose();
        }
    }
}