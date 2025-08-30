using System;

namespace GameFrame.Runtime
{
    public class ObjectBase : IDisposable, IVersions
    {
        public IObjectPoolBase Pool { get; internal set; }
        public float ExpiredTime { get; internal set; }

        public bool Luck { get; set; }

        public int Versions { get; protected set; }


        /// <summary>
        /// 初始化对象基类。
        /// </summary>
        public virtual void Initialize(object initData)
        {
        }


        /// <summary>
        /// 获取对象时的事件。
        /// </summary>
        public virtual void OnSpawn(object obj)
        {
            Versions++;
        }

        /// <summary>
        /// 回收对象时的事件。
        /// </summary>
        public virtual void OnUnspawn()
        {
            Versions++;
        }

        public virtual void Dispose()
        {
            Versions++;
            ExpiredTime = 0;
            Luck = false;
            Pool = null;
        }
    }
}