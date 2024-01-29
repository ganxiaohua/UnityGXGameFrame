using System.Collections.Generic;

namespace GameFrame
{
    public class Collector : IReference
    {
        private HashSet<ECSEntity> m_CollectedEntities;
        
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
        }
    }
}