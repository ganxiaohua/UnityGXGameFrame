namespace GameFrame
{
    public abstract class FsmState : Entity
    {
        private FsmController fsmController;

        public virtual void OnEnter(FsmController fsmController)
        {
            this.fsmController = fsmController;
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

        protected void ChangeState<T>() where T : FsmState
        {
            fsmController.ChangeState<T>();
        }

        protected void SetData(string key, object data)
        {
            fsmController.SetData(key, data);
        }

        protected object GetData(string key)
        {
            return fsmController.GetData(key);
        }

        protected void Remove(string key)
        {
            fsmController.RemoveData(key);
        }
    }
}