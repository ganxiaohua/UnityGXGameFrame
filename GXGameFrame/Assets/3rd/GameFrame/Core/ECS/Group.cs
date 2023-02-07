using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class Group :IReference
    {
        private HashSet<Entity> m_EntitiesMap;

        public static Group CreateGroup()
        {
            Group Group =  ReferencePool.Acquire<Group>();
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


        public void Clear()
        {
            m_EntitiesMap.Clear();
        }
    }
}