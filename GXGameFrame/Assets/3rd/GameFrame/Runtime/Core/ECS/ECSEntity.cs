using System;
using System.Collections.Generic;

namespace GameFrame
{
    [Serializable]
    public partial class ECSComponent : IDisposable
    {
        public virtual void Dispose()
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

        private World world;

        public GXArray<ECSComponent> EcsComponentArray { get; private set; }

        private static int m_SerialId;

        private List<int> indexList;

        public void Initialize(IEntity sceneParent, IEntity parent, int id)
        {
            EcsComponentArray = ReferencePool.Acquire<GXArray<ECSComponent>>();
            indexList = new List<int>(128);
            EcsComponentArray.Init(GXComponents.ComponentTypes.Length);
            State = IEntity.EntityState.IsRunning;
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
            if (EcsComponentArray[cid] != null)
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            ECSComponent entity = EcsComponentArray.Add(cid, type);
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
            if (EcsComponentArray[cid] == null)
            {
                Type type = GXComponents.ComponentTypes[cid];
                throw new Exception($"entity not already  component: {type.FullName}");
            }

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
        public void ClearAllComponent()
        {
            indexList.AddRange(EcsComponentArray.indexList);
            ReferencePool.Release(EcsComponentArray);
            ((World) Parent).Reactive(indexList, this);
            indexList.Clear();
        }

        public void Dispose()
        {
            State = IEntity.EntityState.IsClear;
            ClearAllComponent();
        }
    }
}