using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class Group : IDisposable
    {
        private Matcher matcher;
        private HashSet<ECSEntity> entitiesMap;
        public event GroupChanged GroupAdd;
        public event GroupChanged GroupRomve;
        public event GroupChanged GroupUpdate;
        public HashSet<ECSEntity> EntitiesMap => entitiesMap;

        public static Group CreateGroup(Matcher matcher)
        {
            Group Group = ReferencePool.Acquire<Group>();
            Group.matcher = matcher;
            Group.entitiesMap = new(128);
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
                var addSucc = entitiesMap.Add(entity);
                if (addSucc && !silently)
                    GroupAdd?.Invoke(this, entity);
            }
        }


        private void RemoveComponent(ECSEntity entity, bool silently)
        {
            bool b = entitiesMap.Remove(entity);
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
            if (this.matcher.Match(entity))
                this.AddOrUpdateComponent(entity, silently, changeType);
            else
                this.RemoveComponent(entity, silently);
            return entitiesMap.Count;
        }


        public void Dispose()
        {
            GroupAdd -= GroupAdd;
            GroupRomve -= GroupRomve;
            GroupUpdate -= GroupUpdate;
            matcher = null;
            entitiesMap.Clear();
        }

        public IEnumerable<ECSEntity> AsEnumerable() => (IEnumerable<ECSEntity>) this.EntitiesMap;

        public HashSet<ECSEntity>.Enumerator GetEnumerator() => this.EntitiesMap.GetEnumerator();
    }
}