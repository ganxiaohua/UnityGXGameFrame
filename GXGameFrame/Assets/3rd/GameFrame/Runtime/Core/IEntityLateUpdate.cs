namespace GameFrame
{
    public interface ILateUpdate
    {
    }

    public interface ILateUpdateSystem
    {
        void Run(object o, float elapseSeconds, float realElapseSeconds);
    }

    public abstract class LateUpdateSystem<T> : ISystem, ILateUpdateSystem where T : ILateUpdate
    {
        public void Run(object o, float elapseSeconds, float realElapseSeconds)
        {
            this.LateUpdate((T) o, elapseSeconds, realElapseSeconds);
        }

        protected abstract void LateUpdate(T self, float elapseSeconds, float realElapseSeconds);

        public void Clear()
        {
            
        }
    }

    // -------------------------------------------
    public interface IECSLateUpdateSystem : ISystem
    {
        void LateUpdate(float elapseSeconds, float realElapseSeconds);
    }
}