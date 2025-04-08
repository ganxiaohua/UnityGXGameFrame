using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class BehaviorEntity : Entity, IInitializeSystem<Type, BehaviorWorld>, IUpdateSystem
    {
        public Behavior Behavior;
        private List<IBehaviorData> dataList;

        public void OnInitialize(Type behaviorType, BehaviorWorld behaviorWorld)
        {
            Behavior = (Behavior) ReferencePool.Acquire(behaviorType);
            Behavior.Init(behaviorWorld);
            dataList = new List<IBehaviorData>(16);
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (dataList.Count == 0) return;
            Behavior.Update(dataList, elapseSeconds);
        }

        public override void Dispose()
        {
            base.Dispose();
            dataList.Clear();
            ReferencePool.Release(Behavior);
        }

        public void DataJoin(IBehaviorData behaviorData)
        {
            Behavior.DataJoin(behaviorData);
            dataList.Add(behaviorData);
        }

        public void DataLeave(IBehaviorData behaviorData)
        {
            Behavior.DataLeave(behaviorData);
            dataList.Remove(behaviorData);
        }
    }
}