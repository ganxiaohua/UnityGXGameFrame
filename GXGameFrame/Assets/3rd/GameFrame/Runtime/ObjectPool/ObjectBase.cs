using System;

namespace GameFrame
{
    public class ObjectBase : IReference
    {
        private DateTime m_LastUseTime;
        private bool m_Luck;
        
        public DateTime LastUseTime
        {
            get
            {
                return m_LastUseTime;
            }
            internal set
            {
                m_LastUseTime = value;
            }
        }
        
        public bool Luck
        {
            get
            {
                return m_Luck;
            }
             set
            {
                m_Luck = value;
            }
        }
        /// <summary>
        /// 初始化对象基类。
        /// </summary>
        internal virtual void Initialize(object initData)
        {
            
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

        public virtual void Clear()
        {
            m_LastUseTime = default(DateTime);
            Luck = false;
        }
    }
}