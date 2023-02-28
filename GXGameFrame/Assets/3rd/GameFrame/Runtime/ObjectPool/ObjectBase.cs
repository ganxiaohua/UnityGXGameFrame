using System;

namespace GameFrame
{
    public abstract class ObjectBase : IReference
    {
        protected object m_InitData;
        /// <summary>
        /// 初始化对象基类。
        /// </summary>
        internal virtual void Initialize(object initData)
        {
            m_InitData = initData;
        }


        /// <summary>
        /// 获取对象时的事件。
        /// </summary>
        internal virtual void OnSpawn()
        {
        }

        /// <summary>
        /// 回收对象时的事件。
        /// </summary>
        internal virtual void OnUnspawn()
        {
        }

        /// <summary>
        /// 清理对象基类。
        /// </summary>
        public virtual void Clear()
        {
        }
    }
}