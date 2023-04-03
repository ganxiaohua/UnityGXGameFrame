namespace GameFrame
{
    public interface IShow
    {
    }

    public interface IShowSystem
    {
        void Run(object o);
    }
    
    public abstract class ShowSystem<T> : ISystem, IShowSystem where T : IShow
    {
        public void Run(object o)
        {
            this.Show((T) o);
        }

        protected abstract void Show(T self);

        public void Clear()
        {
        }
    }
}