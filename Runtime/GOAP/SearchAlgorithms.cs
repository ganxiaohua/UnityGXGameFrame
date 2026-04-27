using GameFrame.Runtime;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct SearchAlgorithms : IJob
{
    public static int MaxSearchNodes = 64;
    private int OpenListHeadPtr;
    private int OpenListEndPtr;
    public NativeArray<GOAPPlanNode> OpenList;
    private NativeList<int> CloseList;
    public NativeArray<GOAPState> Preconditions;
    public NativeArray<GOAPState> Effects;
    public NativeArray<float> Cost;
    public GOAPState GoalState;
    public GOAPState WorldState;
    public int StartIndex;
    public NativeList<int> ActionPathIndex;

    public void Execute()
    {
        float bestCost = float.MaxValue;
        int actionCount = Preconditions.Length;
        OpenListHeadPtr = 0;
        OpenListEndPtr = 0;
        StartIndex = 0;
        var rootNode = new GOAPPlanNode(GoalState, GOAPState.Empty, GOAPState.Empty, -1, -1, 0, Heuristic(GoalState, WorldState));
        OpenList = new NativeArray<GOAPPlanNode>(MaxSearchNodes, Allocator.Temp);
        CloseList = new NativeList<int>(MaxSearchNodes, Allocator.Temp);
        OpenList[OpenListHeadPtr] = rootNode;
        OpenListEndPtr++;
        while (OpenListHeadPtr < OpenListEndPtr)
        {
            OpenList.Sort();
            var currentNode = OpenList[OpenListHeadPtr];
            OpenListHeadPtr++;
            var currentNodeHash = currentNode.State.GetHashCode();
            if (!CloseList.Contains(currentNodeHash))
            {
                CloseList.Add(currentNodeHash);
            }

            if (WorldState.HasAll(currentNode.State))
            {
                if (currentNode.TotalCost < bestCost)
                {
                    bestCost = currentNode.TotalCost;
                    StartIndex = OpenListHeadPtr - 1;
                }

                continue;
            }

            for (int i = 0; i < actionCount; i++)
            {
                if (!ActionHelps(Effects[i], currentNode.State))
                    continue;
                var parentState = RegressState(Effects[i], Preconditions[i], currentNode.State);
                float newCost = currentNode.TotalCost + Cost[i];
                float heuristic = Heuristic(parentState, WorldState);
                if (CloseList.Contains(parentState.GetHashCode()))
                    continue;
                var neighborNode = new GOAPPlanNode(parentState, Preconditions[i], Effects[i], OpenListHeadPtr, i, newCost, heuristic);
                var endPtr = OpenListEndPtr++;
                OpenListExpansion();
                OpenList[endPtr] = neighborNode;
            }
        }

        if (Mathf.Approximately(bestCost, float.MaxValue))
            return;
        DoActionPathIndex();
    }

    private bool ActionHelps(GOAPState effects, GOAPState state)
    {
        for (int i = 0; i < GOAPState.BitsSizeMax; i++)
        {
            if (effects.Get(i) && state.Get(i))
                return true;
        }

        return false;
    }


    private GOAPState RegressState(GOAPState effects, GOAPState preconditions, GOAPState state)
    {
        var result = GOAPState.Empty;
        for (int i = 0; i < GOAPState.BitsSizeMax; i++)
        {
            bool inState = state.Get(i);
            bool inEffects = effects.Get(i);
            bool inPreconditions = preconditions.Get(i);

            if (inState && inEffects)
            {
                result.Set(i, inPreconditions);
            }
            else if (inState)
            {
                result.Set(i, true);
            }
            else if (inPreconditions)
            {
                result.Set(i, true);
            }
        }

        return result;
    }


    private float Heuristic(GOAPState state, GOAPState goalState)
    {
        int unsatisfied = 0;
        for (int i = 0; i < GOAPState.BitsSizeMax; i++)
        {
            if (goalState.Get(i) && !state.Get(i))
                unsatisfied++;
        }

        return unsatisfied * 0.5f;
    }

    private void OpenListExpansion()
    {
        if (OpenListEndPtr >= OpenList.Length)
        {
            var newOpenList = new NativeArray<GOAPPlanNode>(OpenListEndPtr * 2, Allocator.Temp);
            newOpenList.CopyFrom(OpenList);
            OpenList.Dispose();
            OpenList = newOpenList;
        }
    }

    private void DoActionPathIndex()
    {
        ActionPathIndex = new NativeList<int>(MaxSearchNodes, Allocator.Temp);
        while (OpenList[StartIndex].ActionIndex != -1)
        {
            ActionPathIndex.Add(OpenList[StartIndex].ActionIndex);
        }
    }
}