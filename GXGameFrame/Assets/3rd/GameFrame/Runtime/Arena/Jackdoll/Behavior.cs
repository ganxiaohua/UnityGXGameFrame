using System.Collections.Generic;

namespace GameFrame
{
    public abstract class Behavior : IReference
    {
        private BehaviorWorld m_BehaviorWorld;

        public virtual void Init(BehaviorWorld ar)
        {
            m_BehaviorWorld = ar;
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
            m_BehaviorWorld.ChangeBehavior<T>(behaviorData);
        }

        public void Clear()
        {
        }
    }
}