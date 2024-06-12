namespace GameFrame
{
    public class SystemEntity : IReference
    {
        public ISystemObject SystemObject { get; private set; }
        public IEntity Entity { get; private set; }

        public void Create(ISystemObject systemobject, IEntity entity)
        {
            SystemObject = systemobject;
            Entity = entity;
        }

        public void Clear()
        {
            SystemObject = null;
            Entity = null;
        }
    }
}