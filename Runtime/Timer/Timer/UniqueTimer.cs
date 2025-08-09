using System;
using UnityEngine;

namespace GameFrame.Runtime
{
    public sealed class UniqueTimer : IMutablePriorityQueueItem
    {
        public string Name { get; set; }

        public int MutableIndex { get; set; }

        public float ExpireTime { get; private set; }

        public bool Scheduled => TimerSystem.Instance.Contains(this);

        private Action expireCallback;

        public UniqueTimer(Action expireCallback)
        {
            Assert.IsNotNull(expireCallback, "Expire Callback can't be null");
            this.expireCallback = expireCallback;
            ExpireTime = float.PositiveInfinity;
        }

        public void Schedule(float lifetime)
        {
            ExpireTime = Time.realtimeSinceStartup + lifetime;
            TimerSystem.Instance.Schedule(this);
        }

        public bool Cancel()
        {
            ExpireTime = float.PositiveInfinity;
            return TimerSystem.Instance.Cancel(this);
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