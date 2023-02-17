using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace GameFrame
{
    public class Group : IReference
    {
        private Matcher Matcher;
        private HashSet<ECSEntity> m_EntitiesMap;
        public event GroupChanged GroupAdd;
        public event GroupChanged GroupRomve;
        public HashSet<ECSEntity> EntitiesMap => m_EntitiesMap;

        public static Group CreateGroup(Matcher matcher)
        {
            Group Group = ReferencePool.Acquire<Group>();
            Group.Matcher = matcher;
            Group.m_EntitiesMap = new();
            return Group;
        }

        public static void RemoveGroup(Group group)
        {
            ReferencePool.Release(group);
        }

        public void AddComponent(ECSEntity entity)
        {
            if (m_EntitiesMap.Add(entity))
            {
                GroupAdd?.Invoke(this, entity);
            }
        }

        public void RemoveComponent(ECSEntity entity)
        {
            if (m_EntitiesMap.Remove(entity))
            {
                GroupRomve?.Invoke(this, entity);
            }
        }

        public int HandleEntitySilently(ECSEntity entity)
        {
            if (this.Matcher.Match(entity))
                this.AddComponent(entity);
            else
                this.RemoveComponent(entity);
            return m_EntitiesMap.Count;
        }


        public void Clear()
        {
            GroupAdd -= GroupAdd;
            GroupRomve -= GroupRomve;
            Matcher = null;
            m_EntitiesMap.Clear();
        }
    }
}