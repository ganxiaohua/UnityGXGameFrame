﻿using System;

namespace GameFrame
{
    [Serializable]
    public partial class ECSComponent : IDisposable
    {
        public ECSEntity Owner;

        public virtual void Dispose()
        {
        }
    }


    /// <summary>
    /// ECSEntity挂载的一定是Context
    /// </summary>
    public class ECSEntity : IEntity, IVersions
    {
        public IEntity.EntityState State { get; private set; }

        public IEntity Parent { get; private set; }

        public int ID { get; private set; }

        public string Name { get; set; }

        public int Versions { get; private set; }

        private World world;

        public bool IsAction => State == IEntity.EntityState.IsRunning;

        public GXArray<ECSComponent> EcsComponentArray { get; private set; }

        public void OnDirty(IEntity parent, int id)
        {
            EcsComponentArray = ReferencePool.Acquire<GXArray<ECSComponent>>();
            EcsComponentArray.Init(world.MaxComponentCount);
            State = IEntity.EntityState.IsRunning;
            Parent = parent;
            ID = id;
            Versions++;
        }

        /// <summary>
        /// 加入组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T AddComponent<T>() where T : ECSComponent
        {
            var cid = ComponentsID<T>.TID;
            if (EcsComponentArray[cid] != null)
            {
                var type = typeof(T);
                throw new Exception($"entity already has component: {type.FullName}");
            }
            T entity = EcsComponentArray.Add<T>(cid);
            entity.Owner = this;
            world.Reactive(cid, this);
            return entity;
        }
        

        /// <summary>
        /// 删除组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="Exception"></exception>
        public void RemoveComponent(int cid)
        {
            var component = EcsComponentArray[cid];
            if (component == null)
            {
                return;
            }

            component.Owner = null;
            EcsComponentArray.Remove(cid);
            world.Reactive(cid, this);
        }

        /// <summary>
        /// 获得组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ECSComponent GetComponent(int cid)
        {
            var component = EcsComponentArray[cid];
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
                if (EcsComponentArray[cids[index]] == null)
                {
                    return false;
                }
            }

            return true;
        }

        public void SetContext(World world)
        {
            this.world = world;
        }

        public bool HasComponent(int cid)
        {
            return EcsComponentArray[cid] != null;
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
                if (EcsComponentArray[cids[index]] != null)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// 清除所有组件
        /// </summary>
        private void ClearAllComponent()
        {
            var list = EcsComponentArray.IndexList;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                RemoveComponent(list[i]);
            }

            ReferencePool.Release(EcsComponentArray);
        }

        public void Dispose()
        {
            State = IEntity.EntityState.IsClear;
            Versions++;
            ClearAllComponent();
        }
    }
}