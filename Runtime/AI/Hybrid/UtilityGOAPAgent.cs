using System;
using System.Collections.Generic;
using System.Linq;
using UnityGXGameFrame.AI.GOAP;
using UnityGXGameFrame.AI.Utility;
using UnityEngine;

namespace UnityGXGameFrame.AI.Hybrid
{
    /// <summary>
    /// Utility + GOAP 混合AI代理
    /// 
    /// 工作原理：
    /// 1. Utility层：每帧评估所有Goal的优先级（由UtilityConsideration驱动）
    /// 2. GOAP层：为优先级最高的有效Goal制定行动计划
    /// 3. 执行层：逐帧执行计划中的动作
    /// </summary>
    public class UtilityGOAPAgent
    {
        public string Name { get; set; }
        
        /// <summary>
        /// 当前活跃的目标
        /// </summary>
        public IGOAPGoal CurrentGoal { get; private set; }
        
        /// <summary>
        /// 当前执行的计划
        /// </summary>
        public GOAPPlan CurrentPlan { get; private set; }
        
        /// <summary>
        /// 当前正在执行的动作索引
        /// </summary>
        public int CurrentActionIndex { get; private set; }

        /// <summary>
        /// AI运行时上下文
        /// </summary>
        public IAIContext Context { get; private set; }

        /// <summary>
        /// 可用目标列表
        /// </summary>
        private readonly List<IGOAPGoal> _goals = new();

        /// <summary>
        /// 可用动作列表
        /// </summary>
        private readonly List<IGOAPAction> _actions = new();

        /// <summary>
        /// GOAP规划器
        /// </summary>
        private readonly GOAPPlanner _planner = new();

        /// <summary>
        /// 当前世界状态
        /// </summary>
        public GOAPWorldState WorldState { get; private set; } = GOAPWorldState.Empty;

        /// <summary>
        /// 重新规划间隔（秒）
        /// </summary>
        public float ReplanInterval { get; set; } = 0.5f;

        /// <summary>
        /// 目标切换冷却（秒）
        /// </summary>
        public float GoalSwitchCooldown { get; set; } = 1f;

        /// <summary>
        /// 调试模式
        /// </summary>
        public bool DebugMode { get; set; }

        private float _replanTimer;
        private float _goalSwitchTimer;
        private bool _isExecuting;

        // 事件
        public event Action<IGOAPGoal> OnGoalChanged;
        public event Action<IGOAPAction> OnActionStarted;
        public event Action<IGOAPAction, bool> OnActionFinished;
        public event Action<GOAPPlan> OnPlanComplete;

        public UtilityGOAPAgent(string name, IAIContext context = null)
        {
            Name = name;
            Context = context ?? new SimpleAIContext();
        }

        /// <summary>
        /// 添加目标
        /// </summary>
        public void AddGoal(IGOAPGoal goal)
        {
            if (goal == null) throw new ArgumentNullException(nameof(goal));
            if (!_goals.Contains(goal))
                _goals.Add(goal);
        }

        /// <summary>
        /// 移除目标
        /// </summary>
        public void RemoveGoal(IGOAPGoal goal)
        {
            _goals.Remove(goal);
        }

        /// <summary>
        /// 添加动作
        /// </summary>
        public void AddAction(IGOAPAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (!_actions.Contains(action))
                _actions.Add(action);
        }

        /// <summary>
        /// 移除动作
        /// </summary>
        public void RemoveAction(IGOAPAction action)
        {
            _actions.Remove(action);
        }

        /// <summary>
        /// 设置世界状态
        /// </summary>
        public void SetWorldState(GOAPWorldState state)
        {
            WorldState = state;
        }

        /// <summary>
        /// 更新世界状态的某个位
        /// </summary>
        public void SetWorldState(int index, bool value)
        {
            var state = WorldState;
            state.Set(index, value);
            WorldState = state;
        }

        /// <summary>
        /// 主更新入口，每帧调用
        /// </summary>
        public void Update(float deltaTime)
        {
            // 1. 更新所有目标的优先级
            foreach (var goal in _goals)
            {
                goal.UpdatePriority(Context);
            }

            // 2. 选择最佳目标
            var bestGoal = SelectBestGoal();
            
            // 3. 检查是否需要切换目标或重新规划
            if (bestGoal != null && (CurrentGoal == null || bestGoal != CurrentGoal))
            {
                _goalSwitchTimer += deltaTime;
                if (_goalSwitchTimer >= GoalSwitchCooldown)
                {
                    SwitchGoal(bestGoal);
                    _goalSwitchTimer = 0f;
                }
            }
            else
            {
                _goalSwitchTimer = 0f;
            }

            // 4. 执行当前计划
            if (CurrentGoal != null && CurrentPlan != null && CurrentPlan.IsValid)
            {
                _replanTimer += deltaTime;
                
                // 定期重新规划
                if (_replanTimer >= ReplanInterval)
                {
                    _replanTimer = 0f;
                    Replan();
                }

                ExecutePlan(deltaTime);
            }
            else if (CurrentGoal != null)
            {
                // 有目标但没有有效计划，尝试规划
                _replanTimer += deltaTime;
                if (_replanTimer >= ReplanInterval)
                {
                    _replanTimer = 0f;
                    Replan();
                }
            }
        }

