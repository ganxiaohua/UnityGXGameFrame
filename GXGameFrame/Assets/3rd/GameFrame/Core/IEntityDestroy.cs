namespace GameFrame
{
    public interface IDestroy
    {
    }

    public interface IDestroySystem
    {
        void Run(object o);
    }

    public abstract class DestroySystem<T> : ISystem, IDestroySystem where T : IDestroy
    {
        public void Run(object o)
        {
            this.Destroy((T) o);
        }

        protected abstract void Destroy(T self);

        public void Clear()
        {
        }
    }
}