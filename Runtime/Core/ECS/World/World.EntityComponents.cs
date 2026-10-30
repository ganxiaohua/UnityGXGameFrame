using System;

namespace GameFrame.Runtime
{
    public unsafe partial class World
    {
        public bool HasComp<T>(EffEntityHandle entity) where T : unmanaged, EffComponent => HasComp(entity, ComponentsID<T>.TID);

        public bool HasComp(EffEntityHandle entity, int cid) => IsAlive(entity) && HasCompAt(entity.Id, cid);

        private bool HasCompAt(int entityId, int cid) =>
            (uint)cid < (uint)MaxComponentCount && entityComponentFlags[entityId][cid];

        public bool HasComps(EffEntityHandle entity, int[] cids)
        {
            if (!IsAlive(entity)) return false;
            foreach (int cid in cids) if (!HasCompAt(entity.Id, cid)) return false;
            return true;
        }

        public bool HasAnyComp(EffEntityHandle entity, int[] cids)
        {
            if (!IsAlive(entity)) return false;
            foreach (int cid in cids) if (HasCompAt(entity.Id, cid)) return true;
            return false;
        }

        public bool TryGetComp<T>(EffEntityHandle entity, out T value) where T : unmanaged, EffComponent
        {
            int cid = ComponentsID<T>.TID;
            if (!HasComp(entity, cid)) { value = default; return false; }
            value = *GetCompPtr<T>(entity.Id, cid);
            return true;
        }

        public ref T GetComp<T>(EffEntityHandle entity) where T : unmanaged, EffComponent => ref *GetCompPtr<T>(entity);

        public T* GetCompPtr<T>(EffEntityHandle entity) where T : unmanaged, EffComponent
        {
            RequireEntity(entity);
            int cid = ComponentsID<T>.TID;
            if (!HasCompAt(entity.Id, cid)) throw new InvalidOperationException($"Entity {entity} does not have {typeof(T).FullName}.");
            return GetCompPtr<T>(entity.Id, cid);
        }

        public void AddCompNoGet<T>(EffEntityHandle entity) where T : unmanaged, EffComponent => AddComp<T>(entity);

        public T* AddComp<T>(EffEntityHandle entity) where T : unmanaged, EffComponent
        {
            RequireEntity(entity);
            int cid = ComponentsID<T>.TID;
            ValidateComponentId(cid);
            if (componentDisposalDepth != 0)
                throw new InvalidOperationException("Components cannot be added from a component's Dispose callback.");
            if (components[cid] == null) throw new InvalidOperationException($"Register {typeof(T).FullName} before adding it.");
            if (entityComponentFlags[entity.Id][cid]) throw new InvalidOperationException($"Entity already has component: {typeof(T).FullName}");
            entityComponentIds[entity.Id].Add(cid);
            entityComponentFlags[entity.Id][cid] = true;
            Reactive(cid, entity);
            // Notifications may remove the entity/component or expand storage; resolve again afterwards.
            return GetCompPtr<T>(entity);
        }

        public bool RemoveComp<T>(EffEntityHandle entity) where T : unmanaged, EffComponent => RemoveComp(entity, ComponentsID<T>.TID);

        public bool RemoveComp(EffEntityHandle entity, int cid)
        {
            if (!HasComp(entity, cid)) return false;
            ReleaseEntityComp(entity, cid, true);
            return true;
        }

        public void Reactive<T>(EffEntityHandle entity) where T : unmanaged, EffComponent => Reactive(entity, ComponentsID<T>.TID);

        public void Reactive(EffEntityHandle entity, int cid)
        {
            RequireEntity(entity);
            if (!HasCompAt(entity.Id, cid)) throw new InvalidOperationException($"Entity {entity} does not have component {cid}.");
            Reactive(cid, entity);
        }

        private void ReleaseEntityComp(EffEntityHandle entity, int cid, bool reactive)
        {
            // Clear membership first so recursive disposal cannot release the same component twice.
            entityComponentFlags[entity.Id][cid] = false;
            entityComponentIds[entity.Id].RemoveSwapBack(cid);
            componentDisposalDepth++;
            try { ComponentDisposeAction.ComponentDisposeActions[cid](GetCompBytes(entity.Id, cid)); }
            finally
            {
                ClearComp(entity.Id, cid);
                componentDisposalDepth--;
                if (reactive) Reactive(cid, entity);
            }
        }
    }
}
