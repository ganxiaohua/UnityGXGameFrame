namespace GameFrame
{
    public interface IUpdate
    {
    }

    public interface IUpdateSystem
    {
        void Run(object o, float elapseSeconds, float realElapseSeconds);
    }

    public abstract class UpdateSystem<T> : ISystem, IUpdateSystem where T : IUpdate
    {
        public void Run(object o, float elapseSeconds, float realElapseSeconds)
        {
            this.Update((T) o, elapseSeconds, realElapseSeconds);
        }

        protected abstract void Update(T self, float elapseSeconds, float realElapseSeconds);

        public void Clear()
        {
            
        }
    }

    // -------------------------------------------
    public interface IECSUpdateSystem : ISystem
    {
        void Update(float elapseSeconds, float realElapseSeconds);
    }
}