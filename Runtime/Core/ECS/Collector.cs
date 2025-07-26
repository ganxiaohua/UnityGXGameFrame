using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class Collector : IDisposable
    {
        private GXHashSet<ECSEntity> collectedEntities;

        public GXHashSet<ECSEntity> CollectedEntities => collectedEntities;

        private Group[] groups;

        private GroupChanged groupChange;


        private EcsChangeEventState.ChangeEventState state = 0;

        

        public static Collector CreateCollector(World world, EcsChangeEventState.ChangeEventState stateType, params int[] indexs)
        {
            Group[] groups = new Group[indexs.Length];
            for (int i = 0; i < indexs.Length; i++)
            {
                Matcher matcher = Matcher.SetAll(indexs[i]);
                groups[i] = world.GetGroup(matcher);
            }

            Collector collector = ReferencePool.Acquire<Collector>();
            collector.state = stateType;
            collector.InitCollector(world.ChildsCount, groups);
            return collector;
        }


        private void InitCollector(int childSize, Group[] group)
        {
            groups = group;
            collectedEntities ??= new GXHashSet<ECSEntity>(childSize);
            groupChange = AddEvent;
            foreach (var item in groups)
            {
                if ((state & EcsChangeEventState.ChangeEventState.Add) != 0)
                {
                    item.GroupAdd += groupChange;
                }

                if ((state & EcsChangeEventState.ChangeEventState.Remove) != 0)
                {
                    item.GroupRomve += groupChange;
                }

                if ((state & EcsChangeEventState.ChangeEventState.Update) != 0)
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