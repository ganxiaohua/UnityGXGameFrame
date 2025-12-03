using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Runtime
{
    public abstract partial class World : IEntity, IVersions, IInitializeSystem<int>, IUpdateSystem, IFixedUpdateSystem
    {
        public IEntity Parent { get; private set; }

        public int ID { get; private set; }

        public string Name { get; set; }

        public int Versions { get; private set; }

        public IEntity.EntityState State { get; private set; }

        public bool IsAction => State == IEntity.EntityState.IsRunning;

        public int MaxComponentCount { get; private set; }

        private int ecsSerialId;

        protected float DeltaTime { get; private set; }

        protected float FixedDeltaTime { get; private set; }

        public float Multiple { get; private set; }

        private Dictionary<Matcher, Group> groups;

        private List<Group>[] groupsList;

        private int sIndex;


        public void OnDirty(IEntity parent, int id)
        {
            State = IEntity.EntityState.IsRunning;
            Parent = parent;
            ecsSerialId = 0;
            ID = id;
            Versions++;
        }

        public virtual void OnInitialize(int maxComponentCount)
        {
            MaxComponentCount = maxComponentCount;
            groupsList = new List<Group>[MaxComponentCount];
            groups = new();
            InitializeChilds();
            SetMultiple(1);
        }

        protected virtual void SetMultiple(float mul)
        {
            Multiple = mul;
        }

        public Group GetGroup(Matcher matcher)
        {
            if (groups.TryGetValue(matcher, out Group group))
            {
                Matcher.ClearMatcher(matcher);
                return group;
            }

            group = Group.CreateGroup(ChildsCount, matcher);
            foreach (var item in Children)
            {
                group.HandleEntitySilently((EffEntity) item);
            }

            groups.Add(matcher, group);
            foreach (var cid in matcher.Indices)
            {
                groupsList[cid] ??= new List<Group>(128);
                groupsList[cid].Add(group);
            }

            return group;
        }

        public void Reactive(int comid, EffEntity entity)
        {
            var groupList = groupsList[comid];
            if (groupList != null)
            {
                int count = groupList.Count;
                for (int i = 0; i < count; i++)
                {
                    groupList[i].HandleEntity(entity);
                }
            }
        }


        public virtual void Dispose()
        {
            DisposeChilds();
            foreach (var group in groups)
            {
                Matcher.ClearMatcher(group.Key);
                Group.RemoveGroup(group.Value);
            }

            Versions++;
            ecsSerialId = 0;
            groupsList = null;
            groups.Clear();
            groups = null;
        }

        public virtual void OnFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            FixedDeltaTime = elapseSeconds * Multiple;
        }

        public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            DeltaTime = elapseSeconds * Multiple;
        }
    }
}