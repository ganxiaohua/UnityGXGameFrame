using System;

namespace GameFrame
{
    public class SystemEntity : IDisposable
    {
        public ISystemObject SystemObject { get; private set; }
        public IEntity Entity { get; private set; }

        public void Create(ISystemObject systemobject, IEntity entity)
        {
            SystemObject = systemobject;
            Entity = entity;
        }

        public void Dispose()
        {
            SystemObject = null;
            Entity = null;
        }
    }
}