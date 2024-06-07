using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class Collector : IReference
    {
        private HashSet<ECSEntity> m_CollectedEntities;

        public HashSet<ECSEntity> CollectedEntities => m_CollectedEntities;

        private Group[] m_Groups;

        private GroupChanged GroupChange;

        private const ushort AddType = 0;
        private const ushort RemoveType = 1;
        private const ushort UpdateType = 2;

        private ChangeEventState State = 0;

        [Flags]
        public enum ChangeEventState : ushort
        {
            Add = 1 << AddType,
            Remove = 1 << RemoveType,
            Update = 1 << UpdateType,
            AddRemove = 1 << AddType | 1 << RemoveType,
            AddUpdate = 1 << AddType | 1 << UpdateType,
            RemoveUpdate = 1 << RemoveType | 1 << UpdateType,
            AddRemoveUpdate = 1 << AddType | 1 << UpdateType | 1 << RemoveType,
        }


        public static Collector CreateCollector(World world, ChangeEventState stateType, params int[] indexs)
        {
            Group[] groups = new Group[indexs.Length];
            for (int i = 0; i < indexs.Length; i++)
            {
                Matcher matcher = Matcher.SetAll(indexs[i]);
                groups[i] = world.GetGroup(matcher);
            }

            Collector collector = ReferencePool.Acquire<Collector>();
            collector.State = stateType;
            collector.InitCollector(groups);
            return collector;
        }

        private void InitCollector(Group[] group)
        {
            m_Groups = group;
            m_CollectedEntities ??= new HashSet<ECSEntity>();
            GroupChange = AddEvent;
            foreach (var item in m_Groups)
            {
                if ((State & ChangeEventState.Add) != 0)
                {
                    item.GroupAdd += GroupChange;
                }

                if ((State & ChangeEventState.Remove) != 0)
                {
                    item.GroupRomve += GroupChange;
                }

                if ((State & ChangeEventState.Update) != 0)
                {
                    item.GroupUpdate += GroupChange;
                }
                Add(item);
            }
        }

        private void Add(Group group)
        {
            foreach (var item in group.EntitiesMap)
            {
                AddEvent(group, item);
            }
        }

        private void AddEvent(Group group, ECSEntity ecsEntity)
        {
            m_CollectedEntities.Add(ecsEntity);
        }
        
        public void Clear()
        {
            m_CollectedEntities.Clear();
            foreach (var item in m_Groups)
            {
                item.GroupAdd -= GroupChange;
                item.GroupRomve -= GroupChange;
                item.GroupUpdate -= GroupChange;
            }
        }
    }
}