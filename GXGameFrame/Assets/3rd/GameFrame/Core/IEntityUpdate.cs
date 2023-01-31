namespace GameFrame
{
    public interface IUpdate
    {
    }

    public interface IUpdateSystem
    {
        void Run(object o);
    }

    public abstract class UpdateSystem<T> : ISystem, IUpdateSystem where T : IUpdate
    {
        public void Run(object o)
        {
            this.Update((T) o);
        }

        protected abstract void Update(T self);

        public void Clear()
        {
        }
    }
}