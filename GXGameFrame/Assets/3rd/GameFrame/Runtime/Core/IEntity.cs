﻿namespace GameFrame
{
    public interface IEntity : IReference
    {
        public enum EntityStatus : byte
        {
            None = 0,
            IsCreated,
            IsClear
        }

        public IEntity SceneParent { set; }

        public IEntity Parent { set; }

        public int ID { get; set; }

        public string Name { get; set; }

        public EntityStatus m_EntityStatus { set; }

        public void ClearAllComponent();
    }
}