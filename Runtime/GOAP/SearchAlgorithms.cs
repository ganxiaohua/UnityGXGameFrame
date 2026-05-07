using GameFrame.Runtime;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct SearchAlgorithms : IJob
{
    private int OpenListHeadPtr;
    private int OpenListEndPtr;
    public NativeArray<GOAPState> Preconditions;
    public NativeArray<GOAPState> Effects;
    public NativeArray<float> Cost;
    public NativeList<int> ActionPathIndex;
    public NativeList<GOAPPlanNode> OpenList;
    public NativeHashSet<int> CloseList;
    public GOAPState GoalState;
    public GOAPState WorldState;
    public int StartIndex;

    public void Execute()
    {
        float bestCost = float.MaxValue;
        int actionCount = Preconditions.Length;
        OpenListHeadPtr = 0;
        OpenListEndPtr = 0;
        StartIndex = 0;
        OpenListEndPtr++;
        var rootNode = new GOAPPlanNode(GoalState, -1, -1, 0, Heuristic(GoalState, WorldState));
        OpenList.Add(rootNode);
        while (OpenListHeadPtr < OpenListEndPtr)
        {
            OpenList.Sort();
            var currentNode = OpenList[OpenListHeadPtr];
            var currentNodeHash = currentNode.GetHashCode();
            if (!CloseList.Contains(currentNodeHash))
            {
                CloseList.Add(currentNodeHash);
            }

            if (WorldState.HasAll(currentNode.State))
            {
                if (currentNode.TotalCost < bestCost)
                {
                    bestCost = currentNode.TotalCost;
                    StartIndex = OpenListHeadPtr;
                }

                OpenListHeadPtr++;
                continue;
            }

            for (int i = 0; i < actionCount; i++)
            {
                if (!ActionHelps(Effects[i], currentNode.State))
                    continue;
                var parentState = RegressState(Effects[i], Preconditions[i], currentNode.State);
                float newCost = currentNode.TotalCost + Cost[i];
                float heuristic = Heuristic(parentState, WorldState);
                var neighborNode = new GOAPPlanNode(parentState, OpenListHeadPtr, i, newCost, heuristic);
                if (CloseList.Contains(neighborNode.GetHashCode()))
                    continue;
                OpenListEndPtr++;
                OpenList.Add(neighborNode);
                if (OpenList.Length >= GOAPPlan.MaxSearchNodes)
                {
                    Debug.LogError("OpenList Exceed the limit");
                    return;
                }
            }

            OpenListHeadPtr++;
        }

        if (Mathf.Approximately(bestCost, float.MaxValue))
            return;
        DoActionPathIndex();
    }

    private bool ActionHelps(GOAPState effects, GOAPState state)
    {
        for (int i = 0; i < GOAPState.BitsSizeMax; i++)
        {
            if (!state.GetCare(i)) continue;
            if (effects.GetCare(i) && effects.Get(i) == state.Get(i))
                return true;
        }

        return false;
    }

    public float Heuristic(GOAPState state, GOAPState goalState)
    {
        int unsatisfied = 0;
        for (int i = 0; i < GOAPState.BitsSizeMax; i++)
        {
            if (goalState.GetCare(i) && goalState.Get(i) != state.Get(i))
                unsatisfied++;
        }

        return unsatisfied * 0.5f;
    }


    private GOAPState RegressState(GOAPState effects, GOAPState preconditions, GOAPState state)
    {
        var result = GOAPState.Empty;
        for (int i = 0; i < GOAPState.BitsSizeMax; i++)
        {
            bool stateCares = state.GetCare(i);
            bool effectCares = effects.GetCare(i);
            bool preCares = preconditions.GetCare(i);

            if (stateCares)
            {
                if (effectCares && effects.Get(i) == state.Get(i))
                {
                    if (preCares)
                        result.Set(i, preconditions.Get(i));
                }
                else
                {
                    result.Set(i, state.Get(i));
                }
            }
            else if (preCares)
            {
                result.Set(i, preconditions.Get(i));
            }
        }

        return result;
    }

    private void DoActionPathIndex()
    {
        while (OpenList[StartIndex].ActionIndex != -1)
        {
            ActionPathIndex.Add(OpenList[StartIndex].ActionIndex);
            StartIndex = OpenList[StartIndex].ParentIndex;
        }
    }
}