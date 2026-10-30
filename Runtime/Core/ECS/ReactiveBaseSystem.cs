using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public abstract class ReactiveBaseSystem : SimpleEntity, IInitializeSystem
    {
        /// <summary>
        /// 我想要关注的实体
        /// </summary>
        private Collector collector;
        protected World World { get; private set; }

        private List<EffEntity> effEntities = new(64);

        public virtual void OnInitialize()
        {
            World = ((World)Parent);
            collector = this.GetTrigger((World) Parent);
        }

        protected abstract Collector GetTrigger(World world);

        protected abstract bool Filter(EffEntity entity);

        protected abstract void Execute(List<EffEntity> entities);

        protected void Do()
        {
            if (collector.CollectedEntities.Count == 0)
                return;
            foreach (EffEntity ecsentity in collector.CollectedEntities)
            {
                if (World.IsAlive(ecsentity) && Filter(ecsentity))
                {
                    effEntities.Add(ecsentity);
                }
            }

            collector.CollectedEntities.Clear();
            int count = effEntities.Count;
            Execute(effEntities);
            effEntities.Clear();
        }

        public override void Dispose()
        {
            base.Dispose();
            collector?.Dispose();
            World = default;
        }
    }
}