        /// <summary>
        /// 选择优先级最高的有效目标
        /// </summary>
        private IGOAPGoal SelectBestGoal()
        {
            IGOAPGoal best = null;
            float bestPriority = float.MinValue;

            foreach (var goal in _goals)
            {
                if (!goal.IsValid(Context))
                    continue;

                if (goal.Priority > bestPriority)
                {
                    bestPriority = goal.Priority;
                    best = goal;
                }
            }

            return best;
        }

        /// <summary>
        /// 切换目标
        /// </summary>
        private void SwitchGoal(IGOAPGoal newGoal)
        {
            if (CurrentGoal != null)
            {
                // 取消当前动作
                if (CurrentPlan != null && CurrentPlan.IsValid && CurrentActionIndex < CurrentPlan.Actions.Count)
                {
                    CurrentPlan.Actions[CurrentActionIndex].OnAbort();
                }
                CurrentGoal.OnDeactivate(Context);
                if (DebugMode)
                    Debug.Log($"[{Name}] Goal deactivated: {CurrentGoal.Name} (Priority: {CurrentGoal.Priority:F2})");
            }

            CurrentGoal = newGoal;
            CurrentGoal.OnActivate(Context);
            OnGoalChanged?.Invoke(CurrentGoal);
            
            if (DebugMode)
                Debug.Log($"[{Name}] Goal activated: {CurrentGoal.Name} (Priority: {CurrentGoal.Priority:F2})");

            // 立即重新规划
            Replan();
        }

        /// <summary>
        /// 为当前目标制定计划
        /// </summary>
        private void Replan()
        {
            if (CurrentGoal == null)
                return;

            CurrentPlan = _planner.Plan(Context, WorldState, CurrentGoal, _actions);
            CurrentActionIndex = 0;

            if (DebugMode)
            {
                if (CurrentPlan.IsValid)
                {
                    string actions = string.Join(" -> ", CurrentPlan.Actions.Select(a => a.Name));
                    Debug.Log($"[{Name}] Plan created for '{CurrentGoal.Name}': [{actions}] (Cost: {CurrentPlan.TotalCost:F2})");
                }
                else
                {
                    Debug.LogWarning($"[{Name}] Failed to create plan for '{CurrentGoal.Name}'");
                }
            }
        }

        /// <summary>
        /// 执行计划
        /// </summary>
        private void ExecutePlan(float deltaTime)
        {
            if (!CurrentPlan.IsValid || CurrentActionIndex >= CurrentPlan.Actions.Count)
            {
                // 计划完成
                if (CurrentPlan.IsValid)
                {
                    OnPlanComplete?.Invoke(CurrentPlan);
                    if (DebugMode)
                        Debug.Log($"[{Name}] Plan completed for '{CurrentGoal.Name}'");
                }
                
                CurrentPlan = null;
                CurrentActionIndex = 0;
                return;
            }

            var action = CurrentPlan.Actions[CurrentActionIndex];

            // 检查动作前提条件
            if (!action.CheckProceduralPrecondition(Context))
            {
                if (DebugMode)
                    Debug.LogWarning($"[{Name}] Action '{action.Name}' precondition failed, replanning...");
                Replan();
                return;
            }

            if (!action.IsRunning)
            {
                action.Start();
                OnActionStarted?.Invoke(action);
                if (DebugMode)
                    Debug.Log($"[{Name}] Action started: {action.Name}");
            }

            bool success = action.Perform(Context, deltaTime);

            if (!success)
            {
                // 动作执行失败，重新规划
                action.OnAbort();
                OnActionFinished?.Invoke(action, false);
                if (DebugMode)
                    Debug.LogWarning($"[{Name}] Action failed: {action.Name}, replanning...");
                Replan();
                return;
            }

            if (!action.IsRunning)
            {
                // 动作完成
                action.OnComplete();
                OnActionFinished?.Invoke(action, true);
                
                // 应用动作效果到世界状态
                var newState = WorldState;
                // 注意：WorldState的Apply方法需要按位或
                newState = ApplyEffects(newState, action.Effects);
                WorldState = newState;

                if (DebugMode)
                    Debug.Log($"[{Name}] Action completed: {action.Name}");

                CurrentActionIndex++;
            }
        }

        /// <summary>
        /// 手动应用效果到世界状态
        /// </summary>
        private GOAPWorldState ApplyEffects(GOAPWorldState state, GOAPWorldState effects)
        {
            // 由于WorldState.Apply是struct的方法，我们模拟它的行为
            var result = state;
            for (int i = 0; i < 128; i++)
            {
                if (effects.Get(i))
                    result.Set(i, true);
            }
            return result;
        }

        /// <summary>
        /// 强制中断当前计划
        /// </summary>
        public void Abort()
        {
            if (CurrentPlan != null && CurrentPlan.IsValid && CurrentActionIndex < CurrentPlan.Actions.Count)
            {
                CurrentPlan.Actions[CurrentActionIndex].OnAbort();
            }
            CurrentPlan = null;
            CurrentActionIndex = 0;
        }

        /// <summary>
        /// 清空所有目标和动作
        /// </summary>
        public void Clear()
        {
            Abort();
            CurrentGoal?.OnDeactivate(Context);
            CurrentGoal = null;
            _goals.Clear();
            _actions.Clear();
        }
    }
}
