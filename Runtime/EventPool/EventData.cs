using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class EventData : Singleton<EventData>
    {
        /// <summary>
        /// 标记实体组件和和对应的事件类型
        /// </summary>
        private Dictionary<Type, HashSet<Type>> sourceDic = new();

        /// <summary>
        /// 标记事件和对应的实体
        /// </summary>
        private Dictionary<Type, HashSet<IEntity>> eventEntityDic = new();

        public void AddSourceDic(Type key, Type vale)
        {
            if (!sourceDic.TryGetValue(key, out HashSet<Type> types))
            {
                types = new HashSet<Type>();
                sourceDic.Add(key, types);
            }

            types.Add(vale);
        }

        /// <summary>
        /// 当一个实体加入的时候,找到这个事件,然后加入到eventEntityDic字典
        /// </summary>
        /// <param name="enitiType"></param>
        /// <param name="entity"></param>
        public void AddEventEntity(IEntity entity)
        {
            Type enitiType = entity.GetType();
            if (!sourceDic.TryGetValue(enitiType, out var eventhashset))
            {
                return;
            }

            foreach (Type eventType in eventhashset)
            {
                if (!eventEntityDic.TryGetValue(eventType, out var entities))
                {
                    entities = new HashSet<IEntity>();
                    eventEntityDic.Add(eventType, entities);
                }

                entities.Add(entity);
            }
        }

        /// <summary>
        /// 当一个实体从系统中删除的时候删除其系统组件
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveEventEntity(IEntity entity)
        {
            Type enitiType = entity.GetType();
            if (!sourceDic.TryGetValue(enitiType, out var eventhashset))
            {
                return;
            }

            foreach (Type eventType in eventhashset)
            {
                if (!eventEntityDic.TryGetValue(eventType, out var entities))
                {
                    continue;
                }

                entities.Remove(entity);
            }
        }

        public HashSet<IEntity> GetEntity(Type eventType)
        {
            var hashset = eventEntityDic.GetValueOrDefault(eventType);
            var tempHashset = HashSetPool<IEntity>.Get();
            foreach (var obj in hashset)
                tempHashset.Add(obj);
            return tempHashset;
        }
    }
}