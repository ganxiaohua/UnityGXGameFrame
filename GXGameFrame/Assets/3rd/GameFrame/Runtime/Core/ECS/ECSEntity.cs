using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    public class ECSComponent : IReference
    {
        public virtual void Clear()
        {
        }
    }


    /// <summary>
    /// ECSEntity挂载的一定是Context
    /// </summary>
    public abstract class ECSEntity : IEntity
    {
        public IEntity.EntityStatus m_EntityStatus { get; set; }

        public IEntity SceneParent { get; set; }

        public IEntity Parent { get; set; }

        public int ID { get; set; }

        private GXArray<ECSComponent> m_ECSComponentArray;

        protected ECSEntity()
        {
        }

        /// <summary>
        /// 通用书初始化
        /// </summary>
        public virtual void Initialize()
        {
            m_ECSComponentArray = ReferencePool.Acquire<GXArray<ECSComponent>>();
            m_ECSComponentArray.Init(Components.TotalComponents);
        }


        /// <summary>
        /// 加入组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ECSComponent AddComponent(int index)
        {
            Type type = Components.ComponentTypes[index];
            if (m_ECSComponentArray[index] != null)
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            ECSComponent entity = m_ECSComponentArray.Add(index, Components.ComponentTypes[index]);
            ((Context) Parent).ChangeAddRomoveChildOrCompone(this);
            return entity;
        }

        /// <summary>
        /// 删除组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="Exception"></exception>
        public void RemoveComponent(int index)
        {
            Type type = Components.ComponentTypes[index];
            if (m_ECSComponentArray[index] == null)
            {
                throw new Exception($"entity not already  component: {type.FullName}");
            }

            m_ECSComponentArray.Remove(index);
            ((Context) Parent).ChangeAddRomoveChildOrCompone(this);
        }

        /// <summary>
        /// 获得组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ECSComponent GetComponent(int index)
        {
            var component = m_ECSComponentArray[index];
            return component;
        }

        /// <summary>
        /// 全部包含
        /// </summary>
        /// <param name="hascode"></param>
        /// <returns></returns>
        public bool HasComponents(int[] indexs)
        {
            for (int index = 0; index < indexs.Length; ++index)
            {
                if (m_ECSComponentArray[indexs[index]] == null)
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasComponent(int indexs)
        {
            if (m_ECSComponentArray[indexs] == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 包含任意一个
        /// </summary>
        /// <param name="indexs"></param>
        /// <returns></returns>
        public bool HasAnyComponent(int[] indexs)
        {
            for (int index = 0; index < indexs.Length; ++index)
            {
                if (m_ECSComponentArray[indexs[index]] != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 清除所有组件
        /// </summary>
        public void ClearAllComponent()
        {
            ReferencePool.Release(m_ECSComponentArray);
            ((Context) Parent).ChangeAddRomoveChildOrCompone(this);
        }

        public void Clear()
        {
            ClearAllComponent();
        }
    }
}