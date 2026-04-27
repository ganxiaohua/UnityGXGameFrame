using System;
using UnityGXGameFrame.AI.GOAP;
using UnityGXGameFrame.AI.Utility;
using UnityEngine;

namespace UnityGXGameFrame.AI.Hybrid
{
    /// <summary>
    /// 由Utility驱动的GOAP目标
    /// 将Utility评分系统与GOAP目标结合
    /// </summary>
    public class UtilityGOAPGoal : GOAPGoal
    {
        /// <summary>
        /// 用于评分此目标的效用考虑因素
        /// </summary>
        public IUtilityConsideration Consideration { get; private set; }

        /// <summary>
        /// 最低激活阈值，低于此值目标不会被选中
        /// </summary>
        public float ActivationThreshold { get; set; } = 0.1f;

        /// <summary>
        /// 优先级乘数（用于调节Utility得分的权重）
        /// </summary>
        public float PriorityMultiplier { get; set; } = 1f;

        /// <summary>
        /// 目标一旦激活后的最低持续时间（防止目标快速切换）
        /// </summary>
        public float MinDuration { get; set; } = 0.5f;

        private float _activeTimer;
        private bool _isActive;

        public UtilityGOAPGoal(string name, IUtilityConsideration consideration) : base(name)
        {
            Consideration = consideration ?? throw new ArgumentNullException(nameof(consideration));
        }

        public override void UpdatePriority(IAIContext context)
        {
            if (Consideration == null)
            {
                Priority = 0f;
                return;
            }

            float utilityScore = Consideration.Evaluate(context);
            Priority = utilityScore * PriorityMultiplier;

            if (_isActive)
            {
                _activeTimer += Time.deltaTime;
            }
        }

        public override bool IsValid(IAIContext context)
        {
            if (_isActive && _activeTimer < MinDuration)
                return true; // 仍在最低持续时间内

            return Priority >= ActivationThreshold;
        }

        public override void OnActivate(IAIContext context)
        {
            base.OnActivate(context);
            _isActive = true;
            _activeTimer = 0f;
        }

        public override void OnDeactivate(IAIContext context)
        {
            base.OnDeactivate(context);
            _isActive = false;
            _activeTimer = 0f;
        }

        /// <summary>
        /// 便捷方法：构建一个基于单个考虑因素的Utility目标
        /// </summary>
        public static UtilityGOAPGoal CreateSimple(string name, string contextKey, 
            float inputMin, float inputMax, UtilityCurveType curveType, float weight = 1f,
            params string[] desiredStates)
        {
            var consideration = new UtilityConsideration
            {
                Name = $"{name}_Consideration",
                ContextKey = contextKey,
                InputMin = inputMin,
                InputMax = inputMax,
                CurveType = curveType,
                Weight = weight
            };

            var goal = new UtilityGOAPGoal(name, consideration);
            foreach (var state in desiredStates)
            {
                goal.AddDesiredState(state);
            }
            return goal;
        }
    }
}
