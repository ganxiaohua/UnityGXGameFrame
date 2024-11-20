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

        public int ChildsCount { get; private set; }

        public FastSoleList<IEntity> Children;

        private Stack<int> heritageId = new();

        public void Initialize(IEntity parent, int id)
        {
            State = IEntity.EntityState.IsRunning;
            Parent = parent;
            ID = id;
            ChildsCount = 32;
            Children = new FastSoleList<IEntity>(ChildsCount);
        }

        public void EstimateChildsCount(int count)
        {
            ChildsCount = count;
            Children.SetCapacity(ChildsCount);
        }

        public T AddChild<T>() where T : ECSEntity, new()
        {
            var entity = ReferencePool.Acquire<T>();
            AddChild(entity);
            return entity;
        }

        public ECSEntity AddChild()
        {
            var entity = (ECSEntity) ReferencePool.Acquire(typeof(ECSEntity));
            AddChild(entity);
            return entity;
        }

        private void AddChild(ECSEntity entity)
        {
            int id = 0;
            if (!heritageId.TryPop(out id))
            {
                id = ecsSerialId++;
            }

            entity.Initialize(this, id);
            entity.SetContext(this);
            Children.Add(entity);
        }

        public void RemoveChild(IEntity ecsEntity)
        {
            bool b = Children.Remove(ecsEntity);
            if (!b)
                return;
            heritageId.Push(ecsEntity.ID);
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