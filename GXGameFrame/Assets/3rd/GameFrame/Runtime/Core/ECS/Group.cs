using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class Group : IDisposable
    {
        private Matcher matcher;
        public event GroupChanged GroupAdd;
        public event GroupChanged GroupRomve;
        public event GroupChanged GroupUpdate;
        public FastSoleList<ECSEntity> EntitiesMap { get; private set; }

        public static Group CreateGroup(int childsCount, Matcher matcher)
        {
            Group Group = ReferencePool.Acquire<Group>();
            Group.matcher = matcher;
            Group.EntitiesMap = new(childsCount);
            return Group;
        }

        public static void RemoveGroup(Group group)
        {
            ReferencePool.Release(group);
        }

        private void AddOrUpdateComponent(ECSEntity entity, bool silently, ushort changeType)
        {
            if (changeType == EcsChangeEventState.UpdateType)
            {
                if (!silently)
                    GroupUpdate?.Invoke(this, entity);
            }
            else if (changeType == EcsChangeEventState.AddType)
            {
                var addSucc = EntitiesMap.Add(entity);
                if (addSucc && !silently)
                    GroupAdd?.Invoke(this, entity);
            }
        }


        private void RemoveComponent(ECSEntity entity, bool silently)
        {
            bool b = EntitiesMap.Remove(entity);
            if (!silently && b)
                GroupRomve?.Invoke(this, entity);
        }

        public int HandleEntitySilently(ECSEntity entity, ushort changeType)
        {
            return DoEntity(entity, true, changeType);
        }


        public int HandleEntity(ECSEntity entity, ushort changeType)
        {
            return DoEntity(entity, false, changeType);
        }


        private int DoEntity(ECSEntity entity, bool silently, ushort changeType)
        {
            bool match = this.matcher.Match(entity);
            if (changeType != EcsChangeEventState.RemoveType && match)
                this.AddOrUpdateComponent(entity, silently, changeType);
            else if (changeType == EcsChangeEventState.RemoveType && !match)
                this.RemoveComponent(entity, silently);
            return EntitiesMap.Count;
        }


        public void Dispose()
        {
            GroupAdd -= GroupAdd;
            GroupRomve -= GroupRomve;
            GroupUpdate -= GroupUpdate;
            matcher = null;
            EntitiesMap.Clear();
        }

        public IEnumerable<ECSEntity> AsEnumerable() => (IEnumerable<ECSEntity>) this.EntitiesMap;

        public List<ECSEntity>.Enumerator GetEnumerator() => this.EntitiesMap.GetEnumerator();
    }
}