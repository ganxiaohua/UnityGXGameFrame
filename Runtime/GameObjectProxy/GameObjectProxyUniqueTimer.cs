namespace GameFrame.Runtime
{
    public class GameObjectProxyUniqueTimer : GameObjectProxy
    {
        private static TimerGroup recycleGroup;
        private UniqueTimer recycleTimer;

        public override void Initialize(object initData = null)
        {
            base.Initialize(initData);
            recycleTimer = CreateRecycleTimer();
        }

        private UniqueTimer CreateRecycleTimer()
        {
            if (recycleGroup == null || recycleGroup.IsDisposed)
                recycleGroup = TimerSystem.Instance.CreateGroup("GameObjectRecycle");

            var timer = new UniqueTimer(recycleGroup, OnRequestRecycle);
#if UNITY_EDITOR
            timer.Name = this.GetType().Name;
#endif
            return timer;
        }

        public void Recycle(float delay)
        {
            if (recycleTimer.Group.IsDisposed)
                recycleTimer = CreateRecycleTimer();
            recycleTimer.Schedule(delay);
        }

        public override void Dispose()
        {
            base.Dispose();
            recycleTimer.Cancel();
        }
    }
}
