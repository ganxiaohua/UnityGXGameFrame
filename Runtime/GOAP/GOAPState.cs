using System;

namespace GameFrame.Runtime
{
    public struct GOAPState : IEquatable<GOAPState>
    {
        /// <summary>
        /// 一共64个预设位。
        /// </summary>
        private ulong bits;

        // 为1表示这一位有意义（被设置过）
        private ulong care;

        public const uint BitsSizeMax = sizeof(ulong) * 8;

        public static readonly GOAPState Empty = new(0UL, 0UL);

        public GOAPState(ulong bits, ulong care = 0)
        {
            this.bits = bits;
            this.care = care;
        }

        public void Set(int index, bool value)
        {
            ValidateIndex(index);

            ulong mask = 1UL << index;
            care |= mask;
            if (value)
                bits |= mask;
            else
                bits &= ~mask;
        }

        public bool Get(int index)
        {
            ValidateIndex(index);

            return (bits & (1UL << index)) != 0;
        }

        public bool GetCare(int index)
        {
            ValidateIndex(index);

            return (care & (1UL << index)) != 0;
        }

        public bool Satisfies(GOAPState goal)
        {
            ulong relevant = goal.care;
            return (bits & relevant) == (goal.bits & relevant);
        }

        public void Apply(GOAPState effects)
        {
            bits = (bits & ~effects.care) | (effects.bits & effects.care);
        }


        public bool HasAll(GOAPState preconditions)
        {
            return Satisfies(preconditions);
        }

        public bool Equals(GOAPState other)
        {
            return bits == other.bits;
        }

        public override bool Equals(object obj)
        {
            return obj is GOAPState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return bits.GetHashCode();
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
            return $"WS(Bits:{ToBinaryString(bits)}, Care:{ToBinaryString(care)})";
        }


        private static void ValidateIndex(int index)
        {
            if ((uint) index >= BitsSizeMax)
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        private static string ToBinaryString(ulong value)
        {
            return Convert.ToString(unchecked((long) value), 2).PadLeft((int) BitsSizeMax, '0');
        }
    }
}