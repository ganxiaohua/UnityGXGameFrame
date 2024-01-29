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
            m_Groups = group;
            m_CollectedEntities ??= new HashSet<ECSEntity>();
            AddChnage ??= this.EventAdd;
            RemoveChnage ??= this.EventAdd;
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

        /// <summary>
        /// 当有组件加入的时候或者更新的时候触发
        /// </summary>
        /// <param name="group"></param>
        /// <param name="ecsEntity"></param>
        private void EventAdd(Group group, ECSEntity ecsEntity)
        {
            m_CollectedEntities.Add(ecsEntity);
        }


        public void Clear()
        {
            m_CollectedEntities.Clear();
            foreach (var item in m_Groups)
            {
                item.GroupAdd -= AddChnage;
            }
        }
    }
}