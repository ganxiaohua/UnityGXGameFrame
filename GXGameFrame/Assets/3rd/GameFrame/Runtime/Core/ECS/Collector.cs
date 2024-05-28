using System.Collections.Generic;

namespace GameFrame
{
    public class Collector : IReference
    {
        private HashSet<ECSEntity> m_CollectedEntities;

        public HashSet<ECSEntity> CollectedEntities => m_CollectedEntities;

        private Group[] m_Groups;

        private GroupChanged AddChnage;
        private GroupChanged RemoveChnage;

        public static Collector CreateCollector(World world, params int[] indexs)
        {
            Group[] groups = new Group[indexs.Length];
            for (int i = 0; i < indexs.Length; i++)
            {
                Matcher matcher = Matcher.SetAll(indexs[i]);
                groups[i] = world.GetGroup(matcher);
            }

            Collector collector = ReferencePool.Acquire<Collector>();
            collector.InitCollector(groups);
            return collector;
        }

        private void InitCollector(Group[] group)
        {
            m_Groups = group;
            m_CollectedEntities ??= new HashSet<ECSEntity>();
            AddChnage ??= this.EventAdd;
            RemoveChnage ??= this.EventRemove;
            foreach (var item in m_Groups)
            {
                item.GroupAdd -= AddChnage;
                item.GroupAdd += AddChnage;
                item.GroupRomve -= RemoveChnage;
                item.GroupRomve += RemoveChnage;
                Add(item);
            }
        }

        private void Add(Group grop)
        {
            foreach (var item in grop.EntitiesMap)
            {
                m_CollectedEntities.Add(item);
            }
        }
        
        private void EventAdd(Group group, ECSEntity ecsEntity)
        {
            m_CollectedEntities.Add(ecsEntity);
        }
        
        private void EventRemove(Group group, ECSEntity ecsEntity)
        {
            m_CollectedEntities.Remove(ecsEntity);
        }


        public void Clear()
        {
            m_CollectedEntities.Clear();
            foreach (var item in m_Groups)
            {
                item.GroupAdd -= AddChnage;
                item.GroupRomve -= RemoveChnage;
            }
        }
    }
}