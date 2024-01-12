namespace GameFrame
{
    public class SystemEnitiy : IReference
    {
        public ISystemObject SystemObject { get; set; }
        public IEntity Entity { get; set; }

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