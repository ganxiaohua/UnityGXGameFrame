using System.Collections.Generic;

namespace GameFrame
{
    public partial class World
    {
        public int ChildsCount { get; private set; }

        public GXHashSet<IEntity> Children;

        private Stack<int> heritageId = new();
        
        private void InitializeChilds()
        {
            ChildsCount = 32;
            Children = new GXHashSet<IEntity>(ChildsCount);
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

            entity.OnDirty(this, id);
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

        private void DisposeChilds()
        {
            ClearAllChild();
            ChildsCount = 0;
            heritageId.Clear();
        }
    }
}