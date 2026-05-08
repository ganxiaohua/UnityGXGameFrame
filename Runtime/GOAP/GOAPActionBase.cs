using System;

namespace GameFrame.Runtime
{
    public class GOAPActionBase : IDisposable
    {
        public float Cost;
        public GOAPState Preconditions;
        public GOAPState Effects;
        public GOAPAgent Agent { get; set; }
        public bool IsRunning { get; protected set; }

        public virtual bool CheckProceduralPrecondition()
        {
            return Agent.WorldState.Satisfies(Preconditions);
        }

        public void Update(float deltaTime)
        {
            IsRunning = OnExecute(deltaTime);
        }

        public virtual void OnEnter()
        {
        }

        public virtual bool OnExecute(float deltaTime)
        {
            return true;
        }

        public virtual void OnExit()
        {
        }

        public virtual void Dispose()
        {
            IsRunning = false;
        }
    }
}