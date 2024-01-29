using System.Collections.Generic;

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

        private void AddComponent(ECSEntity entity, bool silently)
        {
            m_EntitiesMap.Add(entity);
            if (!silently)
                GroupAdd?.Invoke(this, entity);
        }

        private void RemoveComponent(ECSEntity entity, bool silently)
        {
            bool b = m_EntitiesMap.Remove(entity);
            if (!silently && b)
                GroupRomve?.Invoke(this, entity);
        }

        public int HandleEntitySilently(ECSEntity entity)
        {
            return DoEntity(entity, true);
        }


        public int HandleEntity(ECSEntity entity)
        {
            return DoEntity(entity, false);
        }

        
        private int DoEntity(ECSEntity entity, bool silently)
        {
            if (this.Matcher.Match(entity))
                this.AddComponent(entity, silently);
            else
                this.RemoveComponent(entity, silently);
            return m_EntitiesMap.Count;
        }


        public void Clear()
        {
            GroupAdd -= GroupAdd;
            GroupRomve -= GroupRomve;
            Matcher = null;
            m_EntitiesMap.Clear();
        }

        public IEnumerable<ECSEntity> AsEnumerable() => (IEnumerable<ECSEntity>) this.EntitiesMap;

        public HashSet<ECSEntity>.Enumerator GetEnumerator() => this.EntitiesMap.GetEnumerator();
    }
}