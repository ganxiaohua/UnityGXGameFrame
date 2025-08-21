using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameFrame.Runtime
{
    public class ArrayEx<T>
    {
        public T[] Data { get; private set; }

        public int Count => Data.Length;

        private bool selfExpansion;

        public T this[int index]
        {
            get
            {
                if (index >= (uint) Count)
                {
                    if (selfExpansion)
                        SetCapacity((index / Count + 1) * Count);
                    else
                    {
                        throw new IndexOutOfRangeException();
                    }
                }

                return Data[index];
            }
            set
            {
                if (index >= (uint) Count)
                {
                    if (selfExpansion)
                        SetCapacity((index / Count + 1) * Count);
                    else
                    {
                        throw new IndexOutOfRangeException();
                    }
                }

                Data[index] = value;
            }
        }

        public ArrayEx(int capacity = 0, bool selfExpansion = false)
        {
            Data = new T[capacity == 0 ? 4 : capacity];
            this.selfExpansion = selfExpansion;
        }


        public void Clear()
        {
            Array.Clear(Data, 0, Count);
            this.selfExpansion = false;
        }

        public void SetCapacity(int size)
        {
            if (Count > size)
                throw new ArgumentOutOfRangeException();
            if (Data.Length == size)
                return;
            var newArray = new T[size];
            if (Count > 0)
                Array.Copy(Data, 0, newArray, 0, Count);
            Data = newArray;
        }


        public GXEnumerator GetEnumerator() => new GXEnumerator(Data);

        [StructLayout(LayoutKind.Auto)]
        public struct GXEnumerator : IEnumerator<T>, IEnumerator
        {
            private T[] items;
            private int cur;
            private int max;

            internal GXEnumerator(T[] items)
            {
                if (items == null)
                {
                    throw new Exception("Linked list is invalid.");
                }

                this.items = items;
                max = this.items.Length;
                cur = -1;
            }

            public T Current
            {
                get { return items[cur]; }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                cur = -1;
            }

            public bool MoveNext()
            {
                cur++;
                return cur < max;
            }

            void IEnumerator.Reset()
            {
                cur = -1;
            }
        }
    }
}