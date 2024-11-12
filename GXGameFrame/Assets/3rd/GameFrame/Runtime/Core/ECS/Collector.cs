using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class Collector : IDisposable
    {
        private HashSet<ECSEntity> collectedEntities;

        public HashSet<ECSEntity> CollectedEntities => collectedEntities;

        private Group[] groups;

        private GroupChanged groupChange;

        private const ushort AddType = 0;
        private const ushort RemoveType = 1;
        private const ushort UpdateType = 2;

        private ChangeEventState state = 0;

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
            collector.state = stateType;
            collector.InitCollector(groups);
            return collector;
        }
        

        private void InitCollector(Group[] group)
        {
            groups = group;
            collectedEntities ??= new HashSet<ECSEntity>();
            groupChange = AddEvent;
            foreach (var item in groups)
            {
                if ((state & ChangeEventState.Add) != 0)
                {
                    item.GroupAdd += groupChange;
                }

                if ((state & ChangeEventState.Remove) != 0)
                {
                    item.GroupRomve += groupChange;
                }

                if ((state & ChangeEventState.Update) != 0)
                {
                    item.GroupUpdate += groupChange;
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
            collectedEntities.Add(ecsEntity);
        }
        
        public void Dispose()
        {
            collectedEntities.Clear();
            foreach (var item in groups)
            {
                item.GroupAdd -= groupChange;
                item.GroupRomve -= groupChange;
                item.GroupUpdate -= groupChange;
            }
        }
    }
}