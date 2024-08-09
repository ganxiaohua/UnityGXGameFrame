namespace GameFrame
{
    public abstract class VersionsReference : IReference
    {
        protected int Versions;

        public virtual void Initialize()
        {
            Versions++;
        }

        public virtual void Clear()
        {
            Versions++;
        }
    }
    
    public abstract class VersionsReference<T> : IReference
    {
        protected int Versions;

        public virtual void Initialize(T t)
        {
            Versions++;
        }

        public virtual void Clear()
        {
            Versions++;
        }
    }
}