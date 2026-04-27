using System.Collections.Generic;
using System.Linq;
using UnityGXGameFrame.AI.Utility;

namespace UnityGXGameFrame.AI.GOAP
{
    /// <summary>
    /// 规划节点（用于A*搜索）
    /// </summary>
    internal class PlanNode
    {
        public GOAPWorldState State;
        public IGOAPAction Action; // 到达此状态所执行的动作（根节点为null）
        public PlanNode Parent;
        public float Cost; // 从起始到此的累积代价（g）
        public float Heuristic; // 到目标的估计代价（h）
        public float TotalCost => Cost + Heuristic; // f = g + h

        public PlanNode(GOAPWorldState state, IGOAPAction action, PlanNode parent, float cost, float heuristic)
        {
            State = state;
            Action = action;
            Parent = parent;
            Cost = cost;
            Heuristic = heuristic;
        }
    }

    /// <summary>
    /// GOAP计划结果
    /// </summary>
    public class GOAPPlan
    {
        public IGOAPGoal Goal { get; }
        public List<IGOAPAction> Actions { get; }
        public float TotalCost { get; }
        public bool IsValid => Actions != null && Actions.Count > 0;

        public GOAPPlan(IGOAPGoal goal, List<IGOAPAction> actions, float totalCost)
        {
            Goal = goal;
            Actions = actions;
            TotalCost = totalCost;
        }

        public static GOAPPlan Invalid(IGOAPGoal goal)
        {
            return new GOAPPlan(goal, new List<IGOAPAction>(), float.MaxValue);
        }
    }

    /// <summary>
    /// GOAP反向A*规划器
    /// </summary>
    public class GOAPPlanner
    {
        /// <summary>
        /// 最大搜索节点数，防止无限搜索
        /// </summary>
        public int MaxSearchNodes = 1000;

        /// <summary>
        /// 规划计划：从当前世界状态出发，找到到达目标状态的动作序列
        /// </summary>
        public GOAPPlan Plan(IAIContext context, GOAPWorldState currentState, IGOAPGoal goal, List<IGOAPAction> availableActions)
        {
            if (goal.DesiredState == GOAPWorldState.Empty)
                return GOAPPlan.Invalid(goal);

            // 如果当前状态已经满足目标
            if (currentState.Satisfies(goal.DesiredState))
                return new GOAPPlan(goal, new List<IGOAPAction>(), 0f);

            // 过滤不可用的动作
            var usableActions = availableActions
                .Where(a => a.CheckProceduralPrecondition(context))
                .ToList();

            if (usableActions.Count == 0)
                return GOAPPlan.Invalid(goal);

            // A*搜索：从目标状态反向搜索到当前状态
            var openList = new List<PlanNode>();
            var closedSet = new HashSet<ulong>(); // 用状态哈希来去重

            // 起始节点：目标状态
            var startNode = new PlanNode(goal.DesiredState, null, null, 0f, Heuristic(goal.DesiredState, currentState));
            openList.Add(startNode);

            int iterations = 0;
            PlanNode bestGoalNode = null;
            float bestCost = float.MaxValue;

            while (openList.Count > 0 && iterations++ < MaxSearchNodes)
            {
                // 找到f值最小的节点
                openList.Sort((a, b) => a.TotalCost.CompareTo(b.TotalCost));
                var currentNode = openList[0];
                openList.RemoveAt(0);

                ulong stateHash = ComputeHash(currentNode.State);
                if (closedSet.Contains(stateHash))
                    continue;
                closedSet.Add(stateHash);

                // 检查是否到达目标（当前状态满足此节点的状态要求）
                if (currentState.HasAll(currentNode.State))
                {
                    if (currentNode.Cost < bestCost)
                    {
                        bestCost = currentNode.Cost;
                        bestGoalNode = currentNode;
                        // 继续搜索看看是否有更优解
                    }

                    continue;
                }

                // 扩展节点：尝试每个动作的反向应用
                foreach (var action in usableActions)
                {
                    // 如果动作的效果能帮助满足当前节点的状态
                    if (!ActionHelps(action, currentNode.State))
                        continue;

                    // 计算父状态：移除动作效果，添加前提条件
                    var parentState = RegressState(currentNode.State, action);

                    // 计算新的代价
                    float newCost = currentNode.Cost + action.Cost;
                    float heuristic = Heuristic(parentState, currentState);

                    var neighborNode = new PlanNode(parentState, action, currentNode, newCost, heuristic);

                    ulong neighborHash = ComputeHash(parentState);
                    if (closedSet.Contains(neighborHash))
                        continue;

                    openList.Add(neighborNode);
                }
            }

            if (bestGoalNode == null)
                return GOAPPlan.Invalid(goal);

            // 重构计划（反向展开，需要反转）
            var plan = ReconstructPlan(bestGoalNode, goal);
            return plan;
        }

        /// <summary>
        /// 检查动作效果是否与目标状态有任何交集
        /// </summary>
        private bool ActionHelps(IGOAPAction action, GOAPWorldState state)
        {
            for (int i = 0; i < 128; i++)
            {
                if (action.Effects.Get(i) && state.Get(i))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 状态回归：用动作的前提替换动作能提供的那些效果
        /// </summary>
        private GOAPWorldState RegressState(GOAPWorldState state, IGOAPAction action)
        {
            // 父状态 = (状态 - 动作效果) | 动作前提
            // 也就是：去掉动作能提供的那些效果（因为它们由这个动作来实现了），
            // 然后加上这个动作需要什么前提

            var result = GOAPWorldState.Empty;
            for (int i = 0; i < 128; i++)
            {
                bool inState = state.Get(i);
                bool inEffects = action.Effects.Get(i);
                bool inPreconditions = action.Preconditions.Get(i);

                // 如果状态需要这个条件，且动作效果能提供，则由动作前提替代
                // 否则保留原状态
                if (inState && inEffects)
                {
                    // 这个动作提供此效果，所以用前提替换
                    if (inPreconditions)
                        result.Set(i, true);
                    else
                        result.Set(i, false); // 效果对应的位不再需要（由动作解决）
                }
                else if (inState)
                {
                    result.Set(i, true);
                }
                else if (inPreconditions)
                {
                    // 前提条件也需要加入到父状态中
                    result.Set(i, true);
                }
            }

            return result;
        }

        /// <summary>
        /// 启发函数：估计从state到goalState的代价
        /// </summary>
        private float Heuristic(GOAPWorldState state, GOAPWorldState goalState)
        {
            // 简单启发：统计未满足的状态位数
            int unsatisfied = 0;
            for (int i = 0; i < 128; i++)
            {
                if (goalState.Get(i) && !state.Get(i))
                    unsatisfied++;
            }

            return unsatisfied * 0.5f; // 假设每个未满足状态的平均代价为0.5
        }

        /// <summary>
        /// 计算世界状态的简单哈希
        /// </summary>
        private ulong ComputeHash(GOAPWorldState state)
        {
            // 由于WorldState字段是私有的，我们用GetHashCode
            return (ulong) state.GetHashCode();
        }

        /// <summary>
        /// 从目标节点重构计划
        /// </summary>
        private GOAPPlan ReconstructPlan(PlanNode goalNode, IGOAPGoal goal)
        {
            var actions = new List<IGOAPAction>();
            var current = goalNode;
            float totalCost = goalNode.Cost;

            // 反向遍历，收集动作
            while (current != null && current.Action != null)
            {
                actions.Add(current.Action);
                current = current.Parent;
            }

            // 反转得到正序计划
            actions.Reverse();
            return new GOAPPlan(goal, actions, totalCost);
        }
    }
}