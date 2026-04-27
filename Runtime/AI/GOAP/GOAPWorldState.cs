using System;
using System.Collections.Generic;

namespace UnityGXGameFrame.AI.GOAP
{
    /// <summary>
    /// GOAP世界状态 - 使用位掩码高效存储布尔状态
    /// </summary>
    [Serializable]
    public struct GOAPWorldState : IEquatable<GOAPWorldState>
    {
        // 使用两个ulong可以存储128个布尔状态
        // 对于大多数游戏AI已经足够了
        private ulong _bitsLow;
        private ulong _bitsHigh;

        public static readonly GOAPWorldState Empty = new(0, 0);

        public GOAPWorldState(ulong low, ulong high)
        {
            _bitsLow = low;
            _bitsHigh = high;
        }

        /// <summary>
        /// 设置指定索引的状态（0-127）
        /// </summary>
        public void Set(int index, bool value)
        {
            if (index < 0 || index > 127)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (index < 64)
            {
                if (value)
                    _bitsLow |= 1UL << index;
                else
                    _bitsLow &= ~(1UL << index);
            }
            else
            {
                int idx = index - 64;
                if (value)
                    _bitsHigh |= 1UL << idx;
                else
                    _bitsHigh &= ~(1UL << idx);
            }
        }

        /// <summary>
        /// 获取指定索引的状态
        /// </summary>
        public bool Get(int index)
        {
            if (index < 0 || index > 127)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (index < 64)
                return (_bitsLow & (1UL << index)) != 0;
            else
                return (_bitsHigh & (1UL << (index - 64))) != 0;
        }

        /// <summary>
        /// 检查当前状态是否满足目标状态的所有要求
        /// （目标中为true的位，在当前状态中必须为true）
        /// </summary>
        public bool Satisfies(GOAPWorldState goal)
        {
            // goal中为1的位，this中也必须为1
            return (_bitsLow & goal._bitsLow) == goal._bitsLow &&
                   (_bitsHigh & goal._bitsHigh) == goal._bitsHigh;
        }

        /// <summary>
        /// 应用效果到当前状态（按位或）
        /// </summary>
        public void Apply(GOAPWorldState effects)
        {
            _bitsLow |= effects._bitsLow;
            _bitsHigh |= effects._bitsHigh;
        }

        /// <summary>
        /// 检查当前状态是否满足给定条件（前提）
        /// </summary>
        public bool HasAll(GOAPWorldState preconditions)
        {
            return Satisfies(preconditions);
        }

        public static GOAPWorldState operator |(GOAPWorldState a, GOAPWorldState b)
        {
            return new GOAPWorldState(a._bitsLow | b._bitsLow, a._bitsHigh | b._bitsHigh);
        }

        public static GOAPWorldState operator &(GOAPWorldState a, GOAPWorldState b)
        {
            return new GOAPWorldState(a._bitsLow & b._bitsLow, a._bitsHigh & b._bitsHigh);
        }

        public static GOAPWorldState operator ~(GOAPWorldState a)
        {
            return new GOAPWorldState(~a._bitsLow, ~a._bitsHigh);
        }

        public bool Equals(GOAPWorldState other)
        {
            return _bitsLow == other._bitsLow && _bitsHigh == other._bitsHigh;
        }

        public override bool Equals(object obj)
        {
            return obj is GOAPWorldState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_bitsLow, _bitsHigh);
        }

        public static bool operator ==(GOAPWorldState left, GOAPWorldState right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GOAPWorldState left, GOAPWorldState right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"WS(L:{Convert.ToString((long) _bitsLow, 2).PadLeft(64, '0')}, H:{Convert.ToString((long) _bitsHigh, 2).PadLeft(64, '0')})";
        }
    }

    /// <summary>
    /// 世界状态定义管理器（运行时单例或静态引用）
    /// </summary>
    public static class GOAPWorldStateRegistry
    {
        private static readonly Dictionary<string, int> _nameToIndex = new();
        private static readonly Dictionary<int, string> _indexToName = new();
        private static int _nextIndex = 0;
        private const int MaxStates = 128;

        /// <summary>
        /// 注册一个世界状态名称，返回其位索引
        /// </summary>
        public static int Register(string stateName)
        {
            if (_nameToIndex.TryGetValue(stateName, out var existingIndex))
                return existingIndex;

            if (_nextIndex >= MaxStates)
                throw new InvalidOperationException($"无法注册状态'{stateName}'：已达到最大状态数{MaxStates}");

            int index = _nextIndex++;
            _nameToIndex[stateName] = index;
            _indexToName[index] = stateName;
            return index;
        }

        /// <summary>
        /// 获取状态名称对应的位索引
        /// </summary>
        public static int GetIndex(string stateName)
        {
            if (_nameToIndex.TryGetValue(stateName, out var index))
                return index;
            return -1;
        }

        /// <summary>
        /// 获取位索引对应的状态名称
        /// </summary>
        public static string GetName(int index)
        {
            return _indexToName.TryGetValue(index, out var name) ? name : $"Unknown({index})";
        }

        /// <summary>
        /// 构建一个世界状态（从多个状态名称）
        /// </summary>
        public static GOAPWorldState Build(params string[] stateNames)
        {
            GOAPWorldState state = GOAPWorldState.Empty;
            foreach (var name in stateNames)
            {
                int idx = GetIndex(name);
                if (idx >= 0)
                    state.Set(idx, true);
            }

            return state;
        }

        /// <summary>
        /// 重置注册表（用于测试或场景切换）
        /// </summary>
        public static void Clear()
        {
            _nameToIndex.Clear();
            _indexToName.Clear();
            _nextIndex = 0;
        }
    }
}