namespace GameFrame
{
    public interface IHide
    {
    }

    public interface IHideSystem
    {
        void Run(object o);
    }

    public abstract class HideSystem<T> : ISystem, IHideSystem where T : IHide
    {
        public void Run(object o)
        {
            this.Hide((T) o);
        }

        protected abstract void Hide(T self);

        public void Clear()
        {
        }
    }
}