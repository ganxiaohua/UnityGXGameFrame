using System;

namespace GameFrame
{
    public interface IEntity : IDisposable
    {
        public enum EntityState : byte
        {
            None = 0,
            IsRunning,
            IsClear
        }

        public IEntity SceneParent { get; }

        public IEntity Parent { get; }

        public int ID { get; }

        public string Name { get; }

        public EntityState State { get; }

        public void Initialize(IEntity sceneParent, IEntity parent, int id);
        public void ClearAllComponent();
    }
}