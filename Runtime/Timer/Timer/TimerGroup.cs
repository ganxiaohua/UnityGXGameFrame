using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    /// <summary>
    /// 主线程使用的计时器组；增删和续期只调整本组的优先队列。
    /// </summary>
    public sealed partial class TimerGroup : IDisposable
    {
        private static readonly IComparer<UniqueTimer> TimerComparer =
            Comparer<UniqueTimer>.Create((left, right) => left.ExpireTime.CompareTo(right.ExpireTime));

        private readonly TimerSystem owner;
        private readonly MutablePriorityQueue<UniqueTimer> timers = new(TimerComparer);
        private int version;

        internal int SystemIndex { get; set; } = -1;
        internal long SystemSequence { get; }

        public string Name { get; }
        public int Count => timers.Count;
        public bool IsDisposed { get; private set; }

        internal TimerGroup(TimerSystem owner, long systemSequence, string name)
        {
            this.owner = owner;
            SystemSequence = systemSequence;
            Name = name;
        }

        internal bool Contains(UniqueTimer timer) => timers.Contains(timer);

        internal void Schedule(UniqueTimer timer)
        {
            if (timers.Contains(timer))
                timers.Update(timer);
            else
                timers.Enqueue(timer);
        }

        internal bool Cancel(UniqueTimer timer) => timers.Remove(timer);

        /// <summary>
        /// 取消本组全部计时器，不触发回调；清空后仍可继续使用本组。
        /// </summary>
        public void Clear()
        {
            version++;
            for (var i = 0; i < timers.Count; i++)
                timers[i].ResetSchedule();
            timers.Clear();
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            Clear();
            owner.RemoveGroup(this);
        }

        internal void Update(float currentTime)
        {
            if (IsDisposed)
                return;

            int updateVersion = version;
            while (timers.TryPeek(out var timer) && currentTime >= timer.ExpireTime)
            {
                timers.Dequeue();
                timer.Execute();

                // 回调可以清空或销毁组；清空后新加入的计时器从下一帧处理。
                if (version != updateVersion)
                    return;
            }
        }
    }
}
