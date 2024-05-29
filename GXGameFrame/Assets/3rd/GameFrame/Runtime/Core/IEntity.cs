namespace GameFrame
{
    public interface IEntity : IReference
    {
        public enum EntityState : byte
        {
            None = 0,
            IsCreated,
            IsClear
        }

        public IEntity SceneParent { get; }

        public IEntity Parent { get; }

        public int ID { get; }

        public string Name { get; }

        public EntityState State { get; }

        public void Initialize(IEntity sceneParent,IEntity parent,int id);
        public void ClearAllComponent();
    }
}