using System;

namespace GameFrame
{
    public class SystemObject : IReference
    {
        enum ISystemType : byte
        {
            None = 0,
            IsCreated = 1,
            IsClear = 2,
        }

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
            // m_ISystemType = ISystemType.IsCreated;
        }

        public void Clear()
        {
            ReferencePool.Release(System);
            // m_ISystemType = ISystemType.IsClear;
        }
    }
}