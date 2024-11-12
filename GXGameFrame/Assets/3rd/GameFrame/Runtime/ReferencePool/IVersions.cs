using System;

namespace GameFrame
{
    public abstract class VersionsDisposable : IDisposable
    {
        protected int Versions;

        public virtual void Initialize()
        {
            Versions++;
        }

        public virtual void Dispose()
        {
            Versions++;
        }
    }
    
    public abstract class VersionsDisposable<T> : IDisposable
    {
        protected int Versions;

        public virtual void Initialize(T t)
        {
            Versions++;
        }

        public virtual void Dispose()
        {
            Versions++;
        }
    }
}