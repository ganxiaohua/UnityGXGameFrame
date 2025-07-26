using System.Collections.Generic;

namespace GameFrame
{
    public abstract class ReactiveBaseSystem : IInitializeSystem<World>
    {
        /// <summary>
        /// 挂载的父实体
        /// </summary>
        protected World World;

        /// <summary>
        /// 我想要关注的实体
        /// </summary>
        private Collector collector;

        public virtual void OnInitialize(World world)
        {
            World = world;
            collector = this.GetTrigger(world);
        }

        protected abstract Collector GetTrigger(World world);

        protected abstract bool Filter(ECSEntity entity);

        protected abstract void Execute(ECSEntity entities);
        
        protected void Do()
        {
            if (collector.CollectedEntities.Count == 0)
                return;

            foreach (ECSEntity ecsentity in collector.CollectedEntities)
            {
                if (ecsentity.State != IEntity.EntityState.IsClear && this.Filter(ecsentity))
                {
                    Execute(ecsentity);
                }
            }
            collector.CollectedEntities.Clear();
        }

        public abstract void Dispose();
    }
}