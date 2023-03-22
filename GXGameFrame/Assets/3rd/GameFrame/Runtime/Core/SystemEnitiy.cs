namespace GameFrame
{
    public class SystemEnitiy : IReference
    {
        public SystemObject SystemObject { get; set; }
        public IEntity Entity { get; set; }

        public void Create(SystemObject systemobject, IEntity entity)
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