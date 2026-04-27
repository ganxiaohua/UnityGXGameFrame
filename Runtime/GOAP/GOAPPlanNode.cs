using System;

namespace GameFrame.Runtime
{
    public struct GOAPPlanNode : IComparable<GOAPPlanNode>
    {
        public GOAPState State;
        public GOAPState Preconditions;
        public GOAPState Effects;
        public int ParentIndex;
        public int ActionIndex;
        public float Cost; // 从起始到此的累积代价（g）
        public float Heuristic; // 到目标的估计代价（h）
        public float TotalCost => Cost + Heuristic; // f = g + h

        public GOAPPlanNode(GOAPState state, GOAPState preconditions, GOAPState effects, int parentIndex, int actionIndex, float cost, float heuristic)
        {
            State = state;
            Preconditions = preconditions;
            Effects = effects;
            ParentIndex = parentIndex;
            Cost = cost;
            Heuristic = cost;
            ActionIndex = actionIndex;
        }

        public int CompareTo(GOAPPlanNode other)
        {
            if (TotalCost > other.TotalCost)
                return -1;
            else if (TotalCost < other.TotalCost)
            {
                return 1;
            }

            return 0;
        }
    }
}