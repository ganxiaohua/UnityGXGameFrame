using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class BehaviorWorldEntity : Entity, IInitializeSystem<Type>, IUpdateSystem
    {
        private BehaviorWorld behaviorWorld;
        private Dictionary<Type, BehaviorEntity> behaviorDic;
        private Dictionary<IBehaviorData, BehaviorEntity> dataForBehaviorDic;
        private List<IBehaviorData> dataList;

        public void OnInitialize(Type world)
        {
            dataForBehaviorDic = new();
            behaviorDic = new();
            dataList = new();
            behaviorWorld = (BehaviorWorld) ReferencePool.Acquire(world);
            behaviorWorld.Init(this);
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            behaviorWorld.Update(elapseSeconds);
        }

        public override void Dispose()
        {
            base.Dispose();
            ReferencePool.Release(behaviorWorld);
            behaviorDic.Clear();
            dataForBehaviorDic.Clear();
            foreach (var doll in dataList)
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
            var jackdollComponent = AddChild<BehaviorEntity, Type, BehaviorWorld>(type, behaviorWorld);
            behaviorDic.Add(type, jackdollComponent);
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
            if (!behaviorDic.TryGetValue(type, out BehaviorEntity jackdoll))
            {
                return;
            }

            RemoveChild(jackdoll);
            behaviorDic.Remove(type);
        }

        /// <summary>
        /// 将实体转到其他行为机下
        /// </summary>
        /// <param name="self"></param>
        /// <param name="jackdollType">人偶师类型</param>
        /// <param name="behaviorData">玩偶本身</param>
        public void ChangeBehavior<T>(IBehaviorData behaviorData) where T : Behavior
        {
            if (dataForBehaviorDic.TryGetValue(behaviorData, out BehaviorEntity  behavior))
            {
                dataForBehaviorDic.Remove(behaviorData);
                behavior.DataLeave(behaviorData);
            }
            Type behaviorType = typeof(T);
            if (!behaviorDic.TryGetValue(behaviorType, out behavior))
            {
                Debugger.LogError($"不存在这个{behaviorType}的状态机");
                return;
            }

            behavior.DataJoin(behaviorData);
            dataForBehaviorDic.Add(behaviorData, behavior);
        }

        /// <summary>
        /// 加入操控数据
        /// </summary>
        public T AddData<T>() where T : class, IBehaviorData, new()
        {
            T doll = ReferencePool.Acquire<T>();
            dataList.Add(doll);
            return doll;
        }

        /// <summary>
        /// 删除操控数据
        /// </summary>
        public void RemoveData(IBehaviorData behaviorData)
        {
            if (!dataList.Contains(behaviorData))
            {
                return;
            }

            dataList.Remove(behaviorData);
            if (dataForBehaviorDic.TryGetValue(behaviorData, out BehaviorEntity jackdoll))
            {
                dataForBehaviorDic.Remove(behaviorData);
                jackdoll.DataLeave(behaviorData);
            }
        }
    }
}