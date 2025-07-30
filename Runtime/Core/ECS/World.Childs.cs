using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public partial class World
    {
        public JumpIndexArrayEx<EffEntity> Children { get; private set; }
        
        public int ChildsCount => Children.Count;

        private Stack<int> heritageId = new();

        private void InitializeChilds()
        {
            Children = new JumpIndexArrayEx<EffEntity>();
        }

        public void EstimateChildsCount(int count)
        {
            Children.Init(count);
        }

        public virtual T AddChild<T>() where T : EffEntity
        {
            return (T)CreateChild(typeof(T));
        }

        public virtual EffEntity AddChild()
        {
            return CreateChild(typeof(EffEntity));
        }

        private EffEntity CreateChild(Type type)
        {
            int id = 0;
            if (!heritageId.TryPop(out id))
            {
                id = ecsSerialId++;
            }

            var entity = Children.Add(id,type);
            entity.SetContext(this);
            entity.OnDirty(this, id);
            return entity;
        }

        public virtual void RemoveChild(EffEntity ecsEntity)
        {
            bool b = Children.Remove(ecsEntity.ID);
            if (!b)
                return;
            heritageId.Push(ecsEntity.ID);
        }

        public EffEntity GetChild(int id)
        {
            return Children[id];
        }

        private void ClearAllChild()
        {
            Children.Dispose();
        }

        private void DisposeChilds()
        {
            ClearAllChild();
            heritageId.Clear();
        }
    }
}