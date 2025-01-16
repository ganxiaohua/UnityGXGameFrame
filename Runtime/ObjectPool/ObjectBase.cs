using System;

namespace GameFrame
{
    public class ObjectBase : IDisposable
    {
        public float ExpiredTime { get; internal set; }

        public bool Luck { get; set; }

        /// <summary>
        /// 初始化对象基类。
        /// </summary>
        public virtual void Initialize(object initData)
        {
            
        }


        /// <summary>
        /// 获取对象时的事件。
        /// </summary>
        public virtual void OnSpawn()
        {
            
        }

        /// <summary>
        /// 回收对象时的事件。
        /// </summary>
        public virtual void OnUnspawn()
        {
            
        }

        public virtual void Dispose()
        {
            ExpiredTime = 0;
            Luck = false;
        }
    }
}