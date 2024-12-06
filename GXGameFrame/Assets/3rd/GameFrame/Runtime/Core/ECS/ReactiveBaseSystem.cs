using System.Collections.Generic;

namespace GameFrame
{
    public abstract class ReactiveBaseSystem : IInitializeSystem<World>
    {
        /// <summary>
        /// 挂载的父实体
        /// </summary>
        protected World World;

        private List<ECSEntity> buffer;

        /// <summary>
        /// 我想要关注的实体
        /// </summary>
        private Collector collector;

        public virtual void OnInitialize(World entity)
        {
            buffer = new List<ECSEntity>();
            World = entity;
            collector = this.GetTrigger(entity);
        }

        protected abstract Collector GetTrigger(World world);

        protected abstract bool Filter(ECSEntity entity);

        protected abstract void Execute(List<ECSEntity> entities);

        protected void Do(float elapseSeconds, float realElapseSeconds)
        {
            if (collector.CollectedEntities.Count == 0)
                return;

            foreach (ECSEntity ecsentity in collector.CollectedEntities)
            {
                if (ecsentity.State != IEntity.EntityState.IsClear && this.Filter(ecsentity))
                {
                    buffer.Add(ecsentity);
                }
            }
            collector.CollectedEntities.Clear();
            if (buffer.Count == 0)
                return;
            Execute(buffer);
            buffer.Clear();
        }

        public abstract void Dispose();
    }
}