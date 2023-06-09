namespace GameFrame
{
    public interface IFixUpdate
    {
    }

    public interface IFixedUpdateSystem
    {
        void Run(object o, float elapseSeconds, float realElapseSeconds);
    }

    public abstract class FixedUpdateSystem<T> : ISystem, IFixedUpdateSystem where T : IFixUpdate
    {
        public void Run(object o, float elapseSeconds, float realElapseSeconds)
        {
            this.FixedUpdate((T) o, elapseSeconds, realElapseSeconds);
        }

        protected abstract void FixedUpdate(T self, float elapseSeconds, float realElapseSeconds);

        public void Clear()
        {
            
        }
    }

    // -------------------------------------------
    public interface IECSFixedUpdateSystem : ISystem
    {
        void FixedUpdate(float elapseSeconds, float realElapseSeconds);
    }
}