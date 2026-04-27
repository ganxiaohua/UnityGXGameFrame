using UnityGXGameFrame.AI.Utility;

namespace UnityGXGameFrame.AI.GOAP
{
    /// <summary>
    /// GOAP目标接口
    /// </summary>
    public interface IGOAPGoal
    {
        string Name { get; }

        /// <summary>
        /// 目标期望达到的世界状态
        /// </summary>
        GOAPWorldState DesiredState { get; }

        /// <summary>
        /// 目标优先级（动态计算）
        /// </summary>
        float Priority { get; }

        /// <summary>
        /// 目标是否仍然有效
        /// </summary>
        bool IsValid(IAIContext context);

        /// <summary>
        /// 更新优先级（每帧调用）
        /// </summary>
        void UpdatePriority(IAIContext context);

        /// <summary>
        /// 当目标被选中时调用
        /// </summary>
        void OnActivate(IAIContext context);

        /// <summary>
        /// 当目标完成或取消时调用
        /// </summary>
        void OnDeactivate(IAIContext context);
    }

    /// <summary>
    /// GOAP基础目标类
    /// </summary>
    public abstract class GOAPGoal : IGOAPGoal
    {
        public string Name { get; protected set; }
        public GOAPWorldState DesiredState { get; protected set; }
        public float Priority { get; protected set; }

        protected GOAPGoal(string name)
        {
            Name = name;
            DesiredState = GOAPWorldState.Empty;
        }

        /// <summary>
        /// 添加期望状态
        /// </summary>
        protected void AddDesiredState(string stateName)
        {
            int idx = GOAPWorldStateRegistry.GetIndex(stateName);
            if (idx < 0)
                idx = GOAPWorldStateRegistry.Register(stateName);
            var state = DesiredState;
            state.Set(idx, true);
            DesiredState = state;
        }

        public virtual bool IsValid(IAIContext context)
        {
            return true;
        }

        public abstract void UpdatePriority(IAIContext context);

        public virtual void OnActivate(IAIContext context)
        {
        }

        public virtual void OnDeactivate(IAIContext context)
        {
        }
    }
}