using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public partial class World
    {
        private struct EntitySlot
        {
            internal ulong Generation;
            internal IEntity.EntityState State;
            internal bool Destroying;
        }

        private JumpIndexArray<EffEntityHandle> children;
        private EntitySlot[] entitySlots;
        private string[] entityNames;
        private bool[][] entityComponentFlags;
        private List<int>[] entityComponentIds;
        private readonly Stack<int> heritageId = new();
        private int componentDisposalDepth;
        private bool disposing;
        private bool initialized;
        // Shared across worlds; retained when entering Play Mode without domain reload.
        private static ulong nextEntityGeneration;
        public EntityEnumerable Children => new EntityEnumerable(children);
        public EntityEnumerable Entities => Children;
        public int ChildsCount => children.Count;

        public readonly struct EntityEnumerable
        {
            private readonly JumpIndexArray<EffEntityHandle> items;
            internal EntityEnumerable(JumpIndexArray<EffEntityHandle> items) { this.items = items; }
            public JumpIndexArray<EffEntityHandle>.Enumerator GetEnumerator() => items.GetEnumerator();
        }

        private void InitializeChilds()
        {
            children = new JumpIndexArray<EffEntityHandle>();
            entitySlots = Array.Empty<EntitySlot>();
            entityNames = Array.Empty<string>();
            entityComponentFlags = Array.Empty<bool[]>();
            entityComponentIds = Array.Empty<List<int>>();
            heritageId.Clear();
            ecsSerialId = 0;
            disposing = false;
            initialized = true;
        }

        public void EstimateChildsCount(int count)
        {
            RequireWorld();
            if (children.isInit) throw new InvalidOperationException("Entity storage has already been initialized.");
            count = Math.Max(1, count);
            children.Init(count);
            EnsureEntityCapacity(count);
            InitComponents(count);
        }

        private void EnsureEntityCapacity(int count)
        {
            if (count <= entitySlots.Length) return;
            Array.Resize(ref entitySlots, count);
            Array.Resize(ref entityNames, count);
            Array.Resize(ref entityComponentFlags, count);
            Array.Resize(ref entityComponentIds, count);
        }

        public EffEntityHandle CreateEntity()
        {
            RequireWorld();
            if (disposing || componentDisposalDepth != 0)
                throw new InvalidOperationException("Entities cannot be created during world or component disposal.");
            if (!children.isInit) throw new InvalidOperationException("Initialize entity storage before creating entities.");
            if (!heritageId.TryPop(out int id)) id = ecsSerialId++;
            children.EnsureCapacity(Math.Max(children.AllCount * (id >= children.AllCount ? 2 : 1), id + 1));
            EnsureEntityCapacity(children.AllCount);
            Expansion();
            var entity = new EffEntityHandle(id, checked(++nextEntityGeneration));
            entitySlots[id] = new EntitySlot { Generation = entity.Generation, State = IEntity.EntityState.IsRunning };
            entityComponentFlags[id] ??= new bool[MaxComponentCount];
            entityComponentIds[id] ??= new List<int>(MaxComponentCount);
            children.Set(id, entity);
            try
            {
                OnEntityCreated(entity);
                foreach (var group in groups.Values) group.HandleEntitySilently(entity);
                return entity;
            }
            catch
            {
                DestroyEntity(entity);
                throw;
            }
        }

        protected virtual void OnEntityCreated(EffEntityHandle entity) { }
        protected virtual void OnEntityDestroying(EffEntityHandle entity) { }

        public bool IsAlive(EffEntityHandle entity) => initialized && entity.Generation != 0 &&
            (uint)entity.Id < (uint)entitySlots.Length && entitySlots[entity.Id].Generation == entity.Generation &&
            entitySlots[entity.Id].State == IEntity.EntityState.IsRunning;

        internal void RequireEntity(EffEntityHandle entity)
        {
            if (!IsAlive(entity)) throw new InvalidOperationException($"Entity {entity} is not alive in this world.");
        }

        public bool DestroyEntity(EffEntityHandle entity)
        {
            if (!IsAlive(entity) || entitySlots[entity.Id].Destroying) return false;
            int id = entity.Id;
            entitySlots[id].Destroying = true;
            // Capability callbacks must still be able to read/remove Owner components.
            OnEntityDestroying(entity);
            entitySlots[id].State = IEntity.EntityState.IsClear;
            foreach (var group in groups.Values) group.HandleEntity(entity);
            var ids = entityComponentIds[id];
            while (ids.Count != 0) ReleaseEntityComp(entity, ids[ids.Count - 1], false);
            entityNames[id] = null;
            children.Remove(id);
            entitySlots[id].Destroying = false;
            heritageId.Push(id);
            return true;
        }

        public string GetName(EffEntityHandle entity) { RequireEntity(entity); return entityNames[entity.Id]; }
        public void SetName(EffEntityHandle entity, string name) { RequireEntity(entity); entityNames[entity.Id] = name; }
        public IReadOnlyList<int> GetComponentIds(EffEntityHandle entity) { RequireEntity(entity); return entityComponentIds[entity.Id]; }
        public IEntity.EntityState GetState(EffEntityHandle entity) => IsAlive(entity) ? IEntity.EntityState.IsRunning : IEntity.EntityState.IsClear;

        private void DisposeChilds()
        {
            if (children.isInit)
            {
                foreach (var entity in children) DestroyEntity(entity);
                children.Dispose();
            }
            heritageId.Clear();
            entitySlots = Array.Empty<EntitySlot>();
            entityNames = Array.Empty<string>();
            entityComponentFlags = Array.Empty<bool[]>();
            entityComponentIds = Array.Empty<List<int>>();
        }
    }
}
