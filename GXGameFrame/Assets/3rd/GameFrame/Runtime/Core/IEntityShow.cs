namespace GameFrame
{
    public interface IShow
    {
    }

    public interface IShowSystem
    {
        void Run(object o);
    }

    public interface IShowSystem<P1>
    {
        void Run(object o, P1 p1);
    }

    public interface IShowSystem<P1, P2>
    {
        void Run(object o, P1 p1, P2 p2);
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

    public abstract class ShowSystem<T, P1> : ISystem, IShowSystem<P1> where T : IShow
    {
        public void Run(object o, P1 p1)
        {
            this.Show((T) o, p1);
        }

        protected abstract void Show(T self, P1 p1);

        public void Clear()
        {
        }
    }

    public abstract class ShowSystem<T, P1, P2> : ISystem, IShowSystem<P1, P2> where T : IShow
    {
        public void Run(object o, P1 p1, P2 p2)
        {
            this.Show((T) o, p1, p2);
        }

        protected abstract void Show(T self, P1 p1, P2 p2);

        public void Clear()
        {
        }
    }
}