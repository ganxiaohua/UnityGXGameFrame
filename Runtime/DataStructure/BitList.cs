using System;
using GameFrame.Runtime;

namespace GameFrame.Runtime
{
    public class BitList
    {
        private static readonly int[] Empty = new int[0];

        public int Count
        {
            get => count;
            set
            {
                int k = (value + 31) >> 5;
                int t = (count + 31) >> 5;
                int n = data.Length;
                if (k > n)
                {
                    var newData = new int[k];
                    Array.Copy(data, 0, newData, 0, n);
                    data = newData;
                }
                else if (k <= t)
                {
                    while (k < t) data[k++] = 0;
                    int i = value >> 5;
                    if (i < n)
                    {
                        int j = value & 31;
                        data[i] &= ~(-1 << j);
                    }
                }

                count = value;
            }
        }

        public bool this[int index]
        {
            get
            {
                Assert.IsTrue(index >= 0, "Index can't be negative");
                if (index >= count)
                    return false;
                int i = index >> 5;
                int j = index & 31;
                return (data[i] & (1 << j)) != 0;
            }
            set
            {
                Assert.IsTrue(index >= 0, "Index can't be negative");
                if (index >= count)
                    Count = index + 1;
                int i = index >> 5;
                int j = index & 31;
                if (value)
                    data[i] |= 1 << j;
                else
                    data[i] &= ~(1 << j);
            }
        }

        private int[] data = Empty;
        private int count;

        public BitList(int count = 0)
        {
            Count = count;
        }

        public void Push(bool value)
        {
            int index = Count;
            Count++;
            this[index] = value;
        }

        public void Pop()
        {
            Count--;
        }

        public int IndexOfNextZero(int offset)
        {
            Assert.IsTrue(offset >= 0 && offset < count, "offset out of range");
            for (int i = offset; i < count; i++)
            {
                if ((data[i >> 5] & (1 << (i & 31))) == 0)
                    return i;
            }

            for (int i = 0; i < offset; i++)
            {
                if ((data[i >> 5] & (1 << (i & 31))) == 0)
                    return i;
            }

            return -1;
        }

        public int OccupyIndexOfNextZero(int offset)
        {
            var index = IndexOfNextZero(offset);
            if (index != -1)
                this[index] = true;
            return index;
        }

        public void Clear(bool freeMemory = false)
        {
            if (freeMemory)
            {
                data = Empty;
                count = 0;
            }
            else
            {
                Count = 0;
            }
        }

        public override string ToString()
        {
            var sb = StringBuilderCache.Get();
            for (int i = 0; i < count; i++)
            {
                sb.Append(this[i] ? '1' : '0');
                if ((i + 1) % 8 == 0)
                    sb.Append(' ');
            }

            sb.TrimBack(' ');
            return StringBuilderCache.GetStringAndRelease(sb);
        }
    }
}