namespace GameFrame.Runtime
{
    public class GOAPActionBase
    {
        public float Cost;

        public GOAPState Preconditions;

        public GOAPState Effects;

        public bool IsRunning { get; protected set; }

        public virtual bool CheckProceduralPrecondition()
        {
            return true;
        }

        public virtual void Update(float deltaTime)
        {
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual void OnAbort()
        {
        }
    }
}