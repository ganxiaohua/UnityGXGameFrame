namespace GameFrame
{
    public interface IClear
    {
    }

    public interface IClearSystem
    {
        void Run(object o);
    }

    public abstract class ClearSystem<T> : ISystem, IClearSystem where T : IClear
    {
        public void Run(object o)
        {
            this.Clear((T) o);
        }

        protected abstract void Clear(T self);

        public void Clear()
        {
            
        }
    }
}