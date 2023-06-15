using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.Utilities;

namespace GameFrame
{
    public class Collector : IReference
    {
        private HashSet<ECSEntity> m_CollectedEntities;

        private Dictionary<int, int> m_EntityDictonary;

        public HashSet<ECSEntity> CollectedEntities => m_CollectedEntities;


        public static Collector CreateCollector(Context context, params int[] indexs)
        {
            Group[] groups = new Group[indexs.Length];
            for (int i = 0; i < indexs.Length; i++)
            {
                Matcher matcher = Matcher.SetAllOfIndices(indexs[i]);
                groups[i] = context.GetGroup(matcher);
            }

            Collector collector = ReferencePool.Acquire<Collector>();
            collector.InitCollector(groups);
            return collector;
        }

        private void InitCollector(Group[] group)
        {
            m_CollectedEntities = new HashSet<ECSEntity>();
            m_EntityDictonary = new();
            var temAdd = new GroupChanged(this.EventAdd);
            var temremove = new GroupChanged(this.EventRemove);
            foreach (var item in group)
            {
                item.GroupAdd -= temAdd;
                item.GroupAdd += temAdd;
                item.GroupRomve -= temremove;
                item.GroupRomve += temremove;
                Add(item);
            }
        }

        private void Add(Group grop)
        {
            foreach (var item in grop.EntitiesMap)
            {
                m_CollectedEntities.Add(item);
                if (!m_EntityDictonary.ContainsKey(item.ID))
                {
                    m_EntityDictonary[item.ID] = 0;
                }
                m_EntityDictonary[item.ID]++;
            }
        }

        private void EventAdd(Group group, ECSEntity ecsEntity)
        {
            m_CollectedEntities.Add(ecsEntity);
            if (!m_EntityDictonary.ContainsKey(ecsEntity.ID))
            {
                m_EntityDictonary[ecsEntity.ID] = 0;
            }

            m_EntityDictonary[ecsEntity.ID]++;
        }

        private void EventRemove(Group group, ECSEntity ecsEntity)
        {
            if (m_EntityDictonary.ContainsKey(ecsEntity.ID))
            {
                if (--m_EntityDictonary[ecsEntity.ID] == 0)
                {
                    m_CollectedEntities.Remove(ecsEntity);
                    m_EntityDictonary.Remove(ecsEntity.ID);
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