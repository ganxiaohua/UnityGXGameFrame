using System.Collections.Generic;

namespace GameFrame
{
    public partial class World : IInitializeSystem, IUpdateSystem
    {
        private Dictionary<Matcher, Group> groups = new();
        private List<Group>[] groupsList;
        public float DeltaTime { get; private set; }
        public float Multiple { get; private set; }


        public virtual void Initialize()
        {
            SetMultiple(1);
            groupsList = new List<Group>[GXComponents.ComponentTypes.Length];
        }

        protected virtual void SetMultiple(float mul)
        {
            Multiple = mul;
        }


        public Group GetGroup(Matcher matcher)
        {
            if (groups.TryGetValue(matcher, out Group grop))
            {
                Matcher.ClearMatcher(matcher);
                return grop;
            }

            grop = Group.CreateGroup(ChildsCount, matcher);
            foreach (var item in Children)
            {
                grop.HandleEntitySilently((ECSEntity) item, EcsChangeEventState.AddType);
            }

            groups.Add(matcher, grop);
            foreach (var cid in matcher.Indices)
            {
                groupsList[cid] ??= new List<Group>(128);
                groupsList[cid].Add(grop);
            }

            return grop;
        }


        public void Reactive(List<int> indexs, ECSEntity ecsEntity, ushort changeType)
        {
            int count = indexs.Count;
            for (int i = 0; i < count; i++)
            {
                Reactive(indexs[i], ecsEntity, changeType);
            }
        }


        public void Reactive(int comid, ECSEntity entity, ushort changeType)
        {
            var groupList = groupsList[comid];
            if (groupList != null)
            {
                int count = groupList.Count;
                for (int i = 0; i < count; i++)
                {
                    groupList[i].HandleEntity(entity, changeType);
                }
            }
        }

        public virtual void Dispose()
        {
            ClearAllChild();
            foreach (var group in groups)
            {
                Matcher.ClearMatcher(group.Key);
                Group.RemoveGroup(group.Value);
            }

            ecsSerialId = 0;
            groupsList = null;
            groups.Clear();
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            DeltaTime = elapseSeconds * Multiple;
        }
    }
}