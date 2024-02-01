using System;
using System.Collections.Generic;

namespace GameFrame
{
    public abstract class Entity : IEntity
    {
        public IEntity.EntityStatus m_EntityStatus { get; set; }

        public IEntity SceneParent { get; set; }

        public IEntity Parent { get; set; }

        public int ID { get; set; }
        
        public string Name { get; set; }

        private Dictionary<Type, IEntity> m_Components;

        public Dictionary<Type, IEntity> Components => m_Components;

        private HashSet<IEntity> m_Children;

        public HashSet<IEntity> Children => m_Children;

        private static int m_SerialId;

        protected Entity()
        {
            m_Components = new();
            m_Children = new();
        }
        
        protected virtual IEntity Create<T>(bool isComponent) where T : IEntity
        {
            Type type = typeof(T);
            return Create(type, isComponent);
        }

        private IEntity Create(Type type, bool isComponent)
        {
            IEntity entity = ReferencePool.Acquire(type) as IEntity;
            entity.m_EntityStatus = IEntity.EntityStatus.IsCreated;
            entity.Parent = this;
            entity.SceneParent = this is MainScene ? this : SceneParent;
            entity.ID = ++m_SerialId;
            if (isComponent)
            {
                m_Components.Add(type, entity);
            }
            else
            {
                m_Children.Add(entity);
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
            if (!m_Components.TryGetValue(type, out IEntity entity))
            {
                throw new Exception($"entity not already  component: {type.FullName}");
            }

            m_Components.Remove(type);
            Remove(entity);
        }


        /// <summary>
        /// 删除组件
        /// </summary>
        /// <param name="entity"></param>
        private void Remove(IEntity entity)
        {
            ReferencePool.Release(entity);
            EnitityHouse.Instance.RemoveEntity(entity);
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
            if (this.m_Components != null && this.m_Components.ContainsKey(type))
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


        public T AddComponent<T, P1>(P1 p1) where T : class, IEntity
        {
            Type type = typeof(T);
            return (T) AddComponent(type, p1);
        }

        public IEntity AddComponent<P1>(Type type, P1 p1)
        {
            IEntity component = CreateComponent(type);
            if (component is IStartSystem<P1> system)
            {
                system.SystemStart(p1);
            }
            EnitityHouse.Instance.AddSlefUpdateSystem(component);
            return component;
        }

        public T AddComponent<T, P1, P2>(P1 p1, P2 p2) where T : class, IEntity
        {
            Type type = typeof(T);
            return (T) AddComponent(type, p1, p2);
        }

        public IEntity AddComponent<P1, P2>(Type type, P1 p1, P2 p2)
        {
            IEntity component = CreateComponent(type);
            if (component is IStartSystem<P1,P2> system)
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
            if (this.m_Components != null && !this.m_Components.TryGetValue(type, out value))
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

        public T AddChild<T, P1, P2>(P1 p1, P2 p2) where T : class, IEntity
        {
            Type type = typeof(T);
            return (T) AddChild(type, p1, p2);
        }

        public IEntity AddChild<P1, P2>(Type type, P1 p1, P2 p2)
        {
            IEntity component = Create(type, false);
            if (component is IStartSystem<P1,P2> system)
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
            if (!m_Children.Remove(entity))
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
            foreach (var item in m_Children)
            {
                if (item is Entity entity)
                {
                    entity.ClearAllChild();
                }

                Remove(item);
                item.ClearAllComponent();
            }

            m_Children.Clear();
        }

        /// <summary>
        /// 清理所有的组件
        /// </summary>
        public void ClearAllComponent()
        {
            foreach (var item in m_Components)
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

            m_Components.Clear();
        }


        /// <summary>
        /// 清除
        /// </summary>
        public virtual void Clear()
        {
            m_EntityStatus = IEntity.EntityStatus.IsClear;
            Parent = null;
            ClearAllChild();
            ClearAllComponent();
        }
    }
}