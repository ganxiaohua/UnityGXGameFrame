using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GameFrame
{
    public abstract class Entity : IEntity
    {
        public IEntity.EntityStatus m_EntityStatus { get; set; }

        public IEntity SceneParent { get; set; }

        public IEntity Parent { get; set; }

        public int ID { get; set; }

        private Dictionary<Type, IEntity> m_Components;

        public Dictionary<Type, IEntity> Components => m_Components;

        private List<int> TypeHashCode;

        private Dictionary<int, IEntity> m_Children;

        public Dictionary<int, IEntity> Children => m_Children;

        private static int m_SerialId;

        protected Entity()
        {
            m_Components = new();
            m_Children = new();
            TypeHashCode = new(8);
        }

        public virtual void Initialize()
        {
        }


        protected virtual IEntity Create<T>(bool isComponent) where T : IEntity
        {
            Type type = typeof(T);
            IEntity entity = ReferencePool.Acquire(type) as IEntity;
            entity.m_EntityStatus = IEntity.EntityStatus.IsCreated;
            entity.Parent = this;
            if (this is SceneEntity)
            {
                entity.SceneParent = this;
            }
            else
            {
                entity.SceneParent = SceneParent;
            }

            entity.ID = ++m_SerialId;
            if (isComponent)
            {
                m_Components.Add(type, entity);
                TypeHashCode.Add(type.GetHashCode());
            }
            else
            {
                m_Children.Add(entity.ID, entity);
            }

            // entity.Parameter = parameter;
            entity.Initialize();
            EnitityHouse.Instance.AddEntity(entity);
            // if (Parameter != null)
            // {
            //     ReferencePool.Release(Parameter);
            // }

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
            TypeHashCode.Remove(type.GetHashCode());
            Remove(entity);
        }

        /// <summary>
        /// 删除children
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="Exception"></exception>
        private void Remove(int id)
        {
            if (!m_Children.TryGetValue(id, out var entity))
            {
                throw new Exception($"entity not already  child: {id}");
            }

            m_Children.Remove(id);
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
            EnitityHouse.Instance.RemoveAllSystem(entity);
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
            if (this.m_Components != null && this.m_Components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            IEntity component = Create<T>(true);
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
            IEntity component = CreateComponent<T>();
            AddSystem<T>(component);
            return (T) component;
        }


        public T AddComponent<T, P1>(P1 p1) where T : class, IEntity
        {
            IEntity component = CreateComponent<T>();
            component.AddStartSystem(p1);
            AddSystem<T>(component);
            return (T) component;
        }

        public T AddComponent<T, P1, P2>(P1 p1, P2 p2) where T : class, IEntity
        {
            IEntity component = CreateComponent<T>();
            component.AddStartSystem(p1,p2);
            AddSystem<T>(component);
            return (T) component;
        }
        

        private void AddSystem<T>(IEntity entity) where T : class, IEntity
        {
            Type type = typeof(T);
            List<Type> systemTypesList = AutoBindSystem.Instance.GetEnitityAllSystem(type);
            if (systemTypesList != null)
            {
                foreach (Type systemtype in systemTypesList)
                {
                    entity.AddSystem(systemtype);
                }
            }
        }

        public T GetComponent<T>() where T : class, IEntity
        {
            Type type = typeof(T);
            IEntity value = null;
            if (this.m_Components != null && !this.m_Components.TryGetValue(type, out value))
            {
                return null;
            }

            return value as T;
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
            if (type is IEntity)
            {
                Remove(type);
            }
        }

        /// <summary>
        /// 挂载实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddChild<T>() where T : class, IEntity
        {
            IEntity component = Create<T>(false);
            AddSystem<T>(component);
            return component as T;
        }

        public T AddChild<T, P1>(P1 p1) where T : class, IEntity
        {
            IEntity component = Create<T>(false);
            component.AddStartSystem(p1);
            AddSystem<T>(component);
            return component as T;
        }

        public T AddChild<T, P1, P2>(P1 p1, P2 p2) where T : class, IEntity
        {
            IEntity component = Create<T>(false);
            component.AddStartSystem(p1,p2);
            AddSystem<T>(component);
            return component as T;
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void RemoveChild(int id)
        {
            Remove(id);
        }

        /// <summary>
        /// 清理所有的子对象
        /// </summary>
        public void ClearAllChild()
        {
            foreach (var item in m_Children)
            {
                if (item.Value is Entity entity)
                {
                    entity.ClearAllChild();
                }

                Remove(item.Value);
                item.Value.ClearAllComponent();
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
            TypeHashCode.Clear();
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