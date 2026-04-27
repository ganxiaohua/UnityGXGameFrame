using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace GameFrame.Runtime
{
    public struct GOAPPlan
    {
        public IGOAPGoal Goal;
        public NativeArray<GOAPState> Preconditions;
        public NativeArray<GOAPState> Effects;
        public NativeArray<float> Cost;

        public void Plan(GOAPState worldState, IGOAPGoal goal, List<IGOAPAction> availableActions)
        {
            if (goal.DesiredState == GOAPState.Empty)
                return;
            if (worldState.Satisfies(goal.DesiredState))
            {
                return;
            }

            bool succ = FilterAction(availableActions);
            if (!succ)
                return;
            SearchAlgorithms job = new SearchAlgorithms
            {
                Preconditions = this.Preconditions,
                Effects = this.Effects,
                Cost = this.Cost,
                WorldState = worldState,
                GoalState = goal.DesiredState,
            };
            JobHandle handle = job.Schedule();
            handle.Complete();
        }

        private bool FilterAction(List<IGOAPAction> availableActions)
        {
            bool hasAction = false;
            Preconditions = new NativeArray<GOAPState>(availableActions.Count, Allocator.Temp);
            Effects = new NativeArray<GOAPState>(availableActions.Count, Allocator.Temp);
            Cost = new NativeArray<float>(availableActions.Count, Allocator.Temp);
            int newIndex = 0;
            for (var index = 0; index < availableActions.Count; index++)
            {
                var availableAction = availableActions[index];
                if (availableAction.CheckProceduralPrecondition())
                {
                    Preconditions[newIndex] = availableAction.Preconditions;
                    Effects[newIndex] = availableAction.Effects;
                    Cost[newIndex] = availableAction.Cost;
                    newIndex++;
                    hasAction = true;
                }
            }

            return hasAction;
        }
    }
}