using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class GOAPAgent : IDisposable
    {
        private IGOAPGoal currentGoal;

        private int currentActionIndex;

        private readonly List<IGOAPGoal> goals = new();


        private readonly List<GOAPActionBase> actions = new();


        private readonly List<GOAPActionBase> resultActions = new();


        private GOAPPlan planner;

        public GOAPState WorldState { get; private set; } = GOAPState.Empty;

        public float ReplanInterval { get; set; } = 1.0f;

        private bool environmentalChanges;

        private float replanTimer;

        public void Init(Action finishAction)
        {
            replanTimer = ReplanInterval + 1;
            planner = new GOAPPlan();
            planner.Init(finishAction);
        }


        public void AddGoal(IGOAPGoal goal)
        {
            if (goal == null) throw new ArgumentNullException(nameof(goal));
            if (!goals.Contains(goal))
                goals.Add(goal);
            environmentalChanges = true;
        }


        public void RemoveGoal(IGOAPGoal goal)
        {
            goals.RemoveSwapBack(goal);
            environmentalChanges = true;
        }

        public void AddAction(GOAPActionBase actionBase)
        {
            if (actionBase == null) throw new ArgumentNullException(nameof(actionBase));
            if (!actions.Contains(actionBase))
                actions.Add(actionBase);
            environmentalChanges = true;
        }

        public void RemoveAction(GOAPActionBase actionBase)
        {
            actions.RemoveSwapBack(actionBase);
            environmentalChanges = true;
        }

        public void SetWorldState(GOAPState state)
        {
            WorldState = state;
            environmentalChanges = true;
        }

        public void SetWorldState(int index, bool value)
        {
            var state = WorldState;
            state.Set(index, value);
            WorldState = state;
            environmentalChanges = true;
        }


        private void CheckSwitchGoal(IGOAPGoal bestGoal)
        {
            if (bestGoal != null && (currentGoal == null || bestGoal != currentGoal))
            {
                if (currentGoal != null)
                {
                    if (currentActionIndex < resultActions.Count)
                    {
                        resultActions[currentActionIndex].OnAbort();
                    }

                    currentGoal.OnDeactivate();
                }

                currentGoal = bestGoal;
                currentGoal.OnActivate();
            }
        }

        private IGOAPGoal SelectBestGoal()
        {
            IGOAPGoal best = null;
            float bestPriority = float.MinValue;

            foreach (var goal in goals)
            {
                if (!goal.IsValid())
                    continue;
                if (goal.Priority > bestPriority)
                {
                    bestPriority = goal.Priority;
                    best = goal;
                }
            }

            return best;
        }


        private GOAPState ApplyEffects(GOAPState state, GOAPState effects)
        {
            state.Apply(effects);
            return state;
        }

        private void Replan()
        {
            if (currentGoal == null)
                return;
            resultActions.Clear();
            planner.Plan(WorldState, currentGoal, actions, resultActions);
            currentActionIndex = 0;
        }

        private void ExecutePlan(float deltaTime)
        {
            if (currentActionIndex >= resultActions.Count)
            {
                currentActionIndex = 0;
                return;
            }

            var action = resultActions[currentActionIndex];

            if (!action.CheckProceduralPrecondition())
            {
                Replan();
                return;
            }

            if (!action.IsRunning)
            {
                action.OnEnter();
            }

            action.Update(deltaTime);


            if (!action.IsRunning)
            {
                action.OnExit();
                var newState = WorldState;
                newState = ApplyEffects(newState, action.Effects);
                WorldState = newState;
                currentActionIndex++;
            }
        }

        public void Update(float deltaTime)
        {
            if (environmentalChanges)
            {
                replanTimer += deltaTime;
                if (replanTimer >= ReplanInterval)
                {
                    foreach (var goal in goals)
                    {
                        goal.UpdatePriority();
                    }

                    var bestGoal = SelectBestGoal();
                    CheckSwitchGoal(bestGoal);
                    Replan();
                    environmentalChanges = false;
                    replanTimer = 0;
                }
            }

            ExecutePlan(deltaTime);
        }

        public override string ToString()
        {
            string debug = "";
            foreach (var action in resultActions)
            {
                debug += "->" + action.GetType().Name;
            }

            return debug;
        }

        public void Dispose()
        {
            replanTimer = ReplanInterval + 1;
            goals.Clear();
            resultActions.Clear();
        }
    }
}