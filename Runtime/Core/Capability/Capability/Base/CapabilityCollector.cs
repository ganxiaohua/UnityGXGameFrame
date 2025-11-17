using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class CapabilityCollector : IDisposable
    {
        private Group[] groups;

        private GroupChanged groupChange;

        private CapabilityBase capability;

        public EcsChangeEventState.ChangeEventState State = EcsChangeEventState.ChangeEventState.AddRemoveUpdate;

        public static CapabilityCollector CreateCollector(World world, CapabilityBase capability, params int[] indexs)
        {
            Group[] groups = new Group[indexs.Length];
            for (int i = 0; i < indexs.Length; i++)
            {
                Matcher matcher = Matcher.SetAll(indexs[i]);
                groups[i] = world.GetGroup(matcher);
            }

            CapabilityCollector collector = ReferencePool.Acquire<CapabilityCollector>();
            collector.capability = capability;
            collector.InitCollector(groups);
            return collector;
        }

        public static void Release(CapabilityCollector capabilityCollector)
        {
            ReferencePool.Release(capabilityCollector);
        }


        private void InitCollector(Group[] group)
        {
            groups = group;
            groupChange = AddEvent;
            foreach (var item in groups)
            {
                if ((State & EcsChangeEventState.ChangeEventState.Add) != 0)
                {
                    item.GroupAdd += groupChange;
                }

                if ((State & EcsChangeEventState.ChangeEventState.Remove) != 0)
                {
                    item.GroupRomve += groupChange;
                }

                if ((State & EcsChangeEventState.ChangeEventState.Update) != 0)
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

        private void AddEvent(Group group, EffEntity effEntity)
        {
            capability.ComponentChanges = true;
        }

        public void Dispose()
        {
            foreach (var item in groups)
            {
                item.GroupAdd -= groupChange;
                item.GroupRomve -= groupChange;
                item.GroupUpdate -= groupChange;
            }

            groups = null;
        }
    }
}