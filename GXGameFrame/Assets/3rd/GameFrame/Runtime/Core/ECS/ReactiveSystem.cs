using System.Collections.Generic;

namespace GameFrame
{
    public abstract class ReactiveSystem : IECSInitSystem, IECSUpdateSystem
    {
        /// <summary>
        /// 挂载父实体
        /// </summary>
        protected Context Context;

        private List<ECSEntity> m_Buffer;

        /// <summary>
        /// 我想要关注的实体
        /// </summary>
        private Collector m_Collector;

        // private = new List<ECSEntity>();
        public virtual void Initialize(Context entity)
        {
            m_Buffer = new List<ECSEntity>();
            Context = entity;
            m_Collector = this.GetTrigger(entity);
        }

        protected abstract Collector GetTrigger(Context context);
        protected abstract bool Filter(ECSEntity entity);

        protected abstract void Update(List<ECSEntity> entities);

        public void Update()
        {
            foreach (ECSEntity collectedEntity in this.m_Collector.CollectedEntities)
            {
                var ecsentity = collectedEntity as ECSEntity;
                if (this.Filter(ecsentity))
                {
                    this.m_Buffer.Add(ecsentity);
                }
            }

            if (this.m_Buffer.Count == 0)
                return;
            Update(this.m_Buffer);
            this.m_Buffer.Clear();
        }

        public abstract void Clear();
    }
}