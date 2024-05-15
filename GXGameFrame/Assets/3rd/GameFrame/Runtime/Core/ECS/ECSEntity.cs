using System;

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
    public abstract class ECSEntity : IEntity, IStartSystem
    {
        public IEntity.EntityStatus m_EntityStatus { get; set; }

        public IEntity SceneParent { get; set; }

        public IEntity Parent { get; set; }

        public int ID { get; set; }

        public string Name { get; set; }

        private Context m_Context;

        public GXArray<ECSComponent> ECSComponentArray { get; private set; }

        private static int m_SerialId;

        public virtual void Start()
        {
            ECSComponentArray = ReferencePool.Acquire<GXArray<ECSComponent>>();
            ECSComponentArray.Init(GXComponents.ComponentTypes.Length);
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
            m_Context.ChangeAddRomoveChildOrCompone(this);
            return entity;
        }

        /// <summary>
        /// 删除组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="Exception"></exception>
        public void RemoveComponent(int cid)
        {
            Type type = GXComponents.ComponentTypes[cid];
            if (ECSComponentArray[cid] == null)
            {
                throw new Exception($"entity not already  component: {type.FullName}");
            }

            ECSComponentArray.Remove(cid);
            m_Context.ChangeAddRomoveChildOrCompone(this);
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

        public void SetContext(Context context)
        {
            m_Context = context;
        }

        public bool HasComponent(int cid)
        {
            if (ECSComponentArray[cid] == null)
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
            ReferencePool.Release(ECSComponentArray);
            ((Context) Parent).ChangeAddRomoveChildOrCompone(this);
        }

        public void Clear()
        {
            ClearAllComponent();
        }
    }
}