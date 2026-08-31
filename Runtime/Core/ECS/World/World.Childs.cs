using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public partial class World
    {
        public JumpIndexArrayEx<EffEntity> Children { get; private set; }

        public int ChildsCount => Children.Count;

        private Stack<int> heritageId = new();

        private ulong nextEntityGeneration;

        private void InitializeChilds()
        {
            Children = new JumpIndexArrayEx<EffEntity>();
            nextEntityGeneration = 0;
        }

        public void EstimateChildsCount(int count)
        {
            Children.Init(count);
            InitComponents(count);
        }

        public virtual T AddChild<T>() where T : EffEntity
        {
            return (T) CreateChild(typeof(T));
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

            var entity = Children.Add(id, type);
            entity.SetContext(this);
            entity.SetGeneration(GetNextEntityGeneration());
            entity.OnDirty(this, id);
            Expansion();
            return entity;
        }

        private ulong GetNextEntityGeneration()
        {
            nextEntityGeneration++;
            return nextEntityGeneration;
        }


        public virtual void RemoveChild(EffEntity effEntity)
        {
            bool b = Children.Remove(effEntity.ID);
            if (!b)
                return;
            heritageId.Push(effEntity.ID);
        }

        public EffEntity GetChild(int id)
        {
            return Children[id];
        }

        public EffEntity GetChild(in EffEntityHandle handle)
        {
            return TryGetChild(in handle, out var entity) ? entity : null;
        }

        public bool TryGetChild(in EffEntityHandle handle, out EffEntity entity)
        {
            entity = null;
            var candidate = Children[handle.Id];
            if (candidate == null || !candidate.IsAction || candidate.Generation != handle.Generation)
            {
                return false;
            }

            entity = candidate;
            return true;
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