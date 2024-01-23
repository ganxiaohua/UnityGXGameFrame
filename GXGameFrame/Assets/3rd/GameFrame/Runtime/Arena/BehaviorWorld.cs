namespace GameFrame
{
    public abstract class BehaviorWorld : IReference
    {
        private BehaviorWorldEnitiy m_BehaviorWorldEnitiy;
        public virtual void Init(BehaviorWorldEnitiy behaviorWorld)
        {
            m_BehaviorWorldEnitiy = behaviorWorld;
        }

        public virtual void Update(float elapseSeconds)
        {
        }

        internal void ChangeBehavior<T>(IBehaviorData behaviorData)where T : Behavior
        {
            m_BehaviorWorldEnitiy.ChangeBehavior<T>(behaviorData);
        }

        public void Clear()
        {
        }
    }
}