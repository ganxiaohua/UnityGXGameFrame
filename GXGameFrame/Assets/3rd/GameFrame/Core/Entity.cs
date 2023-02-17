using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    public abstract class Entity : IReference
    {
        public enum EntityStatus : byte
        {
            None = 0,
            IsCreated = 1 << 1,
            IsClear = 1 << 2,
        }

        protected Entity ComponentParent;

        private Dictionary<Type, Entity> m_Components;
        public Dictionary<Type, Entity> Components => m_Components;

        private List<int> TypeHashCode;

        private Dictionary<int, Entity> m_Children;
        
        public Dictionary<int, Entity> Children => m_Children;


        private EntityStatus m_EntityStatus;

        public int ID { get; private set; }

        private static int m_SerialId;

        protected Entity()
        {
            m_Components = new();
            m_Children = new();
            TypeHashCode = new(8);
            ID = ++m_SerialId;
        }

        protected virtual Entity Create<T>(bool isComponent) where T : Entity
        {
            Type type = typeof(T);
            Entity entity = ReferencePool.Acquire(type) as Entity;
            entity.m_EntityStatus = EntityStatus.IsCreated;
            entity.ComponentParent = this;
            entity.ThisInit();
            if (isComponent)
            {
                m_Components.Add(type, entity);
                TypeHashCode.Add(type.GetHashCode());
            }
            else
            {
                m_Children.Add(entity.ID, entity);
            }

            entity.InitializeSystem();

            EnitityHouse.Instance.AddEntity(entity);
            return entity;
        }

        private void Remove<T>() where T : Entity
        {
            Type type = typeof(T);
            if (!m_Components.TryGetValue(type, out Entity entity))
            {
                throw new Exception($"entity not already  component: {type.FullName}");
            }
            m_Components.Remove(type);
            TypeHashCode.Remove(type.GetHashCode());
            Remove(entity);
            
        }
        private void Remove(int id)
        {
            if (!m_Children.TryGetValue(id, out var entity))
            {
                throw new Exception($"entity not already  child: {id}");
            }
            m_Children.Remove(id); 
            Remove(entity);
        }
        
        private void Remove(Entity entity)
        {
            entity.m_EntityStatus = EntityStatus.IsClear;
            EnitityHouse.Instance.RemoveEntity(entity);
            ReferencePool.Release(entity);
        }

        /// <summary>
        /// 加入entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T AddComponent<T>() where T : Entity
        {
            Type type = typeof(T);
            if (this.m_Components != null && this.m_Components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create<T>(true);
            return component as T;
        }

        public T GetComponent<T>() where T : Entity
        {
            Type type = typeof(T);
            Entity value = null;
            if (this.m_Components != null && !this.m_Components.TryGetValue(type, out value))
            {
                return null;
            }
            return value as T;
        }

        /// <summary>
        /// 全部包含
        /// </summary>
        /// <param name="hascode"></param>
        /// <returns></returns>
        public bool HasComponents(int[] hascode)
        {
            for (int index = 0; index < hascode.Length; ++index)
            {
                if (!TypeHashCode.Contains(hascode[index]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 包含任意一个
        /// </summary>
        /// <param name="indices"></param>
        /// <returns></returns>
        public bool HasAnyComponent(int[] indices)
        {
            for (int index = 0; index < indices.Length; ++index)
            {
                if (TypeHashCode.Contains(indices[index]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 删除组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemoveComponent<T>() where T : Entity
        {
            Remove<T>();
        }

        /// <summary>
        /// 挂载实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public  T AddChild<T>() where T : Entity
        {
            Type type = typeof(T);
            Entity component = Create<T>(false);
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
                item.Value.Remove(item.Value);
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
                item.Value.Remove(item.Value);
            }
            m_Components.Clear();
            TypeHashCode.Clear();
        }

        public virtual void InitializeSystem()
        {
            
        }

        protected virtual void ThisInit()
        {
        }

        /// <summary>
        /// 清除
        /// </summary>
        public virtual void Clear()
        {
            ComponentParent = null;
            ClearAllChild();
            ClearAllComponent();
        }
    }
}