using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GameFrame.Runtime
{
    /// <summary>
    /// 主线程使用的轮询组；每轮分帧检查完毕后，再等待指定间隔。
    /// </summary>
    public sealed class InexactCountdownGroup : IDisposable
    {
        private readonly struct Registration
        {
            public readonly IInexactCountdown Item;
            public readonly long Sequence;

            public Registration(IInexactCountdown item, long sequence)
            {
                Item = item;
                Sequence = sequence;
            }
        }

        private sealed class ItemReferenceComparer : IEqualityComparer<IInexactCountdown>
        {
            public static readonly ItemReferenceComparer Instance = new();

            public bool Equals(IInexactCountdown x, IInexactCountdown y) => ReferenceEquals(x, y);

            public int GetHashCode(IInexactCountdown obj) => RuntimeHelpers.GetHashCode(obj);
        }

        private readonly InexactCountdownSystem owner;
        private readonly LinkedList<Registration> items = new();
        private readonly Dictionary<IInexactCountdown, LinkedListNode<Registration>> itemNodes = new(ItemReferenceComparer.Instance);
        private LinkedListNode<Registration> nextItem;
        private long registrationSequence;
        private long roundLastSequence;
        private int version;
        private bool scanning;
        private bool waitingToStart = true;
        private float nextPollTime;

        internal LinkedListNode<InexactCountdownGroup> SystemNode { get; set; }
        internal long SystemSequence { get; }

        public float IntervalSeconds { get; }
        public int MaxChecksPerFrame { get; }
        public int Count => itemNodes.Count;
        public bool IsDisposed { get; private set; }

        internal InexactCountdownGroup(InexactCountdownSystem owner, long systemSequence, float intervalSeconds, int maxChecksPerFrame)
        {
            if (float.IsNaN(intervalSeconds) || float.IsInfinity(intervalSeconds) || intervalSeconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(intervalSeconds));
            if (maxChecksPerFrame <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxChecksPerFrame));

            this.owner = owner;
            SystemSequence = systemSequence;
            IntervalSeconds = intervalSeconds;
            MaxChecksPerFrame = maxChecksPerFrame;
        }

        /// <summary>
        /// 同组同一实例只注册一次；续期无需重新加入。
        /// </summary>
        public bool Add(IInexactCountdown item)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(InexactCountdownGroup));
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (itemNodes.ContainsKey(item))
                return false;

            var node = items.AddLast(new Registration(item, ++registrationSequence));
            itemNodes.Add(item, node);
            return true;
        }

        public bool Remove(IInexactCountdown item)
        {
            if (item == null || !itemNodes.TryGetValue(item, out var node))
                return false;

            RemoveNode(node);
            return true;
        }

        /// <summary>
        /// 取消全部注册，不触发到期回调。下次更新重新开始等待。
        /// </summary>
        public void Clear()
        {
            version++;
            nextItem = null;
            scanning = false;
            waitingToStart = true;
            items.Clear();
            itemNodes.Clear();
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            Clear();
            owner.RemoveGroup(this);
        }

        private void RemoveNode(LinkedListNode<Registration> node)
        {
            // 回调可能移除下一项，及时把游标移到仍在链表中的节点。
            if (nextItem == node)
                nextItem = node.Next;
            items.Remove(node);
            itemNodes.Remove(node.Value.Item);
        }

        internal void Update(float currentTime)
        {
            if (IsDisposed)
                return;
            if (waitingToStart)
            {
                nextPollTime = currentTime + IntervalSeconds;
                waitingToStart = false;
            }

            if (!scanning)
            {
                if (items.Count == 0 || currentTime < nextPollTime)
                    return;

                nextItem = items.First;
                roundLastSequence = registrationSequence;
                scanning = true;
            }

            int updateVersion = version;
            int checkedCount = 0;
            while (nextItem != null && nextItem.Value.Sequence <= roundLastSequence && checkedCount < MaxChecksPerFrame)
            {
                var node = nextItem;
                nextItem = node.Next;
                checkedCount++;
                var item = node.Value.Item;
                bool expired = currentTime >= item.ExpireTime;
                if (IsDisposed || version != updateVersion)
                    return;

                if (expired && node.List == items)
                {
                    RemoveNode(node);
                    item.OnCountdownExpired();
                }

                if (IsDisposed || version != updateVersion)
                    return;
            }

            // 新注册项不参与本轮，确保持续加入对象也能结束这一轮。
            if (nextItem == null || nextItem.Value.Sequence > roundLastSequence)
            {
                nextItem = null;
                scanning = false;
                nextPollTime = currentTime + IntervalSeconds;
            }
        }
    }
}
