using System;

namespace GameFrame.Runtime
{
    public abstract class GoapAction : IDisposable, IUpdateSystem
    {
        private GoapActionState state;

        public bool Success => state == GoapActionState.Success;

        public bool Finish => state != GoapActionState.Running;

        protected IEntity Entity;

        /// <summary>
        /// 前置条件
        /// </summary>
        protected GoapState Preconditions;

        /// <summary>
        /// 执行这个行为之后改变的数据。
        /// </summary>
        protected GoapState Effect;

        public abstract void Init();

        public abstract bool CheckProceduralCondition();

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        protected virtual void Enter()
        {
            state = GoapActionState.Running;
        }

        public virtual void Run()
        {
            state = GoapActionState.Running;
        }

        protected virtual void RunSuccExit()
        {
            state = GoapActionState.Success;
        }

        protected virtual void RunFailExit()
        {
            state = GoapActionState.Fail;
        }

        public abstract void Clear();

        public virtual void Dispose()
        {
            Entity?.Dispose();
            Preconditions?.Dispose();
            Effect?.Dispose();
            state = GoapActionState.None;
        }
    }
}