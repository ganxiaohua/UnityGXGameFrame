namespace GameFrame.Runtime
{
    public sealed partial class TimerSystem : Singleton<TimerSystem>
    {
        private readonly MutablePriorityQueue<UniqueTimer> timers = new MutablePriorityQueue<UniqueTimer>(CompareTimer);


        private static int CompareTimer(UniqueTimer left, UniqueTimer right)
        {
            var tL = left.ExpireTime;
            var tR = right.ExpireTime;
            if (float.IsInfinity(tL)) return -1;
            if (float.IsInfinity(tR)) return 1;
            return tL < tR ? -1 : (tL > tR ? 1 : 0);
        }

        public bool Contains(UniqueTimer timer)
        {
            return timers.Contains(timer);
        }

        public void Schedule(UniqueTimer timer)
        {
            if (timers.Contains(timer))
                timers.Update(timer);
            else
                timers.Enqueue(timer);
        }

        public bool Cancel(UniqueTimer timer)
        {
            return timers.Remove(timer);
        }

        public void Update(float deltaTime)
        {
            var time = deltaTime;
            while (timers.TryPeek(out var timer) && time >= timer.ExpireTime)
            {
                timers.Dequeue();
                timer.Execute();
            }
        }
    }
}