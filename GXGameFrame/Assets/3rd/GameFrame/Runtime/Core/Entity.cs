using System;
using System.Collections.Generic;

namespace GameFrame
{
    public abstract class Entity : IEntity
    {
        public IEntity.EntityState State { get; private set; }

        public IEntity SceneParent { get; private set; }

        public IEntity Parent { get; private set; }

        public int ID { get; private set; }
        
        public string Name { get;  set; }

        private Dictionary<Type, IEntity> components = new();

        public Dictionary<Type, IEntity> Components => components;

        private HashSet<IEntity> children = new ();

        public HashSet<IEntity> Children => children;

        private static int sSerialId;
        
        public void Initialize(IEntity sceneParent, IEntity parent, int id)
        {
            State = IEntity.EntityState.IsCreated;
            Parent = parent;
            SceneParent = sceneParent;
            ID = id;
        }
        
        
        protected virtual IEntity Create<T>(bool isComponent) where T : IEntity
        {
            Type type = typeof(T);
            return Create(type, isComponent);
        }

        private IEntity Create(Type type, bool isComponent)
        {
            IEntity entity = (IEntity)ReferencePool.Acquire(type);
            entity.Initialize(this is MainScene ? this : SceneParent,this,++sSerialId);
            if (isComponent)
            {
                components.Add(type, entity);
            }
            else
            {
                children.Add(entity);
            }
            EnitityHouse.Instance.AddEntity(entity);
            return entity;
        }

        private void Remove<T>() where T : IEntity
        {
            Type type = typeof(T);
            Remove(type);
        }

        private void Remove(Type type)
        {
            if (!components.TryGetValue(type, out IEntity entity))
            {
                throw new Exception($"entity not already  component: {type.FullName}");
            }

            components.Remove(type);
            Remove(entity);
        }


        /// <summary>
        /// 删除组件
        /// </summary>
        /// <param name="entity"></param>
        private void Remove(IEntity entity)
        {
            EnitityHouse.Instance.RemoveEntity(entity);
            ReferencePool.Release(entity);
        }


        /// <summary>
        /// 创建一个组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private IEntity CreateComponent<T>() where T : IEntity
        {
            Type type = typeof(T);
            return CreateComponent(type);
        }

        private IEntity CreateComponent(Type type)
        {
            if (this.components != null && this.components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            IEntity component = Create(type, true);
            return component;
        }

        /// <summary>
        /// 加入entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T AddComponent<T>() where T : class, IEntity
        {
            Type type = typeof(T);
            return (T) AddComponent(type);
        }

        public IEntity AddComponent(Type type)
        {
            IEntity component = CreateComponent(type);
            if (component is IStartSystem system)
            {
                system.SystemStart();
            }
            EnitityHouse.Instance.AddSlefUpdateSystem(component);
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
            if (component is IStartSystem<TP1> system)
            {
                system.SystemStart(p1);
            }
            EnitityHouse.Instance.AddSlefUpdateSystem(component);
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
            if (component is IStartSystem<TP1,TP2> system)
            {
                system.SystemStart(p1,p2);
            }
            EnitityHouse.Instance.AddSlefUpdateSystem(component);
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
            if (this.components != null && !this.components.TryGetValue(type, out value))
            {
                return null;
            }

            return value;
        }


        /// <summary>
        /// 删除组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemoveComponent<T>() where T : IEntity
        {
            Remove<T>();
        }

        public void RemoveComponent(Type type)
        {
            Remove(type);
        }

        /// <summary>
        /// 挂载实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddChild<T>() where T : class, IEntity
        {
            Type type = typeof(T);
            return (T) AddChild(type);
        }

        public IEntity AddChild(Type type)
        {
            IEntity component = Create(type, false);
            if (component is IStartSystem system)
            {
                system.SystemStart();
            }
            EnitityHouse.Instance.AddSlefUpdateSystem(component);
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
            if (component is IStartSystem<P1> system)
            {
                system.SystemStart(p1);
            }
            EnitityHouse.Instance.AddSlefUpdateSystem(component);
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
            if (component is IStartSystem<TP1,TP2> system)
            {
                system.SystemStart(p1,p2);
            }
            EnitityHouse.Instance.AddSlefUpdateSystem(component);
            return component;
        }


        // private void AddOtherSystem(IEntity entity)
        // {
        //     if (entity is IShowSystem)
        //     {
        //         entity.AddSlefSystem((typeof(IShowSystem)));
        //     }
        //
        //     if (entity is IPreShowSystem)
        //     {
        //         entity.AddSlefSystem((typeof(IPreShowSystem)));
        //     }
        //
        //     if (entity is IHideSystem)
        //     {
        //         entity.AddSlefSystem((typeof(IHideSystem)));
        //     }
        // }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void RemoveChild(IEntity entity)
        {
            if (!children.Remove(entity))
            {
                throw new Exception($"entity already not child: {entity.GetType().FullName}");
            }
            Remove(entity);
        }

        /// <summary>
        /// 清理所有的子对象
        /// </summary>
        public void ClearAllChild()
        {
            foreach (var item in children)
            {
                if (item is Entity entity)
                {
                    entity.ClearAllChild();
                }

                Remove(item);
                item.ClearAllComponent();
            }

            children.Clear();
        }
        

        /// <summary>
        /// 清理所有的组件
        /// </summary>
        public void ClearAllComponent()
        {
            foreach (var item in components)
            {
                if (item.Value is Entity entity)
                {
                    entity.Remove(item.Value);
                }
                else if (item.Value is ECSEntity ecsEntity)
                {
                    ReferencePool.Release(ecsEntity);
                }
            }

            components.Clear();
        }


        /// <summary>
        /// 清除
        /// </summary>
        public virtual void Dispose()
        {
            State  = IEntity.EntityState.IsClear;
            Parent = null;
            ClearAllChild();
            ClearAllComponent();
        }
    }
}