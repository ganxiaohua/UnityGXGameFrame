using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace GameFrame.Runtime
{
    public class GOAPPlan : IDisposable
    {
        public static int MaxSearchNodes = 64;
        private NativeArray<GOAPState> preconditions;
        private NativeArray<GOAPState> effects;
        private List<int> availableActionsIndex;
        private NativeArray<float> cost;
        public Action FinishAction;

        public void Init(Action finishAction)
        {
            FinishAction = finishAction;
        }

        public void Plan(GOAPState worldState, IGOAPGoal goal, List<GOAPActionBase> availableActions, List<GOAPActionBase> result)
        {
            if (worldState.Satisfies(goal.DesiredState))
            {
                return;
            }

            preconditions = new NativeArray<GOAPState>(availableActions.Count, Allocator.TempJob);
            effects = new NativeArray<GOAPState>(availableActions.Count, Allocator.TempJob);
            cost = new NativeArray<float>(availableActions.Count, Allocator.TempJob);
            var openList = new NativeList<GOAPPlanNode>(MaxSearchNodes, Allocator.TempJob);
            var closeList = new NativeHashSet<int>(MaxSearchNodes, Allocator.TempJob);
            var actionPathIndexList = new NativeList<int>(availableActions.Count * 2, Allocator.TempJob);
            availableActionsIndex = ListPool<int>.Get();
            bool succ = FilterAction(availableActions);
            if (!succ)
                return;
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
            };
            JobHandle handle = job.Schedule();
            handle.Complete();
            foreach (var index in job.ActionPathIndex)
            {
                result.Add(availableActions[availableActionsIndex[index]]);
            }

            ListPool<int>.Release(availableActionsIndex);
            preconditions.Dispose();
            effects.Dispose();
            cost.Dispose();
            openList.Dispose();
            closeList.Dispose();
            actionPathIndexList.Dispose();
            FinishAction?.Invoke();
        }

        private bool FilterAction(List<GOAPActionBase> availableActions)
        {
            bool hasAction = false;
            int newIndex = 0;
            for (var index = 0; index < availableActions.Count; index++)
            {
                var availableAction = availableActions[index];
                if (availableAction.CheckProceduralPrecondition())
                {
                    preconditions[newIndex] = availableAction.Preconditions;
                    effects[newIndex] = availableAction.Effects;
                    cost[newIndex] = availableAction.Cost;
                    newIndex++;
                    hasAction = true;
                    availableActionsIndex.Add(index);
                }
            }

            return hasAction;
        }

        public void Dispose()
        {
            preconditions.Dispose();
            effects.Dispose();
            cost.Dispose();
        }
    }
}