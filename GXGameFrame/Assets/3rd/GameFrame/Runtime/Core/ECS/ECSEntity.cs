﻿using System;
using System.Collections.Generic;

namespace GameFrame
{
    [Serializable]
    public partial class ECSComponent : IReference
    {
        public virtual void Clear()
        {
        }
    }


    /// <summary>
    /// ECSEntity挂载的一定是Context
    /// </summary>
    public class ECSEntity : IEntity
    {
        public IEntity.EntityState State { get; private set; }

        public IEntity SceneParent { get; private set; }

        public IEntity Parent { get; private set; }

        public int ID { get; private set; }

        public string Name { get; set; }

        private World mWorld;

        public GXArray<ECSComponent> ECSComponentArray { get; private set; }

        private static int m_SerialId;

        private List<int> m_indexs;

        public void Initialize(IEntity sceneParent, IEntity parent, int id)
        {
            ECSComponentArray = ReferencePool.Acquire<GXArray<ECSComponent>>();
            m_indexs = new List<int>(128);
            ECSComponentArray.Init(GXComponents.ComponentTypes.Length);
            State = IEntity.EntityState.IsCreated;
            Parent = parent;
            SceneParent = sceneParent;
            ID = id;
        }

        /// <summary>
        /// 加入组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ECSComponent AddComponent(int cid)
        {
            Type type = GXComponents.ComponentTypes[cid];
            if (ECSComponentArray[cid] != null)
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            ECSComponent entity = ECSComponentArray.Add(cid, type);
            mWorld.Reactive(cid, this);
            return entity;
        }

        /// <summary>
        /// 删除组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="Exception"></exception>
        public void RemoveComponent(int cid)
        {
            if (ECSComponentArray[cid] == null)
            {
                Type type = GXComponents.ComponentTypes[cid];
                throw new Exception($"entity not already  component: {type.FullName}");
            }

            ECSComponentArray.Remove(cid);
            mWorld.Reactive(cid, this);
        }

        /// <summary>
        /// 获得组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ECSComponent GetComponent(int cid)
        {
            var component = ECSComponentArray[cid];
            return component;
        }

        /// <summary>
        /// 全部包含
        /// </summary>
        /// <param name="hascode"></param>
        /// <returns></returns>
        public bool HasComponents(int[] cids)
        {
            for (int index = 0; index < cids.Length; ++index)
            {
                if (ECSComponentArray[cids[index]] == null)
                {
                    return false;
                }
            }

            return true;
        }

        public void SetContext(World world)
        {
            mWorld = world;
        }

        public bool HasComponent(int cid)
        {
            return ECSComponentArray[cid] != null;
        }

        /// <summary>
        /// 包含任意一个
        /// </summary>
        /// <param name="indexs"></param>
        /// <returns></returns>
        public bool HasAnyComponent(int[] cids)
        {
            for (int index = 0; index < cids.Length; ++index)
            {
                if (ECSComponentArray[cids[index]] != null)
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
            m_indexs.AddRange(ECSComponentArray.Indexs);
            ReferencePool.Release(ECSComponentArray);
            ((World) Parent).Reactive(m_indexs, this);
            m_indexs.Clear();
        }

        public void Clear()
        {
            State = IEntity.EntityState.IsClear;
            ClearAllComponent();
        }
    }
}