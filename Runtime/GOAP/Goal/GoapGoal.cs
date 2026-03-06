using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public abstract partial class GoapGoal : IUpdateSystem
    {
        public IEntity Entity;

        /// <summary>
        /// 需要执行的计划
        /// </summary>
        protected Queue<GoapAction> Plan;


        /// <summary>
        /// 当前
        /// </summary>
        protected GoapAction CurAction;

        /// <summary>
        /// 目标优先级
        /// </summary>
        /// <returns></returns>
        protected abstract float GetPriority();

        /// <summary>
        /// 目标是否可行
        /// </summary>
        /// <returns></returns>
        protected abstract bool IsGoalPossible();

        protected virtual void Init()
        {
            Plan = new Queue<GoapAction>();
        }


        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (CurAction == null)
                return;
            if (CurAction.Success)
                Run();
        }

        public virtual void Run()
        {
            if (Plan.Count == 0)
                return;
            Plan.TryDequeue(out CurAction);
            CurAction.Run();
        }

        public void Dispose()
        {
            Entity?.Dispose();
            CurAction?.Dispose();
        }
    }
}