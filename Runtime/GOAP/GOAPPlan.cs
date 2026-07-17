using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace GameFrame.Runtime
{
    public class GOAPPlan : IDisposable, IVersions
    {
        public static int MaxSearchNodes = 64;

        public int Versions { get; private set; }
        private Action onfinishAction;

        public void Plan(GOAPState worldState, IGOAPGoal goal, List<GOAPActionBase> availableActions, List<GOAPActionBase> result)
        {
            var vers = ++Versions;
            result.Clear();

            if (worldState.Satisfies(goal.DesiredState))
            {
                if (vers == Versions)
                    onfinishAction?.Invoke();
                return;
            }

            int maxNodeCount = MaxSearchNodes;
            if (maxNodeCount <= 0)
                throw new InvalidOperationException($"{nameof(MaxSearchNodes)} must be greater than zero.");

            NativeArray<GOAPState> preconditions = new NativeArray<GOAPState>(availableActions.Count, Allocator.TempJob);
            NativeArray<GOAPState> effects = new NativeArray<GOAPState>(availableActions.Count, Allocator.TempJob);
            NativeArray<float> cost = new NativeArray<float>(availableActions.Count, Allocator.TempJob);
            NativeList<GOAPPlanNode> openList = new NativeList<GOAPPlanNode>(maxNodeCount, Allocator.TempJob);
            NativeHashSet<int> closeList = new NativeHashSet<int>(maxNodeCount, Allocator.TempJob);
            NativeList<int> actionPathIndexList = new NativeList<int>(availableActions.Count * 2, Allocator.TempJob);
            List<int> availableActionsIndex = ListPool<int>.Get();
            bool succ = FilterAction(availableActions, preconditions, effects, cost, availableActionsIndex);
            if (succ)
            {
                SearchAlgorithms job = new SearchAlgorithms
                {
                    Preconditions = preconditions,
                    Effects = effects,
                    Cost = cost,
                    OpenList = openList,
                    CloseList = closeList,
                    ActionPathIndex = actionPathIndexList,
                    WorldState = worldState,
                    GoalState = goal.DesiredState,
                    ActionCount = availableActionsIndex.Count,
                    MaxNodeCount = maxNodeCount,
                };
                JobHandle handle = job.Schedule();
                handle.Complete();
                if (vers == Versions)
                {
                    foreach (var index in job.ActionPathIndex)
                    {
                        result.Add(availableActions[availableActionsIndex[index]]);
                    }
                }
            }

            preconditions.Dispose();
            effects.Dispose();
            cost.Dispose();
            openList.Dispose();
            closeList.Dispose();
            actionPathIndexList.Dispose();
            ListPool<int>.Release(availableActionsIndex);
            if (vers != Versions)
                return;
            onfinishAction?.Invoke();
        }

        private bool FilterAction(List<GOAPActionBase> availableActions, NativeArray<GOAPState> preconditions, NativeArray<GOAPState> effects,
            NativeArray<float> cost, List<int> availableActionsIndex)
        {
            bool hasAction = false;
            int newIndex = 0;
            for (var index = 0; index < availableActions.Count; index++)
            {
                var availableAction = availableActions[index];
                preconditions[newIndex] = availableAction.Preconditions;
                effects[newIndex] = availableAction.Effects;
                cost[newIndex] = availableAction.Cost;
                newIndex++;
                hasAction = true;
                availableActionsIndex.Add(index);
            }

            return hasAction;
        }

        public void SetFinishedAction(Action finishAction)
        {
            this.onfinishAction = finishAction;
        }


        public void Dispose()
        {
            onfinishAction = null;
            Versions++;
        }
    }
}