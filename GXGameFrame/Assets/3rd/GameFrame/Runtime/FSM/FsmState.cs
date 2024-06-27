namespace GameFrame
{
    public abstract class FsmState : Entity
    {
        public virtual void Enter(FsmController fsmController)
        {
            
        }

        public virtual void Leave()
        {
        }

        public virtual void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public override void Clear()
        {
        }
    }
}