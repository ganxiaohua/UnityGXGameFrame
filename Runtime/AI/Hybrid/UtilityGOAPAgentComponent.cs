using UnityEngine;
using UnityGXGameFrame.AI.Utility;
using UnityGXGameFrame.AI.GOAP;
using System.Collections.Generic;

namespace UnityGXGameFrame.AI.Hybrid
{
    /// <summary>
    /// Utility + GOAP 混合AI代理的Unity组件封装
    /// </summary>
    public class UtilityGOAPAgentComponent : MonoBehaviour
    {
        [Header("Agent Settings")]
        [Tooltip("代理名称")]
        public string AgentName = "AI_Agent";

        [Tooltip("重新规划间隔（秒）")]
        public float ReplanInterval = 0.5f;

        [Tooltip("目标切换冷却（秒）")]
        public float GoalSwitchCooldown = 1f;

        [Header("Debug")]
        [Tooltip("启用调试日志")]
        public bool DebugMode;

        [Tooltip("在Scene视图中显示调试信息")]
        public bool ShowDebugInScene = true;

        /// <summary>
        /// 底层代理实例
        /// </summary>
        public UtilityGOAPAgent Agent { get; private set; }

        /// <summary>
        /// AI上下文（可以在外部修改以影响AI决策）
        /// </summary>
        public IAIContext Context => Agent?.Context;

        private void Awake()
        {
            Agent = new UtilityGOAPAgent(AgentName);
            Agent.ReplanInterval = ReplanInterval;
            Agent.GoalSwitchCooldown = GoalSwitchCooldown;
            Agent.DebugMode = DebugMode;
        }

        private void Update()
        {
            if (Agent != null)
            {
                Agent.DebugMode = DebugMode;
                Agent.Update(Time.deltaTime);
            }
        }

        /// <summary>
        /// 注册一个目标
        /// </summary>
        public void RegisterGoal(IGOAPGoal goal)
        {
            Agent?.AddGoal(goal);
        }

        /// <summary>
        /// 注册一个动作
        /// </summary>
        public void RegisterAction(IGOAPAction action)
        {
            Agent?.AddAction(action);
        }

        /// <summary>
        /// 设置世界状态
        /// </summary>
        public void SetWorldState(GOAPWorldState state)
        {
            Agent?.SetWorldState(state);
        }

        /// <summary>
        /// 设置世界状态的某个位
        /// </summary>
        public void SetWorldState(string stateName, bool value)
        {
            if (Agent == null) return;
            int idx = GOAPWorldStateRegistry.GetIndex(stateName);
            if (idx >= 0)
                Agent.SetWorldState(idx, value);
        }

        /// <summary>
        /// 设置上下文值
        /// </summary>
        public void SetContextValue<T>(string key, T value)
        {
            Context?.Set(key, value);
        }

        /// <summary>
        /// 获取上下文值
        /// </summary>
        public T GetContextValue<T>(string key)
        {
            return Context != null ? Context.Get<T>(key) : default;
        }

        private void OnDrawGizmos()
        {
            if (!ShowDebugInScene || Agent == null) return;

            string debugText = $"Agent: {AgentName}\n";
            debugText += $"Goal: {Agent.CurrentGoal?.Name ?? "None"}\n";
            debugText += $"Goal Priority: {Agent.CurrentGoal?.Priority.ToString("F2") ?? "N/A"}\n";
            debugText += $"Plan Valid: {Agent.CurrentPlan?.IsValid.ToString() ?? "N/A"}\n";
            debugText += $"Action Index: {Agent.CurrentActionIndex}\n";
            if (Agent.CurrentPlan != null && Agent.CurrentPlan.IsValid && Agent.CurrentActionIndex < Agent.CurrentPlan.Actions.Count)
            {
                debugText += $"Current Action: {Agent.CurrentPlan.Actions[Agent.CurrentActionIndex].Name}\n";
            }

            // 在Scene视图中显示调试信息
            UnityEditor.Handles.BeginGUI();
            Vector3 pos = transform.position + Vector3.up * 2f;
            Vector2 screenPos = UnityEditor.HandleUtility.WorldToGUIPoint(pos);
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.UpperLeft;
            GUI.backgroundColor = new Color(0, 0, 0, 0.7f);
            UnityEditor.Handles.EndGUI();

            // 使用Gizmos绘制文本信息
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
