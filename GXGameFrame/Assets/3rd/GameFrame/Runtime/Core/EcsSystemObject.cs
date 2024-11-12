using System;

namespace GameFrame
{
    public interface ISystemObject : IDisposable
    {
        public ISystem System { get;}
    }

    public class SystemObject : ISystemObject
    {
        public ISystem System { get; private set; }
        
        public void AddSystem(ISystem system)
        {
            System = system;
        }

        public void Dispose()
        {
           
        }
    }

    public class EcsSystemObject :ISystemObject 
    {

        // private ISystemType m_ISystemType;
        public ISystem System { get; private set; }

        public void AddSystem<T>() where T : ISystem
        {
            Type type = typeof(T);
            AddSystem(type);
        }

        public void AddSystem(Type type)
        {
            var system = ReferencePool.Acquire(type) as ISystem;
            System = system;
        }

        public void Dispose()
        {
            ReferencePool.Release(System);
        }
    }
}