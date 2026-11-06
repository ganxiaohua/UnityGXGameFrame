using System;
using UnityEngine;

namespace GameFrame.Runtime
{
    public sealed class UniqueTimer : IMutablePriorityQueueItem
    {
        public string Name { get; set; }

        public int MutableIndex { get; set; } = -1;

        public TimerGroup Group { get; }

        public float ExpireTime { get; private set; }

        public bool Scheduled => Group.Contains(this);

        private readonly Action expireCallback;

        public UniqueTimer(TimerGroup group, Action expireCallback)
        {
            Group = group ?? throw new ArgumentNullException(nameof(group));
            Assert.IsNotNull(expireCallback, "Expire Callback can't be null");
            this.expireCallback = expireCallback;
            ExpireTime = float.PositiveInfinity;
        }

        public void Schedule(float lifetime)
        {
            if (Group.IsDisposed)
                throw new ObjectDisposedException(nameof(TimerGroup));

            ExpireTime = Time.realtimeSinceStartup + lifetime;
            Group.Schedule(this);
        }

        public bool Cancel()
        {
            bool cancelled = Group.Cancel(this);
            ResetSchedule();
            return cancelled;
        }

        internal void ResetSchedule()
        {
            ExpireTime = float.PositiveInfinity;
            MutableIndex = -1;
        }

        public void Execute()
        {
            expireCallback.Invoke();
        }

        public void ExecuteIfScheduled()
        {
            if (Cancel())
                Execute();
        }
    }
}
