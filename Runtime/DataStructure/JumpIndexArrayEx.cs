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
            return Set(index, t);
        }

        public new bool Remove(int index)
        {
            if (Items[index] == null)
            {
                return false;
            }

            // 释放回调可能通过外部 ID 访问自身数据，必须在回调完成后移除槽位。
            ReferencePool.Release(Items[index]);
            base.Remove(index);
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
