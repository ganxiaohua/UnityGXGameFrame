﻿namespace GameFrame
{
    public interface IStart
    {
    }

    public interface IStartSystem
    {
        void Run(object o);
    }

    public interface IStartSystem<P1>
    {
        void Run(object o, P1 p1);
    }

    public abstract class StartSystem<T> : ISystem, IStartSystem where T : IStart
    {
        public void Run(object o)
        {
            this.Start((T) o);
        }

        protected abstract void Start(T self);

        public void Clear()
        {
        }
    }

    public abstract class StartSystem<T, P1> : ISystem, IStartSystem<P1> where T : IStart
    {
        public void Run(object o, P1 p1)
        {
            this.Init((T) o, p1);
        }

        protected abstract void Init(T self, P1 p1);

        public void Clear()
        {
        }
    }

// --------------------------
    public interface IECSInitSystem : ISystem
    {
        void Initialize(Context entity);
    }
}