using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class GOAPAgent : IDisposable
    {
        private IGOAPGoal currentGoal;

        private int currentActionIndex;

        private List<IGOAPGoal> goals;
        private Dictionary<Type, IGOAPGoal> goalsDict;

        private List<GOAPActionBase> actions;
        private Dictionary<Type, GOAPActionBase> actionsDict;

        private List<GOAPActionBase> resultActions;


        private GOAPPlan planner;

        public GOAPState WorldState { get; private set; } = GOAPState.Empty;

        public float ReplanInterval { get; set; } = 1.0f;

        private bool environmentalChanges;

        private float replanTimer;

        public void Init()
        {
            goals = ListPool<IGOAPGoal>.Get(8);
            goalsDict = DictPool<Type, IGOAPGoal>.Get(8);
            actions = ListPool<GOAPActionBase>.Get(16);
            actionsDict = DictPool<Type, GOAPActionBase>.Get(16);
            resultActions = ListPool<GOAPActionBase>.Get(8);
            replanTimer = ReplanInterval + 1;
            planner = ReferencePool.Acquire<GOAPPlan>();
        }

        public void SetFinishedAction(Action action)
        {
            planner.SetFinishedAction(action);
        }


        public void AddGoal<T>() where T : class, IGOAPGoal
        {
            var type = typeof(T);
            if (goalsDict.ContainsKey(type))
                return;
            var goal = ReferencePool.Acquire<T>();
            goals.Add(goal);
            goalsDict[type] = goal;
            environmentalChanges = true;
        }


        public void RemoveGoal<T>() where T : class, IGOAPGoal
        {
            var type = typeof(T);
            if (!goalsDict.TryGetValue(type, out var goal))
                return;
            goals.RemoveSwapBack(goal);
            goalsDict.Remove(type);
            ReferencePool.Release(goal);
            environmentalChanges = true;
        }

        public void AddAction<T>() where T : GOAPActionBase
        {
            var type = typeof(T);
            if (actionsDict.ContainsKey(type))
                return;
            var action = ReferencePool.Acquire<T>();
            actions.Add(action);
            actionsDict[type] = action;
            action.Agent = this;
            environmentalChanges = true;
        }

        public void RemoveAction<T>() where T : GOAPActionBase
        {
            var type = typeof(T);
            if (!actionsDict.TryGetValue(type, out var action))
                return;
            actions.RemoveSwapBack(action);
            actionsDict.Remove(type);
            ReferencePool.Release(action);
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
                        resultActions[currentActionIndex].OnExit();
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
            ReplanInterval = 1;
            replanTimer = ReplanInterval + 1;
            foreach (var goal in goals)
            {
                ReferencePool.Release(goal);
            }

            foreach (var action in actions)
            {
                ReferencePool.Release(action);
            }

            ListPool<IGOAPGoal>.Release(goals);
            DictPool<Type, IGOAPGoal>.Release(goalsDict);
            ListPool<GOAPActionBase>.Release(actions);
            DictPool<Type, GOAPActionBase>.Release(actionsDict);
            ListPool<GOAPActionBase>.Release(resultActions);
            ReferencePool.Release(planner);
        }
    }
}