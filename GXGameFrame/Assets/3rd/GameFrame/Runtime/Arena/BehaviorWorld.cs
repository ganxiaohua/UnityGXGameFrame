namespace GameFrame
{
    public abstract class BehaviorWorld : IReference
    {
        private BehaviorWorldEntity m_BehaviorWorldEntity;
        public virtual void Init(BehaviorWorldEntity behaviorWorld)
        {
            m_BehaviorWorldEntity = behaviorWorld;
        }

        public virtual void Update(float elapseSeconds)
        {
        }

        internal void ChangeBehavior<T>(IBehaviorData behaviorData)where T : Behavior
        {
            m_BehaviorWorldEntity.ChangeBehavior<T>(behaviorData);
        }

        public void Clear()
        {
            
        }
    }
}