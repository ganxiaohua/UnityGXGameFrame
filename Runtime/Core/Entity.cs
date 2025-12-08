using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public abstract class Entity : IEntity, IVersions
    {
        public IEntity.EntityState State { get; private set; }

        public IEntity Parent { get; private set; }

        public int ID { get; private set; }

        public string Name { get; set; }

        public int Versions { get; private set; }

        public bool IsAction => State == IEntity.EntityState.IsRunning;

        public Dictionary<Type, IEntity> Components { get; private set; } = new();

        public HashSet<IEntity> Children { get; private set; } = new();

        private int serialId;

        public void OnDirty(IEntity parent, int id)
        {
            State = IEntity.EntityState.IsRunning;
            Parent = parent;
            serialId = 0;
            ID = id;
            Versions++;
        }


        protected virtual IEntity Create<T>(bool isComponent) where T : IEntity
        {
            Type type = typeof(T);
            return Create(type, isComponent);
        }

        private IEntity Create(Type type, bool isComponent)
        {
            IEntity entity = (IEntity) ReferencePool.Acquire(type);
            entity.OnDirty(this, serialId++);
            if (isComponent)
            {
                Components.Add(type, entity);
            }
            else
            {
                Children.Add(entity);
            }

            EntityHouse.Instance.AddEntity(entity);
            return entity;
        }

        private void Remove<T>() where T : IEntity
        {
            Type type = typeof(T);
            Remove(type);
        }

        private void Remove(Type type)
        {
            if (!Components.TryGetValue(type, out IEntity entity))
            {
                throw new Exception($"entity not already  component: {type.FullName}");
            }

            Components.Remove(type);
            Remove(entity);
        }

        private void Remove(IEntity entity)
        {
            EntityHouse.Instance.RemoveEntity(entity);
            ReferencePool.Release(entity);
        }

        private IEntity CreateComponent<T>() where T : IEntity
        {
            Type type = typeof(T);
            return CreateComponent(type);
        }

        private IEntity CreateComponent(Type type)
        {
            if (this.Components != null && this.Components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            IEntity component = Create(type, true);
            return component;
        }

        public T AddComponent<T>() where T : class, IEntity
        {
            Type type = typeof(T);
            return (T) AddComponent(type);
        }

        public IEntity AddComponent(Type type)
        {
            IEntity component = CreateComponent(type);
            if (component is IInitializeSystem system)
            {
                system.SystemInitialize();
            }

            EntityHouse.Instance.AddUpdateSystem(component);
            return component;
        }


        public T AddComponent<T, TP1>(TP1 p1) where T : class, IEntity
        {
            Type type = typeof(T);
            return (T) AddComponent(type, p1);
        }

        public IEntity AddComponent<TP1>(Type type, TP1 p1)
        {
            IEntity component = CreateComponent(type);
            if (component is IInitializeSystem<TP1> system)
            {
                system.SystemInitialize(p1);
            }

            EntityHouse.Instance.AddUpdateSystem(component);
            return component;
        }

        public T AddComponent<T, TP1, TP2>(TP1 p1, TP2 p2) where T : class, IEntity
        {
            Type type = typeof(T);
            return (T) AddComponent(type, p1, p2);
        }

        public IEntity AddComponent<TP1, TP2>(Type type, TP1 p1, TP2 p2)
        {
            IEntity component = CreateComponent(type);
            if (component is IInitializeSystem<TP1, TP2> system)
            {
                system.SystemInitialize(p1, p2);
            }

            EntityHouse.Instance.AddUpdateSystem(component);
            return component;
        }

        public bool HasComponent<T>() where T : class, IEntity
        {
            if (GetComponent<T>() != null)
            {
                return true;
            }

            return false;
        }

        public T GetComponent<T>() where T : class, IEntity
        {
            Type type = typeof(T);
            return (T) GetComponent(type);
        }

        public IEntity GetComponent(Type type)
        {
            IEntity value = null;
            if (this.Components != null && !this.Components.TryGetValue(type, out value))
            {
                return null;
            }

            return value;
        }

        public void TryRemoveComponent<T>() where T : class, IEntity
        {
            if (HasComponent<T>())
            {
                RemoveComponent<T>();
            }
        }

        public void RemoveComponent<T>() where T : class, IEntity
        {
            Remove<T>();
        }

        public void RemoveComponent(Type type)
        {
            Remove(type);
        }

        public void RemoveComponent(IEntity type)
        {
            Remove(type.GetType());
        }

        public T AddChild<T>() where T : class, IEntity
        {
            Type type = typeof(T);
            return (T) AddChild(type);
        }

        public IEntity AddChild(Type type)
        {
            IEntity component = Create(type, false);
            if (component is IInitializeSystem system)
            {
                system.SystemInitialize();
            }

            EntityHouse.Instance.AddUpdateSystem(component);
            return component;
        }

        public T AddChild<T, P1>(P1 p1) where T : class, IEntity
        {
            Type type = typeof(T);
            return (T) AddChild(type, p1);
        }

        public IEntity AddChild<P1>(Type type, P1 p1)
        {
            IEntity component = Create(type, false);
            if (component is IInitializeSystem<P1> system)
            {
                system.SystemInitialize(p1);
            }

            EntityHouse.Instance.AddUpdateSystem(component);
            return component;
        }

        public T AddChild<T, TP1, TP2>(TP1 p1, TP2 p2) where T : class, IEntity
        {
            Type type = typeof(T);
            return (T) AddChild(type, p1, p2);
        }

        public IEntity AddChild<TP1, TP2>(Type type, TP1 p1, TP2 p2)
        {
            IEntity component = Create(type, false);
            if (component is IInitializeSystem<TP1, TP2> system)
            {
                system.SystemInitialize(p1, p2);
            }

            EntityHouse.Instance.AddUpdateSystem(component);
            return component;
        }


        public void RemoveChild(IEntity entity)
        {
            if (!Children.Remove(entity))
            {
                throw new Exception($"entity already not child: {entity.GetType().FullName}");
            }

            Remove(entity);
        }

        public void ClearAllChild()
        {
            foreach (var item in Children)
            {
                if (item is Entity entity)
                {
                    entity.ClearAllChild();
                    entity.ClearAllComponent();
                }

                Remove(item);
            }

            Children.Clear();
        }

        public void ClearAllComponent()
        {
            foreach (var item in Components)
            {
                if (item.Value is Entity entity)
                {
                    entity.Remove(item.Value);
                }
                else if (item.Value is World world)
                {
                    ReferencePool.Release(world);
                }
            }

            Components.Clear();
        }

        public virtual void Dispose()
        {
            State = IEntity.EntityState.IsClear;
            Parent = null;
            ClearAllChild();
            ClearAllComponent();
            Versions++;
        }
    }
}