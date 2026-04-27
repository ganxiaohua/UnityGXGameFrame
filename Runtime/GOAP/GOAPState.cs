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

        public const uint BitsSize1 = sizeof(uint);
        public const uint BitsSize2 = BitsSize1 * 2;
        public const uint BitsSizeMax = BitsSize2;

        public static readonly GOAPState Empty = new(0, 0);

        public GOAPState(uint low, uint high)
        {
            bits1 = low;
            bits2 = high;
        }

        public void Set(int index, bool value)
        {
            if (index < 0 || index > BitsSizeMax)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (index < BitsSize1)
            {
                if (value)
                    bits1 |= 1U << index;
                else
                    bits1 &= ~(1U << index);
            }
            else
            {
                int idx = index - 64;
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
                return (bits1 & (1UL << index)) != 0;
            else
                return (bits2 & (1UL << (index - 64))) != 0;
        }

        public bool Satisfies(GOAPState goal)
        {
            return (bits1 & goal.bits1) == goal.bits1 &&
                   (bits2 & goal.bits2) == goal.bits2;
        }

        public void Apply(GOAPState effects)
        {
            bits1 |= effects.bits1;
            bits2 |= effects.bits2;
        }


        public bool HasAll(GOAPState preconditions)
        {
            return Satisfies(preconditions);
        }

        public static GOAPState operator |(GOAPState a, GOAPState b)
        {
            return new GOAPState(a.bits1 | b.bits1, a.bits2 | b.bits2);
        }

        public static GOAPState operator &(GOAPState a, GOAPState b)
        {
            return new GOAPState(a.bits1 & b.bits1, a.bits2 & b.bits2);
        }

        public static GOAPState operator ~(GOAPState a)
        {
            return new GOAPState(~a.bits1, ~a.bits2);
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
            return $"WS(L:{Convert.ToString((long) bits1, 2).PadLeft(64, '0')}, H:{Convert.ToString((long) bits2, 2).PadLeft(64, '0')})";
        }
    }
}