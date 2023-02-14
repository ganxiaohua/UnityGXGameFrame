using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace GameFrame
{
    public class Group :IReference
    {
        private Matcher Matcher;
        private HashSet<Entity> m_EntitiesMap;
        
        public HashSet<Entity> EntitiesMap =>m_EntitiesMap;

        public static Group CreateGroup(Matcher matcher)
        {
            Group Group =  ReferencePool.Acquire<Group>();
            Group.Matcher = matcher;
            Group.m_EntitiesMap = new();
            return Group;
        }

        public void AddComponent(Entity entity)
        {
            if (m_EntitiesMap.Contains(entity))
            {
                throw new Exception($"Group have entity{entity.GetType()}");
            }
            m_EntitiesMap.Add(entity);
        }

        public void RemoveComponent(Entity entity)
        {
            if (m_EntitiesMap.Contains(entity))
            {
                throw new Exception($"Group not have entity{entity.GetType()}");
            }
            m_EntitiesMap.Remove(entity);
        }

        public void HandleEntitySilently(Entity entity)
        {
            if (this.Matcher.Match(entity))
                this.AddComponent(entity);
            else
                this.RemoveComponent(entity);
        }



        public void Clear()
        {
            m_EntitiesMap.Clear();
        }
    }
}