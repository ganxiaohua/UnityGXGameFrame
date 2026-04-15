using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public partial interface EffComponent : IDisposable
    {
    }

    public unsafe class EffEntity : IEntity, IVersions
    {
        public IEntity.EntityState State { get; private set; }

        public IEntity Parent { get; private set; }

        public int ID { get; private set; }

        public string Name { get; set; }

        public int Versions { get; private set; }

        public World world { get; private set; }

        public bool IsAction => State == IEntity.EntityState.IsRunning;

        private bool[] componentIds;

        public List<int> ComponentsList { get; private set; }

        public void OnDirty(IEntity parent, int id)
        {
            componentIds = new bool[world.MaxComponentCount];
            ComponentsList = new List<int>(world.MaxComponentCount);
            State = IEntity.EntityState.IsRunning;
            Parent = parent;
            ID = id;
            Versions++;
        }

        public void SetContext(World world)
        {
            this.world = world;
        }


        /// <summary>
        /// 加入组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T* AddComponent<T>() where T : unmanaged, EffComponent
        {
            var cid = ComponentsID<T>.TID;
            if (componentIds[cid])
            {
                var type = typeof(T);
                throw new Exception($"entity already has component: {type.FullName}");
            }

            ComponentsList.Add(cid);
            componentIds[cid] = true;
            world.Reactive(cid, this);
            var t = world.GetCompPtr<T>(ID, cid);
            return t;
        }

        public void AddComponentNoGet<T>() where T : unmanaged, EffComponent
        {
            var cid = ComponentsID<T>.TID;
            if (componentIds[cid])
            {
                var type = typeof(T);
                throw new Exception($"entity already has component: {type.FullName}");
            }

            ComponentsList.Add(cid);
            componentIds[cid] = true;
            world.Reactive(cid, this);
        }


        public void RemoveComponent(int cid)
        {
            var component = componentIds[cid];
            if (!component)
            {
                return;
            }

            ComponentDisposeAction.ComponentDisposeActions[cid](world, ID);
            world.ClearComp(ID, cid);
            componentIds[cid] = false;
            ComponentsList.RemoveSwapBack(cid);
            world.Reactive(cid, this);
        }


        public T GetComponent<T>() where T : unmanaged, EffComponent
        {
            var cid = ComponentsID<T>.TID;
#if UNITY_EDITOR
            if (!componentIds[cid])
            {
                var type = typeof(T);
                throw new Exception($"entity  dont have component: {type.FullName}");
            }
#endif

            var component = world.GetComp<T>(ID, cid);
            return component;
        }

        public T* GetComponentPtr<T>() where T : unmanaged, EffComponent
        {
            var cid = ComponentsID<T>.TID;
#if UNITY_EDITOR
            if (!componentIds[cid])
            {
                var type = typeof(T);
                throw new Exception($"entity  dont have component: {type.FullName}");
            }
#endif
            var component = world.GetCompPtr<T>(ID, cid);
            return component;
        }


        public bool HasComponents(int[] cids)
        {
            for (int index = 0; index < cids.Length; ++index)
            {
                if (!componentIds[cids[index]])
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasComponent<T>() where T : unmanaged, EffComponent
        {
            var cid = ComponentsID<T>.TID;
            return HasComponent(cid);
        }

        public bool HasComponent(int cid)
        {
            return componentIds[cid];
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
                if (componentIds[cids[index]])
                {
                    return true;
                }
            }

            return false;
        }

        private void ClearAllComponent()
        {
            int count = ComponentsList.Count;
            for (int i = 0; i < count; i++)
            {
                ComponentDisposeAction.ComponentDisposeActions[ComponentsList[i]](world, ID);
            }

            world.ClearEntityAllComponent(ID);
            ComponentsList.Clear();
        }

        public void Dispose()
        {
            State = IEntity.EntityState.IsClear;
            Versions++;
            ClearAllComponent();
            componentIds = null;
            ComponentsList = null;
        }
    }
}