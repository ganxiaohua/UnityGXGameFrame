using System.Collections.Generic;

namespace GameFrame
{
    public abstract class ReactiveSystem : IStartSystem<Context>, IUpdateSystem
    {
        /// <summary>
        /// 挂载的父实体
        /// </summary>
        protected Context Context;

        private List<ECSEntity> m_Buffer;

        /// <summary>
        /// 我想要关注的实体
        /// </summary>
        public Collector m_Collector;

        public virtual void Start(Context entity)
        {
            m_Buffer = new List<ECSEntity>();
            Context = entity;
            m_Collector = this.GetTrigger(entity);
        }

        protected abstract Collector GetTrigger(Context context);
        
        protected abstract bool Filter(ECSEntity entity);

        protected abstract void Update(List<ECSEntity> entities);

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (m_Collector.CollectedEntities.Count == 0)
                return;
            foreach (ECSEntity collectedEntity in m_Collector.CollectedEntities)
            {
                var ecsentity = collectedEntity;
                if (this.Filter(ecsentity))
                {
                    this.m_Buffer.Add(ecsentity);
                }
            }

            if (this.m_Buffer.Count == 0)
                return;
            Update(this.m_Buffer);
            this.m_Buffer.Clear();
            m_Collector.CollectedEntities.Clear();
        }

        public abstract void Clear();
    }
}