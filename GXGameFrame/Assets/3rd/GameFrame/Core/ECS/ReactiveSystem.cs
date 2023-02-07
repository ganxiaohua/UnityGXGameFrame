using System.Collections.Generic;

namespace GameFrame
{
    public abstract class ReactiveSystem:IECSInitSystem,IECSUpdateSystem
    {
        /// <summary>
        /// 挂载父实体
        /// </summary>
        protected Context Context;

        /// <summary>
        /// 我想要关注的实体
        /// </summary>
        private HashSet<int> m_Collector;
            // private = new List<ECSEntity>();
        public virtual void Initialize(Context entity)
        {
            Context = entity;
            m_Collector = this.GetTrigger(entity);
        }
        protected abstract HashSet<int> GetTrigger(Context context);
        
        protected abstract void Update(List<ECSEntity> entities);
        
        public void Update()
        {
           //TODO:将过滤的实体update,同时考虑如果在运行update的时候,突然有新的实体生成,加入m_Collector的问题
        }

        public abstract void Clear();
    }
}