using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class BehaviorEnitiy : Entity, IStartSystem<Type, BehaviorWorld>, IUpdateSystem
    {
        public Behavior Behavior;
        private List<IBehaviorData> m_Datas;

        public void Start(Type behaviorType, BehaviorWorld behaviorWorld)
        {
            Behavior = (Behavior) ReferencePool.Acquire(behaviorType);
            Behavior.Init(behaviorWorld);
            m_Datas = new List<IBehaviorData>(16);
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            Behavior.Update(m_Datas, elapseSeconds);
        }

        public override void Clear()
        {
            base.Clear();
            m_Datas.Clear();
            ReferencePool.Release(Behavior);
        }

        public void DataJoin(IBehaviorData behaviorData)
        {
            Behavior.DataJoin(behaviorData);
            m_Datas.Add(behaviorData);
        }

        public void DataLeave(IBehaviorData behaviorData)
        {
            Behavior.DataLeave(behaviorData);
            m_Datas.Remove(behaviorData);
        }
    }
}