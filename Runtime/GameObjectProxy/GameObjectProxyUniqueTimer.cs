namespace GameFrame.Runtime
{
    public class GameObjectProxyUniqueTimer : GameObjectProxy
    {
        private UniqueTimer recycleTimer;

        public override void Initialize(object initData = null)
        {
            base.Initialize(initData);
            recycleTimer = new UniqueTimer(OnRequestRecycle);

#if UNITY_EDITOR
            recycleTimer.Name = this.GetType().Name;
#endif
        }

        public void Recycle(float delay)
        {
            recycleTimer.Schedule(delay);
        }

        public override void Dispose()
        {
            base.Dispose();
            recycleTimer.Cancel();
        }
    }
}