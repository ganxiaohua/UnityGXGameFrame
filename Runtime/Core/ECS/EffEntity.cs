using System;

namespace GameFrame.Runtime
{
    [Serializable]
    public partial class EffComponent : IDisposable
    {
        public EffEntity Owner;

        public virtual void Dispose()
        {
        }
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

        private World world;

        public bool IsAction => State == IEntity.EntityState.IsRunning;

        public JumpIndexArrayEx<EffComponent> ecsComponentArrayEx { get; private set; }

        public void OnDirty(IEntity parent, int id)
        {
            ecsComponentArrayEx = ReferencePool.Acquire<JumpIndexArrayEx<EffComponent>>();
            ecsComponentArrayEx.Init(world.MaxComponentCount);
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
        public T AddComponent<T>() where T : EffComponent
        {
            var cid = ComponentsID<T>.TID;
            if (ecsComponentArrayEx[cid] != null)
            {
                var type = typeof(T);
                throw new Exception($"entity already has component: {type.FullName}");
            }
            T entity = ecsComponentArrayEx.Add<T>(cid);
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
            var component = ecsComponentArrayEx[cid];
            if (component == null)
            {
                return;
            }

            component.Owner = null;
            ecsComponentArrayEx.Remove(cid);
            world.Reactive(cid, this);
        }

        /// <summary>
        /// 获得组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public EffComponent GetComponent(int cid)
        {
            var component = ecsComponentArrayEx[cid];
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
                if (ecsComponentArrayEx[cids[index]] == null)
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
            return ecsComponentArrayEx[cid] != null;
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
                if (ecsComponentArrayEx[cids[index]] != null)
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
            var list = ecsComponentArrayEx.IndexList;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                RemoveComponent(list[i]);
            }

            ReferencePool.Release(ecsComponentArrayEx);
        }

        public void Dispose()
        {
            State = IEntity.EntityState.IsClear;
            Versions++;
            ClearAllComponent();
        }
    }
}