using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class BehaviorWorldEntity : Entity, IStartSystem<Type>, IUpdateSystem
    {
        private BehaviorWorld m_BehaviorWorld;
        private Dictionary<Type, BehaviorEntity> m_BehaviorDic;
        private Dictionary<IBehaviorData, BehaviorEntity> m_DataForBehaviorDic;
        private List<IBehaviorData> m_Datas;

        public void Start(Type arenatype)
        {
            m_BehaviorWorld = (BehaviorWorld) ReferencePool.Acquire(arenatype);
            m_DataForBehaviorDic = new();
            m_BehaviorDic = new();
            m_Datas = new();
            m_BehaviorWorld.Init(this);
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            m_BehaviorWorld.Update(elapseSeconds);
        }

        public override void Clear()
        {
            base.Clear();
            ReferencePool.Release(m_BehaviorWorld);
            m_BehaviorDic.Clear();
            m_DataForBehaviorDic.Clear();
            foreach (var doll in m_Datas)
            {
                ReferencePool.Release(doll);
            }
        }


        /// <summary>
        /// 添加一個行为机
        /// </summary>
        /// <param name="self"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Behavior AddBehavior<T>() where T : Behavior
        {
            Type type = typeof(T);
            var jackdollComponent = AddChild<BehaviorEntity, Type, BehaviorWorld>(type, m_BehaviorWorld);
            m_BehaviorDic.Add(type, jackdollComponent);
            return jackdollComponent.Behavior;
        }

        /// <summary>
        /// 刪除一个行为机
        /// </summary>
        /// <param name="self"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public void RemoveBehavior<T>() where T : Behavior
        {
            Type type = typeof(T);
            if (!m_BehaviorDic.TryGetValue(type, out BehaviorEntity jackdoll))
            {
                return;
            }

            RemoveChild(jackdoll);
            m_BehaviorDic.Remove(type);
        }

        /// <summary>
        /// 将实体转到其他行为机下
        /// </summary>
        /// <param name="self"></param>
        /// <param name="jackdollType">人偶师类型</param>
        /// <param name="behaviorData">玩偶本身</param>
        public void ChangeBehavior<T>(IBehaviorData behaviorData) where T : Behavior
        {
            if (m_DataForBehaviorDic.TryGetValue(behaviorData, out BehaviorEntity  behavior))
            {
                m_DataForBehaviorDic.Remove(behaviorData);
                behavior.DataLeave(behaviorData);
            }
            Type behaviorType = typeof(T);
            if (!m_BehaviorDic.TryGetValue(behaviorType, out behavior))
            {
                Debugger.LogError($"不存在这个{behaviorType}的状态机");
                return;
            }

            behavior.DataJoin(behaviorData);
            m_DataForBehaviorDic.Add(behaviorData, behavior);
        }

        /// <summary>
        /// 加入操控数据
        /// </summary>
        public T AddData<T>() where T : class, IBehaviorData, new()
        {
            T doll = ReferencePool.Acquire<T>();
            m_Datas.Add(doll);
            return doll;
        }

        /// <summary>
        /// 删除操控数据
        /// </summary>
        public void RemoveData(IBehaviorData behaviorData)
        {
            if (!m_Datas.Contains(behaviorData))
            {
                return;
            }

            m_Datas.Remove(behaviorData);
            if (m_DataForBehaviorDic.TryGetValue(behaviorData, out BehaviorEntity jackdoll))
            {
                m_DataForBehaviorDic.Remove(behaviorData);
                jackdoll.DataLeave(behaviorData);
            }
        }
    }
}