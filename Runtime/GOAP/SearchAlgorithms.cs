using GameFrame.Runtime;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct SearchAlgorithms : IJob
{
    private float minimumActionCost;

    [ReadOnly] public NativeArray<GOAPState> Preconditions;
    [ReadOnly] public NativeArray<GOAPState> Effects;
    [ReadOnly] public NativeArray<float> Cost;
    public NativeList<int> ActionPathIndex;

    public NativeList<GOAPPlanNode> OpenList;

    public NativeHashSet<int> CloseList;
    public GOAPState GoalState;
    public GOAPState WorldState;
    public int ActionCount;
    public int MaxNodeCount;
    public int StartIndex;

    public void Execute()
    {
        ActionPathIndex.Clear();
        OpenList.Clear();
        CloseList.Clear();
        StartIndex = -1;

        if (MaxNodeCount <= 0)
        {
            Debug.LogError("GOAP MaxNodeCount must be greater than zero.");
            return;
        }

        if (ActionCount < 0 || ActionCount > Preconditions.Length || ActionCount > Effects.Length ||
            ActionCount > Cost.Length)
        {
            Debug.LogError("GOAP ActionCount is outside the supplied action data range.");
            return;
        }

        if (!InitializeMinimumActionCost())
        {
            Debug.LogError("GOAP action cost must be finite and non-negative.");
            return;
        }

        var rootNode = new GOAPPlanNode(GoalState, -1, -1, 0f, Heuristic(GoalState, WorldState));
        OpenList.Add(rootNode);

        while (TryGetLowestCostOpenNode(out int currentNodeIndex))
        {
            var currentNode = OpenList[currentNodeIndex];
            CloseList.Add(currentNodeIndex);

            if (WorldState.HasAll(currentNode.State))
            {
                StartIndex = currentNodeIndex;
                BuildActionPath();
                return;
            }

            for (int actionIndex = 0; actionIndex < ActionCount; actionIndex++)
            {
                if (!TryRegressState(Effects[actionIndex], Preconditions[actionIndex], currentNode.State,
                        out var regressedState))
                {
                    continue;
                }

                float newCost = currentNode.Cost + Cost[actionIndex];
                if (float.IsNaN(newCost) || float.IsInfinity(newCost))
                    continue;

                float heuristic = Heuristic(regressedState, WorldState);
                int existingNodeIndex = FindStateNode(regressedState);
                if (existingNodeIndex >= 0)
                {
                    var existingNode = OpenList[existingNodeIndex];
                    if (newCost >= existingNode.Cost)
                        continue;

                    existingNode.ParentIndex = currentNodeIndex;
                    existingNode.ActionIndex = actionIndex;
                    existingNode.Cost = newCost;
                    existingNode.Heuristic = heuristic;
                    OpenList[existingNodeIndex] = existingNode;

                    CloseList.Remove(existingNodeIndex);
                    continue;
                }

                if (OpenList.Length >= MaxNodeCount)
                {
                    ActionPathIndex.Clear();
                    StartIndex = -1;
                    Debug.LogError($"GOAP search reached the node limit ({MaxNodeCount}); the truncated plan was discarded.");
                    return;
                }

                OpenList.Add(new GOAPPlanNode(regressedState, currentNodeIndex, actionIndex, newCost, heuristic));
            }
        }
    }

    private bool InitializeMinimumActionCost()
    {
        bool hasAction = false;
        minimumActionCost = 0f;

        for (int i = 0; i < ActionCount; i++)
        {
            float actionCost = Cost[i];
            if (actionCost < 0f || float.IsNaN(actionCost) || float.IsInfinity(actionCost))
                return false;

            if (!hasAction || actionCost < minimumActionCost)
            {
                minimumActionCost = actionCost;
                hasAction = true;
            }
        }

        return true;
    }

    private bool TryGetLowestCostOpenNode(out int nodeIndex)
    {
        nodeIndex = -1;

        for (int i = 0; i < OpenList.Length; i++)
        {
            if (CloseList.Contains(i))
                continue;

            if (nodeIndex < 0 || IsLowerCost(OpenList[i], OpenList[nodeIndex]))
                nodeIndex = i;
        }

        return nodeIndex >= 0;
    }

    private static bool IsLowerCost(GOAPPlanNode candidate, GOAPPlanNode currentBest)
    {
        if (!Mathf.Approximately(candidate.TotalCost, currentBest.TotalCost))
            return candidate.TotalCost < currentBest.TotalCost;

        return candidate.Heuristic < currentBest.Heuristic;
    }

    private int FindStateNode(GOAPState state)
    {
        for (int i = 0; i < OpenList.Length; i++)
        {
            if (StatesEqual(OpenList[i].State, state))
                return i;
        }

        return -1;
    }

    private static bool StatesEqual(GOAPState left, GOAPState right)
    {
        for (int i = 0; i < GOAPState.BitsSizeMax; i++)
        {
            bool leftCares = left.GetCare(i);
            if (leftCares != right.GetCare(i))
                return false;
            if (leftCares && left.Get(i) != right.Get(i))
                return false;
        }

        return true;
    }

    private static bool TryRegressState(GOAPState effects, GOAPState preconditions, GOAPState requiredState,
        out GOAPState result)
    {
        result = GOAPState.Empty;
        bool actionHelps = false;

        for (int i = 0; i < GOAPState.BitsSizeMax; i++)
        {
            bool stateCares = requiredState.GetCare(i);
            bool effectCares = effects.GetCare(i);
            bool preconditionCares = preconditions.GetCare(i);

            if (stateCares && effectCares)
            {
                if (effects.Get(i) != requiredState.Get(i))
                    return false;

                actionHelps = true;
            }

            if (stateCares && !effectCares)
            {
                if (preconditionCares && preconditions.Get(i) != requiredState.Get(i))
                    return false;

                result.Set(i, requiredState.Get(i));
            }
            else if (preconditionCares)
            {
                result.Set(i, preconditions.Get(i));
            }
        }

        return actionHelps;
    }

    public float Heuristic(GOAPState requiredState, GOAPState worldState)
    {
        for (int i = 0; i < GOAPState.BitsSizeMax; i++)
        {
            if (requiredState.GetCare(i) && requiredState.Get(i) != worldState.Get(i))
            {
                return minimumActionCost;
            }
        }

        return 0f;
    }

    private void BuildActionPath()
    {
        int nodeIndex = StartIndex;

        for (int step = 0; step < OpenList.Length; step++)
        {
            if ((uint) nodeIndex >= (uint) OpenList.Length)
            {
                ActionPathIndex.Clear();
                Debug.LogError("GOAP plan contains an invalid parent index.");
                return;
            }

            var node = OpenList[nodeIndex];
            if (node.ActionIndex == -1)
                return;

            ActionPathIndex.Add(node.ActionIndex);
            nodeIndex = node.ParentIndex;
        }

        ActionPathIndex.Clear();
        Debug.LogError("GOAP plan contains a cyclic parent chain.");
    }
}