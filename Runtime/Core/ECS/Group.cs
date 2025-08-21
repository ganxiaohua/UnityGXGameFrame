using System;
using System.Collections;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public partial class Group : IDisposable
    {
        private Matcher matcher;
        public event GroupChanged GroupAdd;
        public event GroupChanged GroupRomve;
        public event GroupChanged GroupUpdate;
        public GXHashSet<EffEntity> EntitiesMap { get; private set; }

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

        private void AddOrUpdateComponent(EffEntity entity, bool silently)
        {
            EntitiesMap.Add(entity);
            if (!silently)
                GroupAdd?.Invoke(this, entity);
        }


        private void RemoveComponent(EffEntity entity, bool silently)
        {
            bool b = EntitiesMap.Remove(entity);
            if (!silently && b)
                GroupRomve?.Invoke(this, entity);
        }

        public int HandleEntitySilently(EffEntity entity)
        {
            return DoEntity(entity, true);
        }


        public int HandleEntity(EffEntity entity)
        {
            return DoEntity(entity, false);
        }


        private int DoEntity(EffEntity entity, bool silently)
        {
            bool match = this.matcher.Match(entity);
            if (match)
                this.AddOrUpdateComponent(entity, silently);
            else
                this.RemoveComponent(entity, silently);
            return EntitiesMap.Count;
        }

        public override string ToString()
        {
            var sb = StringBuilderCache.Get();
            if (matcher.AllOfIndices != null)
            {
                sb.Append("AllOfIndices:");
                foreach (var value in matcher.AllOfIndices)
                {
                    sb.Append(ComponentsID2Type.ComponentsTypes[value].Name);
                }
            }

            if (matcher.AnyOfIndices != null)
            {
                sb.Append("AnyOfIndices:");
                foreach (var value in matcher.AnyOfIndices)
                {
                    sb.Append(ComponentsID2Type.ComponentsTypes[value].Name);
                }
            }

            if (matcher.NoneOfIndices != null)
            {
                sb.Append("NoneOfIndices:");
                foreach (var value in matcher.NoneOfIndices)
                {
                    sb.Append(ComponentsID2Type.ComponentsTypes[value].Name);
                }
            }

            string str = sb.ToString();
            StringBuilderCache.Release(sb);
            return str;
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
        public IEnumerator<EffEntity> GetEnumerator() => EntitiesMap.GetEnumerator();
        
    }
}