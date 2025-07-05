using System;
using System.Collections;
using System.Collections.Generic;

namespace GameFrame
{
    public partial class Group : IDisposable, IEnumerable<ECSEntity>
    {
        private Matcher matcher;
        public event GroupChanged GroupAdd;
        public event GroupChanged GroupRomve;
        public event GroupChanged GroupUpdate;
        public HashSet<ECSEntity> EntitiesMap { get; private set; }

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

        private void AddOrUpdateComponent(ECSEntity entity, bool silently)
        {
            EntitiesMap.Add(entity);
            if (!silently)
                GroupAdd?.Invoke(this, entity);
        }


        private void RemoveComponent(ECSEntity entity, bool silently)
        {
            bool b = EntitiesMap.Remove(entity);
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
            bool match = this.matcher.Match(entity);
            if (match)
                this.AddOrUpdateComponent(entity, silently);
            else
                this.RemoveComponent(entity, silently);
            return EntitiesMap.Count;
        }

        IEnumerator<ECSEntity> IEnumerable<ECSEntity>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            var sb = StringBuilderCache.Get();
            if (matcher.AllOfIndices != null)
            {
                sb.Append("AllOfIndices:");
                foreach (var value in matcher.AllOfIndices)
                {
                    sb.Append(GXComponents.ComponentTypes[value].Name);
                }
            }

            if (matcher.AnyOfIndices != null)
            {
                sb.Append("AnyOfIndices:");
                foreach (var value in matcher.AnyOfIndices)
                {
                    sb.Append(GXComponents.ComponentTypes[value].Name);
                }
            }

            if (matcher.NoneOfIndices != null)
            {
                sb.Append("NoneOfIndices:");
                foreach (var value in matcher.NoneOfIndices)
                {
                    sb.Append(GXComponents.ComponentTypes[value].Name);
                }
            }

            string str = sb.ToString();
            StringBuilderCache.Release(sb);
            return str;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void Dispose()
        {
            GroupAdd -= GroupAdd;
            GroupRomve -= GroupRomve;
            GroupUpdate -= GroupUpdate;
            matcher = null;
            EntitiesMap.Clear();
#if UNITY_EDITOR
            EditorDisPose();
#endif
        }
        
        public GroupEnumerator GetEnumerator() => new GroupEnumerator(EntitiesMap);
        
        public struct GroupEnumerator : IEnumerator<ECSEntity>
        {
            private HashSet<ECSEntity>.Enumerator hashSetEnumerator;
            public GroupEnumerator(HashSet<ECSEntity> set) => hashSetEnumerator = set.GetEnumerator();
            public ECSEntity Current => hashSetEnumerator.Current;
            object IEnumerator.Current => Current;
            public bool MoveNext() => hashSetEnumerator.MoveNext();

            public void Reset() { }

            public void Dispose() => hashSetEnumerator.Dispose();
        }
    }
}