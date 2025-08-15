using System.Collections;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public sealed partial class LoopList<T, T_Data> : IEnumerable<T>
    {
        public struct Enumerator : IEnumerator<T>
        {
            public T Current => items[index];

            object IEnumerator.Current => items[index];

            private T[] items;
            private int itemSize;
            private int index;

            public Enumerator(T[] items, int size)
            {
                this.items = items;
                this.itemSize = size;
                this.index = -1;
            }

            public bool MoveNext()
            {
                index++;
                while (index < itemSize && !items[index].Visible)
                    index++;
                return index < itemSize;
            }

            public void Reset()
            {
                index = -1;
            }

            public void Dispose()
            {
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(items, itemSize);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(items, itemSize);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(items, itemSize);
        }
    }
}