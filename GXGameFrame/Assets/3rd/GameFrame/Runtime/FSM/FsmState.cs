namespace GameFrame
{
    public abstract class FsmState : Entity
    {
        public virtual void OnEnter(FsmController fsmController)
        {
            
        }

        public virtual void OnExit()
        {
        }

        public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        public override void Dispose()
        {
        }
    }
}