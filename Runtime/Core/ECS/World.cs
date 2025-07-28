using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public partial class World : IEntity, IVersions, IInitializeSystem<int>, IUpdateSystem
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

        public float Multiple { get; private set; }

        private Dictionary<Matcher, Group> groups = new();

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
            InitializeChilds();
            SetMultiple(1);
            groupsList = new List<Group>[MaxComponentCount];
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
        }
        
        public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            DeltaTime = elapseSeconds * Multiple;
        }
    }
}