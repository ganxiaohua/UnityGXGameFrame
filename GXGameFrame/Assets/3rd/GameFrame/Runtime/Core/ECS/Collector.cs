using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.Utilities;
using UnityEngine;

namespace GameFrame
{
    public class Collector : IReference
    {
        private HashSet<ECSEntity> m_CollectedEntities;

        private Dictionary<int, int> m_EntityDictonary;

        public HashSet<ECSEntity> CollectedEntities => m_CollectedEntities;

        // public string EditorName;

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
                m_EntityDictonary.TryAdd(item.ID, 0);
                m_EntityDictonary[item.ID]++;
            }
        }

        private void EventAdd(Group group, ECSEntity ecsEntity)
        {
            // Debug.Log(EditorName);
            m_CollectedEntities.Add(ecsEntity);
            m_EntityDictonary.TryAdd(ecsEntity.ID, 0);
            m_EntityDictonary[ecsEntity.ID]++;
        }

        private void EventRemove(Group group, ECSEntity ecsEntity)
        {
            // Debug.Log(EditorName);
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