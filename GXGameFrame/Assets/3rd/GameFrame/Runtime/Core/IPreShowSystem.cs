namespace GameFrame
{
    public interface IPreShow
    {
        
    }
    public interface IPreShowSystem
    {
        void Run(object o, bool isFirstShow);
    }

    public abstract class PreShowSystem<T> : ISystem, IPreShowSystem where T : IShow
    {
        public void Run(object o, bool isFirstShow)
        {
            this.PreShow((T) o, isFirstShow);
        }

        protected abstract void PreShow(T self, bool isFirstShow);

        public void Clear()
        {
        }
    }
}