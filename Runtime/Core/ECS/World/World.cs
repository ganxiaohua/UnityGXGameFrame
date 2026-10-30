using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    /// <summary>
    /// Main-thread entity/component access. Reacquire component refs and pointers after storage
    /// expansion, removal or destruction. Async callers must capture and check Versions after awaits.
    /// </summary>
    public abstract partial class World : IEntity, IVersions, IInitializeSystem<int>, IUpdateSystem, IFixedUpdateSystem, ILateUpdateSystem
    {
        public IEntity Parent { get; private set; }

        public int ID { get; private set; }

        public string Name { get; set; }

        public int Versions { get; private set; }

        public IEntity.EntityState State { get; private set; }

        public bool IsAction => State == IEntity.EntityState.IsRunning;

        public int MaxComponentCount { get; private set; }

        private int ecsSerialId;

        public float DeltaTime { get; private set; }

        public float FixedDeltaTime { get; private set; }

        public float LateDeltaTime { get; private set; }

        public float Multiple { get; private set; }

        public float WorldTime { get; private set; }

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
            if (initialized) throw new InvalidOperationException("World has already been initialized.");
            MaxComponentCount = maxComponentCount;
            WorldTime = 0;
            groupsList = new List<Group>[MaxComponentCount];
            groups = new();
            InitializeChilds();
            SetMultiple(1);
        }

        private void RequireWorld()
        {
            if (!initialized) throw new InvalidOperationException("World is not initialized or has been disposed.");
        }

        protected virtual void SetMultiple(float mul)
        {
            Multiple = mul;
        }

        public Group GetGroup(Matcher matcher)
        {
            RequireWorld();
            if (groups.TryGetValue(matcher, out Group group))
            {
                Matcher.ClearMatcher(matcher);
                return group;
            }

            group = Group.CreateGroup(this, ChildsCount, matcher);
            foreach (var item in Children)
            {
                group.HandleEntitySilently(item);
            }

            groups.Add(matcher, group);
            foreach (var cid in matcher.Indices)
            {
                groupsList[cid] ??= new List<Group>(128);
                groupsList[cid].Add(group);
            }

            return group;
        }

        internal void Reactive(int comid, EffEntityHandle entity)
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


        protected virtual void OnDisposeSystems() { }

        public virtual void Dispose()
        {
            if (disposing || !initialized) return;
            disposing = true;
            try
            {
                DisposeChilds();
                OnDisposeSystems();
            }
            finally
            {
                foreach (var group in groups)
                {
                    Matcher.ClearMatcher(group.Key);
                    Group.RemoveGroup(group.Value);
                }
                Versions++;
                State = IEntity.EntityState.IsClear;
                ecsSerialId = 0;
                groupsList = null;
                groups.Clear();
                groups = null;
                DestroyComp();
                initialized = false;
                Parent = null;
            }
        }

        public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            DeltaTime = elapseSeconds * Multiple;
            WorldTime += DeltaTime;
        }

        public virtual void OnFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            FixedDeltaTime = elapseSeconds * Multiple;
        }

        public virtual void OnLateUpdate(float elapseSeconds, float realElapseSeconds)
        {
            LateDeltaTime = elapseSeconds * Multiple;
        }
    }
}
