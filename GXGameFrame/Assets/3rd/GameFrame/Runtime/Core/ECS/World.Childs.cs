using System.Collections.Generic;

namespace GameFrame
{
    public partial class World : IEntity
    {
        public IEntity Parent { get; private set; }
        public int ID { get; private set; }
        public string Name { get; set; }
        public IEntity.EntityState State { get; private set; }

        private int ecsSerialId;

        public HashSet<IEntity> Children { get; private set; } = new();

        public void Initialize(IEntity parent, int id)
        {
            State = IEntity.EntityState.IsRunning;
            Parent = parent;
            ID = id;
        }


        public T AddChild<T>() where T : ECSEntity, new()
        {
            var entity = ReferencePool.Acquire<T>();
            Children.Add(entity);
            entity.Initialize(this, ecsSerialId++);
            entity.SetContext(this);
            return entity;
        }

        public ECSEntity AddChild()
        {
            var entity = (ECSEntity) ReferencePool.Acquire(typeof(ECSEntity));
            Children.Add(entity);
            entity.Initialize(this, ecsSerialId++);
            entity.SetContext(this);
            return entity;
        }

        public void RemoveChild(IEntity ecsEntity)
        {
            bool b = Children.Remove(ecsEntity);
            if (b)
                ReferencePool.Release(ecsEntity);
        }

        public void ClearAllChild()
        {
            foreach (var item in Children)
            {
                ReferencePool.Release(item);
            }

            Children.Clear();
        }
    }
}