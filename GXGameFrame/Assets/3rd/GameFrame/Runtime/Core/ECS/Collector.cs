using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.Utilities;

namespace GameFrame
{
    public class Collector : IReference
    {
        // private Group[] m_Groups;
        /// <summary>
        /// 存放group里的东西
        /// </summary>
        private HashSet<ECSEntity> m_CollectedEntities;

        public Dictionary<ECSEntity, int> m_EntityDictonary;

        public HashSet<ECSEntity> CollectedEntities => m_CollectedEntities;
        

        public static Collector CreateCollector(Context context, params Type[] types)
        {
            Group[] groups = new Group[types.Length];
            for (int i = 0; i < types.Length; i++)
            {
                Matcher matcher = Matcher.SetAllOfIndices(types[i]);
                groups[i] = context.GetGroup(matcher);
            }
            Collector collector = ReferencePool.Acquire<Collector>();
            collector.InitCollector(groups);
            return collector;
        }

        public void InitCollector(Group[] group)
        {
            m_CollectedEntities = new HashSet<ECSEntity>();
            m_EntityDictonary = new();
            foreach (var item in group)
            {
                item.GroupAdd -= EventAdd;
                item.GroupRomve -= EventRemove;
                item.GroupAdd += EventAdd;
                item.GroupRomve += EventRemove;
                Add(item);
            }
        }

        public void Add(Group grop)
        {
            foreach (var item in grop.EntitiesMap)
            {
                m_CollectedEntities.Add(item);
                if (!m_EntityDictonary.ContainsKey(item))
                {
                    m_EntityDictonary[item] = 0;
                }
                m_EntityDictonary[item]++;
            }
        }

        public void EventAdd(Group group,ECSEntity ecsEntity)
        {
            m_CollectedEntities.Add(ecsEntity);
            if (!m_EntityDictonary.ContainsKey(ecsEntity))
            {
                m_EntityDictonary[ecsEntity] = 0;
            }
            m_EntityDictonary[ecsEntity]++;
        }
        
        public void EventRemove(Group group,ECSEntity ecsEntity)
        {
            if (m_EntityDictonary.ContainsKey(ecsEntity))
            {
                if (--m_EntityDictonary[ecsEntity] == 0)
                {
                    m_CollectedEntities.Remove(ecsEntity);
                    m_EntityDictonary.Remove(ecsEntity);
                }
            }
        }

        public void Clear()
        {
            m_EntityDictonary.Clear();
            m_CollectedEntities.Clear();
        }
    }
}