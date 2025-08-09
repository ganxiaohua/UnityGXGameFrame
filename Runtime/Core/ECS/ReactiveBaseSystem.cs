using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Runtime
{
    public abstract class ReactiveBaseSystem : SimpleEntity, IInitializeSystem<World>
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

        private List<EffEntity> effEntities = new(64);

        protected abstract Collector GetTrigger(World world);

        protected abstract bool Filter(EffEntity entity);

        protected abstract void Execute(List<EffEntity> entities);

        protected void Do()
        {
            if (collector.CollectedEntities.Count == 0)
                return;
            foreach (EffEntity ecsentity in collector.CollectedEntities)
            {
                if (ecsentity.State != IEntity.EntityState.IsClear && Filter(ecsentity))
                {
                    effEntities.Add(ecsentity);
                }
            }

            collector.CollectedEntities.Clear();
            int count = effEntities.Count;
            Execute(effEntities);
            effEntities.Clear();
        }
    }
}