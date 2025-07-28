using System;

namespace GameFrame.Runtime
{
    public abstract class BehaviorWorld : IDisposable
    {
        private BehaviorWorldEntity behaviorWorldEntity;
        public virtual void Init(BehaviorWorldEntity behaviorWorld)
        {
            behaviorWorldEntity = behaviorWorld;
            
        }

        public virtual void Update(float elapseSeconds)
        {
            
        }

        internal void ChangeBehavior<T>(IBehaviorData behaviorData)where T : Behavior
        {
            behaviorWorldEntity.ChangeBehavior<T>(behaviorData);
        }

        public void Dispose()
        {
            
        }
    }
}