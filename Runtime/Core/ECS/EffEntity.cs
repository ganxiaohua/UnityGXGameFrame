using System;

namespace GameFrame.Runtime
{
    public partial interface EffComponent : IDisposable
    {
    }

    /// <summary>
    /// ECSEntity挂载的一定是Context
    /// </summary>
    public class EffEntity : IEntity, IVersions
    {
        public IEntity.EntityState State { get; private set; }

        public IEntity Parent { get; private set; }

        public int ID { get; private set; }

        public string Name { get; set; }

        public int Versions { get; private set; }

        public World world { get; private set; }

        public bool IsAction => State == IEntity.EntityState.IsRunning;


        public bool[] ComponentIds;

        public void OnDirty(IEntity parent, int id)
        {
            ComponentIds = new bool[world.MaxComponentCount];
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
        public T AddComponent<T>() where T : unmanaged, EffComponent
        {
            var cid = ComponentsID<T>.TID;
            if (ComponentIds[cid])
            {
                var type = typeof(T);
                throw new Exception($"entity already has component: {type.FullName}");
            }

            ComponentIds[cid] = true;
            world.Reactive(cid, this);
            return world.GetComp<T>(ID, cid);
        }


        public void RemoveComponent(int cid)
        {
            var component = ComponentIds[cid];
            if (!component)
            {
                return;
            }

            world.ClearComp(ID, cid);
            ComponentIds[cid] = false;
            world.Reactive(cid, this);
        }


        public T GetComponent<T>() where T : unmanaged, EffComponent
        {
            var cid = ComponentsID<T>.TID;
            var component = world.GetComp<T>(ID, cid);
            return component;
        }

        public unsafe T* GetComponentPtr<T>() where T : unmanaged, EffComponent
        {
            var cid = ComponentsID<T>.TID;
            var component = world.GetCompPtr<T>(ID, cid);
            return component;
        }


        public bool HasComponents(int[] cids)
        {
            for (int index = 0; index < cids.Length; ++index)
            {
                if (!ComponentIds[cids[index]])
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
            return ComponentIds[cid];
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
                if (ComponentIds[cids[index]])
                {
                    return true;
                }
            }

            return false;
        }

        private void ClearAllComponent()
        {
            world.ClearEntityAllComponent(ID);
        }

        public void Dispose()
        {
            State = IEntity.EntityState.IsClear;
            Versions++;
            ClearAllComponent();
        }
    }
}