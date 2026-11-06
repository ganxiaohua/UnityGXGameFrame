using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GameFrame.Runtime
{
    /// <summary>
    /// 主线程使用的轮询组；每隔指定时间检查一批条目，不保证遍历顺序。
    /// </summary>
    public sealed class InexactCountdownGroup : IDisposable
    {
        private sealed class ItemReferenceComparer : IEqualityComparer<IInexactCountdown>
        {
            public static readonly ItemReferenceComparer Instance = new();

            public bool Equals(IInexactCountdown x, IInexactCountdown y) => ReferenceEquals(x, y);

            public int GetHashCode(IInexactCountdown obj) => RuntimeHelpers.GetHashCode(obj);
        }

        private readonly InexactCountdownSystem owner;
        private readonly List<IInexactCountdown> items = new();
        private readonly Dictionary<IInexactCountdown, int> itemIndices = new(ItemReferenceComparer.Instance);
        private int nextItemIndex = -1;
        private float nextPollTime;

        internal int SystemIndex { get; set; } = -1;

        public float IntervalSeconds { get; }
        /// <summary>每次间隔检查的数量上限。</summary>
        public int MaxChecksPerFrame { get; }
        public int Count => items.Count;
        public bool IsDisposed { get; private set; }

        internal InexactCountdownGroup(InexactCountdownSystem owner, float intervalSeconds, int maxChecksPerFrame)
        {
            this.owner = owner;
            IntervalSeconds = intervalSeconds;
            MaxChecksPerFrame = maxChecksPerFrame;
        }

        /// <summary>
        /// 同组同一实例只注册一次；续期无需重新加入。
        /// </summary>
        public bool Add(IInexactCountdown item)
        {
            if (itemIndices.ContainsKey(item))
                return false;
            items.Add(item);
            itemIndices.Add(item, items.Count - 1);
            return true;
        }

        public bool Remove(IInexactCountdown item)
        {
            if (item == null || !itemIndices.TryGetValue(item, out var index))
                return false;
            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// 取消全部注册，不触发到期回调。重新加入条目后可在下次更新时检查。
        /// </summary>
        public void Clear()
        {
            nextItemIndex = -1;
            nextPollTime = 0f;
            items.Clear();
            itemIndices.Clear();
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            Clear();
            owner.RemoveGroup(this);
        }

        private void RemoveAt(int index)
        {
            itemIndices.Remove(items[index]);
            int lastIndex = items.Count - 1;
            if (index != lastIndex)
            {
                var lastItem = items[lastIndex];
                items[index] = lastItem;
                itemIndices[lastItem] = index;
            }
            items.RemoveAt(lastIndex);
            if (nextItemIndex >= items.Count)
                nextItemIndex = items.Count - 1;
        }

        internal void Update(float currentTime)
        {
            if (IsDisposed || items.Count == 0 || currentTime < nextPollTime)
                return;
            nextPollTime = currentTime + IntervalSeconds;
            if (nextItemIndex < 0)
                nextItemIndex = items.Count - 1;

            for (int checkedCount = 0; checkedCount < MaxChecksPerFrame && nextItemIndex >= 0; checkedCount++)
            {
                int index = nextItemIndex--;
                var item = items[index];
                if (currentTime >= item.ExpireTime)
                {
                    RemoveAt(index);
                    item.OnCountdownExpired();
                }
            }
        }
    }
}
