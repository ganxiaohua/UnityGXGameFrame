using UnityGXGameFrame.AI.Utility;

namespace UnityGXGameFrame.AI.GOAP
{
    /// <summary>
    /// GOAP行动接口
    /// </summary>
    public interface IGOAPAction
    {
        string Name { get; }
        float Cost { get; }
        GOAPWorldState Preconditions { get; }
        GOAPWorldState Effects { get; }

        /// <summary>
        /// 检查此动作在当前上下文下是否可用
        /// </summary>
        bool CheckProceduralPrecondition(IAIContext context);

        /// <summary>
        /// 执行动作，返回是否执行成功
        /// </summary>
        bool Perform(IAIContext context, float deltaTime);

        /// <summary>
        /// 是否需要在此帧继续执行
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 开始执行动作
        /// </summary>
        void Start();

        /// <summary>
        /// 动作执行完成时的回调
        /// </summary>
        void OnComplete();

        /// <summary>
        /// 动作被取消时的回调
        /// </summary>
        void OnAbort();
    }

    /// <summary>
    /// GOAP基础行动类（可继承使用）
    /// </summary>
    public abstract class GOAPAction : IGOAPAction
    {
        public string Name { get; protected set; }
        public virtual float Cost { get; protected set; } = 1f;
        public GOAPWorldState Preconditions { get; protected set; }
        public GOAPWorldState Effects { get; protected set; }
        public bool IsRunning { get; protected set; }

        protected GOAPAction(string name)
        {
            Name = name;
            Preconditions = GOAPWorldState.Empty;
            Effects = GOAPWorldState.Empty;
        }

        /// <summary>
        /// 添加前提条件
        /// </summary>
        protected void AddPrecondition(string stateName)
        {
            int idx = GOAPWorldStateRegistry.GetIndex(stateName);
            if (idx < 0)
                idx = GOAPWorldStateRegistry.Register(stateName);
            var pre = Preconditions;
            pre.Set(idx, true);
            Preconditions = pre;
        }

        /// <summary>
        /// 添加效果
        /// </summary>
        protected void AddEffect(string stateName)
        {
            int idx = GOAPWorldStateRegistry.GetIndex(stateName);
            if (idx < 0)
                idx = GOAPWorldStateRegistry.Register(stateName);
            var eff = Effects;
            eff.Set(idx, true);
            Effects = eff;
        }

        public virtual bool CheckProceduralPrecondition(IAIContext context)
        {
            return true;
        }

        public abstract bool Perform(IAIContext context, float deltaTime);

        public virtual void OnComplete()
        {
            IsRunning = false;
        }

        public virtual void OnAbort()
        {
            IsRunning = false;
        }

        public void Start()
        {
            IsRunning = true;
        }
    }

    /// <summary>
    /// 即时完成的动作（一帧内执行完毕）
    /// </summary>
    public abstract class GOAPInstantAction : GOAPAction
    {
        protected GOAPInstantAction(string name) : base(name)
        {
        }

        public sealed override bool Perform(IAIContext context, float deltaTime)
        {
            bool result = Execute(context);
            IsRunning = false;
            return result;
        }

        protected abstract bool Execute(IAIContext context);
    }

    /// <summary>
    /// 需要持续执行的动作（多帧）
    /// </summary>
    public abstract class GOAPContinuousAction : GOAPAction
    {
        protected float Duration;
        protected float Timer;

        protected GOAPContinuousAction(string name, float duration) : base(name)
        {
            Duration = duration;
        }

        public sealed override bool Perform(IAIContext context, float deltaTime)
        {
            if (!IsRunning)
            {
                Timer = 0f;
                Start();
            }

            Timer += deltaTime;
            OnUpdate(context, deltaTime);

            if (Timer >= Duration)
            {
                IsRunning = false;
                return OnFinish(context);
            }

            return true;
        }

        protected virtual void OnUpdate(IAIContext context, float deltaTime)
        {
        }

        protected virtual bool OnFinish(IAIContext context)
        {
            return true;
        }
    }
}