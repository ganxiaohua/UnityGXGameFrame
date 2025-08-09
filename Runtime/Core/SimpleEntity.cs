namespace GameFrame.Runtime
{
    public class SimpleEntity : IEntity
    {
        public int ID { get; private set; }
        public IEntity Parent { get; private set; }
        public string Name { get; private set; }
        public IEntity.EntityState State { get; private set; }
        public bool IsAction => State == IEntity.EntityState.IsRunning;

        public virtual void OnDirty(IEntity parent, int id)
        {
            State = IEntity.EntityState.IsRunning;
            Parent = parent;
            ID = id;
        }

        public virtual void Dispose()
        {
            State = IEntity.EntityState.IsClear;
            Parent = null;
            ID = 0;
        }
    }
}