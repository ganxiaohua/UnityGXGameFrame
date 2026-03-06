using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public struct GoapActionComponent : EffComponent, IInitializeSystem
    {
        public int Value;

        public void OnInitialize()
        {
            Value = ListDatas<List<GoapAction>>.Instance.AddArrayDatas(64);
        }

        public void Dispose()
        {
        }
    }
}