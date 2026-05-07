using System;

namespace GameFrame.Runtime
{
    public struct GOAPState : IEquatable<GOAPState>
    {
        /// <summary>
        /// 一共32*2个预设位，如若不足自己添加，或者整合成ulong 在甲一组ulong
        /// </summary>
        /// ------------
        private uint bits1;

        private uint bits2;

        // 为1表示这一位有意义（被设置过）
        private uint care1;
        private uint care2;

        private const uint BitsSize1 = sizeof(uint) * 8;
        private const uint BitsSize2 = BitsSize1 * 2;
        public const uint BitsSizeMax = BitsSize2;

        public static readonly GOAPState Empty = new(0, 0, 0, 0);

        public GOAPState(uint low, uint high, uint careLow = 0, uint careHigh = 0)
        {
            bits1 = low;
            bits2 = high;
            care1 = careLow;
            care2 = careHigh;
        }

        public void Set(int index, bool value)
        {
            if (index < 0 || index > BitsSizeMax)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (index < BitsSize1)
            {
                care1 |= 1U << index;
                if (value)
                    bits1 |= 1U << index;
                else
                    bits1 &= ~(1U << index);
            }
            else
            {
                int idx = index - (int) BitsSize1;
                care2 |= 1U << idx;
                if (value)
                    bits2 |= 1U << idx;
                else
                    bits2 &= ~(1U << idx);
            }
        }

        public bool Get(int index)
        {
            if (index < 0 || index > BitsSizeMax)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (index < BitsSize1)
                return (bits1 & (1U << index)) != 0;
            else
                return (bits2 & (1U << (index - (int) BitsSize1))) != 0;
        }

        public bool GetCare(int index)
        {
            if (index < 0 || index > BitsSizeMax)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (index < BitsSize1)
                return (care1 & (1U << index)) != 0;
            else
                return (care2 & (1U << (index - (int) BitsSize1))) != 0;
        }

        public bool Satisfies(GOAPState goal)
        {
            uint relevant1 = goal.care1;
            uint relevant2 = goal.care2;
            return ((bits1 & relevant1) == (goal.bits1 & relevant1)) &&
                   ((bits2 & relevant2) == (goal.bits2 & relevant2));
        }

        public void Apply(GOAPState effects)
        {
            bits1 = (bits1 & ~effects.care1) | (effects.bits1 & effects.care1);
            bits2 = (bits2 & ~effects.care2) | (effects.bits2 & effects.care2);
        }


        public bool HasAll(GOAPState preconditions)
        {
            return Satisfies(preconditions);
        }

        public bool Equals(GOAPState other)
        {
            return bits1 == other.bits1 && bits2 == other.bits2;
        }

        public override bool Equals(object obj)
        {
            return obj is GOAPState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(bits1, bits2);
        }

        public static bool operator ==(GOAPState left, GOAPState right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GOAPState left, GOAPState right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return
                $"WS(L:{Convert.ToString((long) bits1, 2).PadLeft(64, '0')}, H:{Convert.ToString((long) bits2, 2).PadLeft(64, '0')}, CareL:{Convert.ToString((long) care1, 2).PadLeft(64, '0')}, CareH:{Convert.ToString((long) care2, 2).PadLeft(64, '0')})";
        }
    }
}