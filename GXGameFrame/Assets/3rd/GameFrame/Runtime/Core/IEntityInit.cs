namespace GameFrame
{
    public interface IInit
    {
    }

    public interface IIInitSystem
    {
        void Run(object o);
    }

    public abstract class InitSystem<T> : ISystem, IIInitSystem where T : IInit
    {
        public void Run(object o)
        {
            this.Init((T) o);
        }

        protected abstract void Init(T self);

        public void Clear()
        {
        }
    }
// --------------------------
    public interface IECSInitSystem:ISystem
    {
        void Initialize(Context entity);
    }

}