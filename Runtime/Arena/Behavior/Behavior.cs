using System;
using System.Collections.Generic;

namespace GameFrame
{
    public abstract class Behavior : IDisposable
    {
        private BehaviorWorld behaviorWorld;

        public virtual void Init(BehaviorWorld ar)
        {
            behaviorWorld = ar;
        }

        public virtual void DataJoin(IBehaviorData behaviorData)
        {
        }

        public virtual void Update(List<IBehaviorData> behaviorData, float elapseSeconds)
        {
        }

        public virtual void DataLeave(IBehaviorData behaviorData)
        {
        }

        protected virtual void ChangeBehavior<T>(IBehaviorData behaviorData) where T : Behavior
        {
            behaviorWorld.ChangeBehavior<T>(behaviorData);
        }

        public void Dispose()
        {
        }
    }
}