using System.Collections.Generic;

namespace GameFrame
{
    public abstract class ReactiveSystem : IStartSystem<World>, IUpdateSystem
    {
        /// <summary>
        /// 挂载的父实体
        /// </summary>
        protected World World;

        private List<ECSEntity> m_Buffer;

        /// <summary>
        /// 我想要关注的实体
        /// </summary>
        private Collector m_Collector;

        public virtual void Start(World entity)
        {
            m_Buffer = new List<ECSEntity>();
            World = entity;
            m_Collector = this.GetTrigger(entity);
        }

        protected abstract Collector GetTrigger(World world);
        
        protected abstract bool Filter(ECSEntity entity);

        protected abstract void Execute(List<ECSEntity> entities);

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (m_Collector.CollectedEntities.Count == 0)
                return;
            
            foreach (ECSEntity ecsentity in m_Collector.CollectedEntities)
            {
                if (ecsentity.State != IEntity.EntityState.IsClear && this.Filter(ecsentity))
                {
                    this.m_Buffer.Add(ecsentity);
                }
            }

            if (this.m_Buffer.Count == 0)
                return;
            Execute(this.m_Buffer);
            this.m_Buffer.Clear();
            m_Collector.CollectedEntities.Clear();
        }

        public abstract void Clear();
    }
}