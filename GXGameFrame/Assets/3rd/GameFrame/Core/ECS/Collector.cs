using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Entitas;
using Sirenix.Utilities;

namespace GameFrame
{
    public class Collector : IReference
    {
        private Group[] m_Groups;
        /// <summary>
        /// 存放group里的东西
        /// </summary>
        private HashSet<Entity> m_CollectedEntities;

        public HashSet<Entity> CollectedEntities => m_CollectedEntities;
        
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
            m_CollectedEntities = new HashSet<Entity>();
            m_Groups = group;
            foreach (var item in m_Groups)
            {
                m_CollectedEntities.AddRange(item.EntitiesMap);
            }
        }

        public void Clear()
        {
        }
    }
}