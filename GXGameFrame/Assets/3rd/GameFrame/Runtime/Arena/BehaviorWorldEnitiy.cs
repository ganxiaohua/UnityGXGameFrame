using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class BehaviorWorldEnitiy : Entity, IStartSystem<Type>, IUpdateSystem
    {
        private BehaviorWorld m_BehaviorWorld;
        private Dictionary<Type, BehaviorEnitiy> m_BehaviorDic;
        private Dictionary<IBehaviorData, BehaviorEnitiy> m_DataForBehaviorDic;
        private List<IBehaviorData> m_Datas;

        public void Start(Type arenatype)
        {
            m_BehaviorWorld = (BehaviorWorld) ReferencePool.Acquire(arenatype);
            m_BehaviorWorld.Init(this);
            m_DataForBehaviorDic = new();
            m_BehaviorDic = new();
            m_Datas = new();
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
            var jackdollComponent = AddChild<BehaviorEnitiy, Type, BehaviorWorld>(type, m_BehaviorWorld);
            m_BehaviorDic.Add(type, jackdollComponent);
            return jackdollComponent.Behavior;
        }

        /// <summary>
        /// 刪除一個人偶師
        /// </summary>
        /// <param name="self"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public void RemoveBehavior<T>() where T : Behavior
        {
            Type type = typeof(T);
            if (!m_BehaviorDic.TryGetValue(type, out BehaviorEnitiy jackdoll))
            {
                return;
            }

            RemoveChild(jackdoll);
            m_BehaviorDic.Remove(type);
        }

        /// <summary>
        /// 玩偶转入到其他的人偶师的操作下
        /// </summary>
        /// <param name="self"></param>
        /// <param name="jackdollType">人偶师类型</param>
        /// <param name="behaviorData">玩偶本身</param>
        public void ChangeBehavior<T>(IBehaviorData behaviorData) where T : Behavior
        {
            if (m_DataForBehaviorDic.TryGetValue(behaviorData, out BehaviorEnitiy  behavior))
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
        /// 加入一个人偶到舞台上
        /// </summary>
        public T AddData<T>() where T : class, IBehaviorData, new()
        {
            T doll = ReferencePool.Acquire<T>();
            m_Datas.Add(doll);
            return doll;
        }

        /// <summary>
        /// 从舞台上删除一个人偶
        /// </summary>
        public void RemoveData(IBehaviorData behaviorData)
        {
            if (!m_Datas.Contains(behaviorData))
            {
                return;
            }

            m_Datas.Remove(behaviorData);
            if (m_DataForBehaviorDic.TryGetValue(behaviorData, out BehaviorEnitiy jackdoll))
            {
                m_DataForBehaviorDic.Remove(behaviorData);
                jackdoll.DataLeave(behaviorData);
            }
        }
    }
}